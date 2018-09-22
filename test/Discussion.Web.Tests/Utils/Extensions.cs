using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using Discussion.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Discussion.Web.Tests
{
    public static class Extensions
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
        
        public static T GetService<T>(this HttpContext httpContext) where T : class
        {
            return httpContext.RequestServices.GetService<T>();
        }
                
        public static T GetService<T>(this Controller controller) where T : class
        {
            return controller.HttpContext.RequestServices.GetService<T>();
        }
        
        public static void MockUser(this TestApplication app)
        {
            var userId = 1;
            var userName = "FancyUser";
            var lastSigninTime = DateTime.UtcNow.AddMinutes(-30);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString(), ClaimValueTypes.Integer32),
                new Claim(ClaimTypes.Name, userName, ClaimValueTypes.String),
                new Claim("SigninTime", lastSigninTime.Ticks.ToString(), ClaimValueTypes.Integer64)
            };
            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            app.User = new DiscussionPrincipal(identity)
            {
                User = new User
                {
                    Id = userId,
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
                    DisplayName = "Fancy User",
                    LastSeenAt = lastSigninTime,
                    UserName = userName
                }
            };
        }
        
        public static TController CreateControllerAndValidate<TController>(this TestApplication app, object model) where TController: Controller
        {
            var controller = app.CreateController<TController>();
            controller.TryValidateModel(model, null);
            return controller;
        }
        
        
        public static IEnumerable<StubLoggerProvider.LogItem> GetLogs(this TestApplication app)
        {
            var loggerProvider = app.ApplicationServices.GetRequiredService<ILoggerProvider>() as StubLoggerProvider;
            return loggerProvider?.LogItems;
        }
        
        public static string Content(this HttpResponseMessage response)
        {
            return response.Content.ReadAsStringAsync().Result;
        }
    }
}