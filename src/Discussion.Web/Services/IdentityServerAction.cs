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
    public class IdentityServerAction : Attribute, IActionFilter
    {
        private readonly IdentityAction _action;

        public IdentityServerAction(IdentityAction action)
        {
            _action = action;
        }


        public void OnActionExecuting(ActionExecutingContext context)
        {
            var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
            var idConfig = configuration.GetSection(nameof(IdentityServerOptions));
            var idsEnable = bool.Parse(idConfig[nameof(IdentityServerOptions.IsEnable)]);
            if (idsEnable)
            {
                switch (_action)
                {
                    case IdentityAction.Signin:
                        if (!context.HttpContext.IsAuthenticated())
                            context.Result = new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme);
                        break;
                    case IdentityAction.SignOut:
                        context.Result = new SignOutResult(new List<string>
                            {
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                OpenIdConnectDefaults.AuthenticationScheme
                            },
                            new AuthenticationProperties {RedirectUri = "/"});
                        break;
                    case IdentityAction.Register:
                        context.Result = new RedirectResult(idConfig[nameof(IdentityServerOptions.RegisterUri)]);
                        break;
                    case IdentityAction.Settings:
                        context.Result = new RedirectResult(idConfig[nameof(IdentityServerOptions.SettingsUri)]);
                        break;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}