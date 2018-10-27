using System.Net;
using Discussion.Core.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Discussion.Admin.Supporting
{
    public static class ResponseExtensions
    {
        public static void UseApiResponseExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    if (context.Response.HasStarted)
                    {
                        return;
                    }

                    // Still use HttpStatus OK on server errors to prevent a redirection on the SPA page
                    context.Response.StatusCode = (int)HttpStatusCode.OK;

                    context.Response.ContentType = "application/json";
                    var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
                    if (exceptionHandler != null)
                    {
                        var result = ApiResponse.Error(exceptionHandler.Error);
                        var json = JsonConvert.SerializeObject(result, new JsonSerializerSettings
                        {
                            ContractResolver = Startup.JsonContractResolver
                        });
                        await context.Response.WriteAsync(json).ConfigureAwait(false);
                    }
                });
            });
        }
    }
}