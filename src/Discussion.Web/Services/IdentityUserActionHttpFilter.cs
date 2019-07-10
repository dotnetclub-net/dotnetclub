using System;
using System.Collections.Generic;
using System.Linq;
using Discussion.Core.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
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
            if (!idsEnable) return;
            
            
            switch (_userAction)
            {
                case IdentityUserAction.Signin:
                    if (!httpContext.IsAuthenticated())
                    {
                        var externalSigninResult = httpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme).Result;
                        if (!externalSigninResult.Succeeded)
                        {
                            context.Result = new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme);
                        }
                    }
                    break;
                case IdentityUserAction.SignOut:
                    var redirectUrl = "/";
                    if (httpContext.Request.Query.TryGetValue("returnUrl", out var returnUrl))
                    {
                        redirectUrl = returnUrl.FirstOrDefault();
                    }

                    context.Result = new SignOutResult(new List<string>
                        {
                            IdentityConstants.ApplicationScheme,
                            IdentityConstants.ExternalScheme,
                            OpenIdConnectDefaults.AuthenticationScheme
                        },
                        new AuthenticationProperties
                        {
                            RedirectUri = redirectUrl
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

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}