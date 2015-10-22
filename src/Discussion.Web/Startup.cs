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
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

        }

        public void Configure(IApplicationBuilder app)
        {
            // Add the platform handler to the request pipeline.
            app.UseIISPlatformHandler();


        }
    }
}
