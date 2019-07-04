using System;
using System.Collections.Generic;
using Discussion.Core.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Web.Services
{
    public class IdentityUserActionHttpFilter : Attribute, IActionFilter
    {
        private readonly IdentityUserAction _userAction;

        public IdentityUserActionHttpFilter(IdentityUserAction userAction)
        {
            _userAction = userAction;
        }


        public void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var configuration = httpContext.RequestServices.GetService<IConfiguration>();
            var idConfig = configuration.GetSection(nameof(IdentityServerOptions));
            var idsEnable = bool.Parse(idConfig[nameof(IdentityServerOptions.IsEnabled)]);
            if (idsEnable)
            {
                switch (_userAction)
                {
                    case IdentityUserAction.Signin:
                        if (!httpContext.IsAuthenticated())
                            context.Result = new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme);
                        break;
                    case IdentityUserAction.SignOut:
                        context.Result = new SignOutResult(new List<string>
                            {
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                OpenIdConnectDefaults.AuthenticationScheme
                            },
                            new AuthenticationProperties
                            {
                                RedirectUri = new UriBuilder(httpContext.Request.Scheme, httpContext.Request.Host.Host).ToString()
                            });
                        break;
                    case IdentityUserAction.Register:
                        context.Result = new RedirectResult(idConfig[nameof(IdentityServerOptions.RegisterUri)]);
                        break;
                    case IdentityUserAction.ChangePassword:
                        context.Result = new RedirectResult(idConfig[nameof(IdentityServerOptions.ChangePasswordUri)]);
                        break;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}