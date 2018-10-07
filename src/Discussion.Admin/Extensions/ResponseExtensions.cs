using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Discussion.Admin.Extensions
{
    public static class ResponseExtensions
    {
        public static void UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.ContentType = "application/json";
                    IExceptionHandlerFeature error = context.Features.Get<IExceptionHandlerFeature>();
                    if (error != null)
                    {
                        var result = new { status = -1, message = error.Error.Message };
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(result)).ConfigureAwait(false);
                    }
                });
            });
        }
    }
}