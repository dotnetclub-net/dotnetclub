using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Discussion.Admin.Services;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Discussion.Tests.Common
{
    public static class TestApplicationExtensions
    {
        public static T CreateController<T>(this TestApplication app) where T : ControllerBase
        {
            var httpContext = app.GetService<IHttpContextFactory>().Create(new DefaultHttpContext().Features);
            httpContext.User = app.User;

            var routeData = new RouteData();
            routeData.Routers.Add(new FakeRoute(app.GetService<IInlineConstraintResolver>()));

            var actionContext = new ActionContext(
                httpContext,
                routeData, 
                new ControllerActionDescriptor
                {
                    ControllerTypeInfo = typeof(T).GetTypeInfo()
                });

            var actionContextAccessor = app.GetService<IActionContextAccessor>();
            if (actionContextAccessor != null)
            {
                actionContextAccessor.ActionContext = actionContext;
            }
            return app.GetService<IControllerFactory>().CreateController(new ControllerContext(actionContext)) as T;
        }

        public static T GetService<T>(this TestApplication app) where T : class
        {
            return app.ApplicationServices.GetService<T>();
        }

        public static void DeleteAll<T>(this TestApplication app) where T: Entity
        {
            var repo = app.GetService<IRepository<T>>();
            repo.All().ToList().ForEach(topic => repo.Delete(topic));
        }

        public static void ReloadEntity<T>(this TestApplication app, params T[] entities) where T: Entity
        {
            var applicationDbContext = app.GetService<ApplicationDbContext>();
            entities.ToList().ForEach(entity =>
            {
                applicationDbContext.Entry(entity).Reload();
            });
        }
        
        public static User MockUser(this TestApplication app)
        {
            var userRepo = app.GetService<IRepository<User>>();
            var passwordHasher = app.GetService<IPasswordHasher<User>>();

            var user = new User
            {
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
                UserName = "FancyUser",
                DisplayName = "FancyUser",
                HashedPassword = passwordHasher.HashPassword(null, "111111")
            };
            userRepo.Save(user);
            
            var lastSigninTime = DateTime.UtcNow.AddMinutes(-30);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.Integer32),
                new Claim(ClaimTypes.Name, user.UserName, ClaimValueTypes.String),
                new Claim("SigninTime", lastSigninTime.Ticks.ToString(), ClaimValueTypes.Integer64)
            };
            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            app.User = new ClaimsPrincipal(identity);
            return user;
        }
        
        public static User NoUser(this TestApplication app)
        {
            app.ResetUser();
            return null;
        }
        
        public static AdminUser MockAdminUser(this TestApplication app)
        {
            var adminUserRepo = app.GetService<IRepository<AdminUser>>();
            var adminUserService = app.GetService<IAdminUserService>();

            var adminUser = new AdminUser
            {
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
                Username = "AdminUser",
                HashedPassword = adminUserService.HashPassword("11111A")
            };
            adminUserRepo.Save(adminUser);

            var token = adminUserService.IssueJwtToken(adminUser);
            var options =  app.GetService<IOptionsMonitor<JwtBearerOptions>>().Get("Bearer");
            var identity = options.SecurityTokenValidators
                                  .First()
                                  .ValidateToken(token.TokenString, options.TokenValidationParameters, out _);
            app.User = identity;
            return adminUser;
        }
        
        public static ModelStateDictionary ValidateModel(this TestApplication app, object model)
        {
            var validator = app.GetService<IObjectModelValidator>();
            var actionContext = new ActionContext();
            
            validator.Validate(actionContext, null, string.Empty, model);
            
            return actionContext.ModelState;
        }
        
        public static IEnumerable<StubLoggerProvider.LogItem> GetLogs(this TestApplication app)
        {
            var loggerProvider = app.ApplicationServices.GetRequiredService<ILoggerProvider>() as StubLoggerProvider;
            return loggerProvider?.LogItems;
        }

        public static User CreateUser(this TestApplication app, string username = null, string password = null, string displayName = null)
        {
            var actualPassword = string.IsNullOrEmpty(password) ? StringUtility.Random() : password;
            var userManager = app.GetService<UserManager<User>>();

            if (username == null)
            {
                username = StringUtility.Random();
            }
            
            var user = new User
            {
                UserName = username,
                DisplayName = string.IsNullOrEmpty(displayName) ? username : displayName,
                CreatedAtUtc = DateTime.UtcNow
            };
            
            var task = userManager.CreateAsync(user, actualPassword);
            task.ConfigureAwait(false);

            var createResult = task.Result;
            if (!createResult.Succeeded)
            {
                var errorMessage = string.Join(";", createResult.Errors.Select(err => err.Code));
                throw new Exception("不能创建用户：" + errorMessage);
            }

            return user;
        }


        public class FakeRoute : RouteBase
        {
            private List<VirtualPathContext> _urlGeneratingList = new List<VirtualPathContext>();
            public FakeRoute(IInlineConstraintResolver constraintResolver) : base("", "foo", constraintResolver, null, null, null)
            {
            }

            protected override Task OnRouteMatched(RouteContext context)
            {
                return Task.CompletedTask;
            }

            protected override VirtualPathData OnVirtualPathGenerated(VirtualPathContext context)
            {
                _urlGeneratingList.Add(context);
                return null;
            }

            public RouteValueDictionary GetGeneratedUrl => _urlGeneratingList.FirstOrDefault()?.Values;

            public List<RouteValueDictionary> GetGeneratedUrls
            {
                get { return _urlGeneratingList.Select(x => x.Values).ToList(); }
            }
        }
        
        
    }
}