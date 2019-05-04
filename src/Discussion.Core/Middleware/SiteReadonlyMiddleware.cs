using System.Diagnostics;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Core.Middleware
{
    public class SiteReadonlyMiddleware
    {
        private readonly RequestDelegate _next;

        public SiteReadonlyMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        
        public async Task Invoke(HttpContext ctx)
        {
            var readonlyDataSettings = ctx.RequestServices.GetService<IReadonlyDataSettings>() as ReadonlyDataSettings;
            Debug.Assert(readonlyDataSettings != null, nameof(readonlyDataSettings) + " != null");

            var siteSettings = ctx.RequestServices.GetService<SiteSettings>();
            readonlyDataSettings.IsReadonly = siteSettings.IsReadonly;
            
            await _next(ctx);
        }
    }
}