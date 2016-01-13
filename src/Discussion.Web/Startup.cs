using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;

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
            app.UseMvc();

        }
    }
}
