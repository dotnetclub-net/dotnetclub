using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Time;
using Discussion.Tools.MigrationToIds4.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Discussion.Tools.MigrationToIds4
{
    class Program
    {
        /// <summary>
        /// 加载配置文件，构建IConfigurationRoot
        /// </summary>
        private static readonly IConfigurationBuilder ConfigurationBuilder = new ConfigurationBuilder();

        /// <summary>
        /// 获取配置文件中的内容，继承自IConfiguration
        /// </summary>
        private static IConfigurationRoot _configuration;

        static void Main()
        {
            _configuration = ConfigurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(cfg =>
                {
                    cfg.Path = "appsettings.json";
                    cfg.ReloadOnChange = true;
                    cfg.Optional = false;
                })
                .Build();

            var connectionString = _configuration["SqliteConnectionString"];
            var idsConnectionString = _configuration["SqlServerConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("数据源SqlLite数据库连接字符串不能空！");
                Console.ReadLine();
                return;
            }

            if (string.IsNullOrEmpty(idsConnectionString))
            {
                Console.WriteLine("目标SqlServer数据库连接字符串不能空！");
                Console.ReadLine();
                return;
            }

            Log("开始读取数据源用户数据...");

            IList<User> users = GetCurrentUsers(connectionString);

            Log($"读取数据源用户数据结束！共 {users.Count.ToString()} 名用户.");

            Log("开始导入数据...");

            var result = ImportToSqlServer(idsConnectionString, users);

            Log($"用户数据导入结束,共导入 {result.Result.ToString()} 名用户! 按Enter键结束！");

            Console.ReadLine();
        }

        static List<User> GetCurrentUsers(string connectionString)
        {
            var optionBuilder =
                new DbContextOptionsBuilder<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var appContext = new ApplicationDbContext(optionBuilder.UseSqlite(connectionString).Options);
            IClock clock = new SystemClock();
            IReadonlyDataSettings readonlyDataSettings = new ReadonlyDataSettings();
            using (appContext)
            {
                var userRepo = new EfRepository<User>(appContext, clock, readonlyDataSettings);

                return userRepo.All().Include(i => i.VerifiedPhoneNumber).ToList();
            }
        }

        static async Task<int> ImportToSqlServer(string connectionString, ICollection<User> users)
        {
            if (users == null || users.Count == 0)
            {
                Log("没有需要导入的用户!");
                return 0;
            }

            var idsOptionBuilder =
                new DbContextOptionsBuilder<IdsContext>(new DbContextOptions<IdsContext>());

            var idsContext = new IdsContext(idsOptionBuilder.UseSqlServer(connectionString).Options);
            var idsUsers = new List<IdentityUsers>(users.Count);

            using (idsContext)
            {
                foreach (var user in users)
                {
                    var isExsit = await idsContext.IdentityUsers.AnyAsync(i => i.UserName == user.UserName);
                    if (isExsit)
                    {
                        Log($"用户 {user.UserName} 已存在！不再导入！");
                        continue;
                    }

                    var idsUser = MapToAspNetUser(user);
                    idsUsers.Add(idsUser);
                }

                await idsContext.IdentityUsers.AddRangeAsync(idsUsers);

                return await idsContext.SaveChangesAsync();
            }
        }


        static IdentityUsers MapToAspNetUser(User user)
        {
            var identityUser = new IdentityUsers
            {
                Id = Guid.NewGuid().ToString(),
                Email = user.EmailAddress,
                EmailConfirmed = user.EmailAddressConfirmed,
                NormalizedEmail = user.EmailAddress?.ToUpper(),
                UserName = user.UserName,
                NormalizedUserName = user.UserName?.ToUpper(),
                PasswordHash = user.HashedPassword,
                PhoneNumber = user.VerifiedPhoneNumber?.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberId.HasValue
            };
            return identityUser;
        }

        static void Log(string log)
        {
            Console.WriteLine($"{DateTime.Now.ToString()}:\t{log}");
        }
    }
}