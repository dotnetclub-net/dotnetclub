using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Discussion.Core.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Discussion.Admin.Supporting
{
    public static class ResponseExtensions
    {
        public static void UseUnifiedApiResponse(this IApplicationBuilder app)
        {
            app.Use(WrapAllResponse);
            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    if (context.Response.HasStarted)
                    {
                        return;
                    }

                    // Still use HttpStatus OK on server errors to prevent a redirection on the SPA page
                    context.Response.StatusCode = (int) HttpStatusCode.OK;

                    context.Response.ContentType = "application/json";
                    var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
                    if (exceptionHandler != null)
                    {
                        var result = ApiResponse.Error(exceptionHandler.Error);
                        var json = SerializeToJson(result);
                        await context.Response.WriteAsync(json).ConfigureAwait(false);
                    }
                });
            });
        }


        private static async Task WrapAllResponse(HttpContext httpContext, Func<Task> next)
        {
            var isApiRequest = httpContext.Request.Path.ToString().ToLowerInvariant().StartsWith("/api/");
            if (!isApiRequest)
            {
                await next();
                return;
            }
            
            var originalStream = httpContext.Response.Body;
            using (var memoryStream = new MemoryStream())
            {
                httpContext.Response.Body = memoryStream;
                
                await next();
                memoryStream.Seek(0, SeekOrigin.Begin);

                if (ShouldNotWrap(httpContext))
                {
                    await WriteToResponseAsync(httpContext, memoryStream, originalStream);
                    return;
                }
                
                var allContent = new StreamReader(memoryStream).ReadToEnd();
                var apiResult = ApiResponse.NoContent(httpContext.Response.StatusCode);
                if (!string.IsNullOrEmpty(allContent))
                {
                    apiResult.Result = allContent;
                }

                if (!httpContext.Response.HasStarted)
                {
                    httpContext.Response.StatusCode = 200;
                    httpContext.Response.ContentType = "application/json";
                }
                httpContext.Response.Body = originalStream;
                await httpContext.Response.WriteAsync(SerializeToJson(apiResult));
            }
        }

        private static bool ShouldNotWrap(HttpContext httpContext)
        {
            var contentType = httpContext.Response.ContentType;
            var hasContentType = contentType != null;
            var isAlreadyJson = hasContentType && contentType.Contains("json");
            var isNotText = hasContentType && !contentType.StartsWith("text/");

            return httpContext.Response.StatusCode < 400 || isAlreadyJson || isNotText;
        }

        private static async Task WriteToResponseAsync(HttpContext httpContext, MemoryStream contentStream, Stream responseStream)
        {
            httpContext.Response.Body = responseStream;
            using (var bufferedStream = new BufferedStream(contentStream))
            {
                await bufferedStream.CopyToAsync(httpContext.Response.Body);
            }
        }
        
        private static string SerializeToJson(ApiResponse result)
        {
            var json = JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                ContractResolver = Startup.JsonContractResolver
            });
            return json;
        }
    }
}