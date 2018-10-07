using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Discussion.Admin.Extensions
{
    public static class ResponseExtensions
    {
        public static void UseJsonExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    // 在异常出现时，仍使用 200 的响应以防止前端 SPA 出现强制的 500 重定向 
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    
                    context.Response.ContentType = "application/json";
                    var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
                    if (exceptionHandler != null)
                    {
                        var result = new { status = -1, message = exceptionHandler.Error?.Message };
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(result)).ConfigureAwait(false);
                    }
                });
            });
        }
    }
}