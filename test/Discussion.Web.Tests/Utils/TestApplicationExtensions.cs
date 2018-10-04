using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Web.Services.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Web.Tests
{
    public static class TestApplicationExtensions
    {
                
        public static T CreateController<T>(this TestApplication app) where T : Controller
        {
            var httpContext = app.GetService<IHttpContextFactory>().Create(new DefaultHttpContext().Features);
            httpContext.User = app.User;
            
            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ControllerActionDescriptor
                {
                    ControllerTypeInfo = typeof(T).GetTypeInfo()
                });

            return app.GetService<IControllerFactory>()
                .CreateController(new ControllerContext(actionContext)) as T;
        }

        public static T GetService<T>(this TestApplication app) where T : class
        {
            return app.ApplicationServices.GetService<T>();
        }
        
        public static void MockUser(this TestApplication app)
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
        }
        
        public static User GetDiscussionUser(this TestApplication app)
        {
            var userRepo = app.GetService<IRepository<User>>();
            return app.User.ToDiscussionUser(userRepo);
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

        public static User CreateUser(this TestApplication app, string username, string password = null, string displayName = null)
        {
            var actualPassword = string.IsNullOrEmpty(password) ? Guid.NewGuid().ToString("N").Substring(4, 8) : password;
            var userManager = app.GetService<UserManager<User>>();
            var user = new User
            {
                UserName = username,
                DisplayName = string.IsNullOrEmpty(displayName) ? username : displayName,
                CreatedAtUtc = DateTime.UtcNow
            };
            
            var task = userManager.CreateAsync(user, actualPassword);
            task.ConfigureAwait(false);
            task.Wait();

            return user;
        }
        
        
        
        
    }
}