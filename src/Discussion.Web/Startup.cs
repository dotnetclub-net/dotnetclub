using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;

namespace Discussion.Web
{
    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app)
        {
            // Add the platform handler to the request pipeline.
            app.UseIISPlatformHandler();

            app.Run(async (context) =>
            {
                await WriteAssemblyInfomationAsync(context);
                await context.Response.WriteAsync("Hello World haha!");
            });
        }

        private async Task WriteAssemblyInfomationAsync(HttpContext context)
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            var infomation = new Dictionary<string, string>
            {
                { "FullName", assembly.FullName },
                { "TypeName", typeof(Startup).FullName },
            };


            foreach (var key in infomation.Keys)
            {
                await context.Response.WriteAsync( key + ":" + infomation[key] );
            }
        }
    }
}
