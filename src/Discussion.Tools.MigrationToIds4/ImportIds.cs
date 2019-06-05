using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Time;
using Microsoft.EntityFrameworkCore;

namespace Discussion.Tools.MigrationToIds4
{
    public class ImportIds
    {
        public void init()
        {
            var connectionString = "";
            
            var optionBuilder = new DbContextOptionsBuilder<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

            var appContext = new ApplicationDbContext(optionBuilder.UseSqlServer(connectionString).Options);
            
            IClock clock = new SystemClock();
            IReadonlyDataSettings  readonlyDataSettings = new ReadonlyDataSettings();
            
            var userRepo = new EfRepository<User>(appContext, clock, readonlyDataSettings);
        }
        
        public bool Import()
        {
            return false;
        }
    }
}