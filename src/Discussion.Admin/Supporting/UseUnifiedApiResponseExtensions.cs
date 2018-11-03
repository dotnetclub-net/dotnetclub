using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discussion.Core.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Discussion.Admin.Supporting
{
    public static class UnifiedApiResponseExtensions
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

                    var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
                    if (exceptionHandler != null)
                    {
                        var result = ApiResponse.Error(exceptionHandler.Error);
                        await WriteJsonToResponse(context, result);
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
                    await PipeToResponseAsync(httpContext, memoryStream, originalStream);
                    return;
                }
                
                var allContent = new StreamReader(memoryStream).ReadToEnd();
                var apiResult = ApiResponse.NoContent(httpContext.Response.StatusCode);
                if (!string.IsNullOrEmpty(allContent))
                {
                    apiResult.Result = allContent;
                }

                httpContext.Response.Body = originalStream;
                await WriteJsonToResponse(httpContext, apiResult);
            }
        }


        private static bool ShouldNotWrap(HttpContext httpContext)
        {
            if (httpContext.Response.HasStarted)
            {
                return true;
            }
            
            var contentType = httpContext.Response.ContentType;
            var hasContentType = contentType != null;
            var isAlreadyJson = hasContentType && contentType.Contains("json");
            var isNotText = hasContentType && !contentType.StartsWith("text/");

            return httpContext.Response.StatusCode < 400 || isAlreadyJson || isNotText;
        }

        private static async Task PipeToResponseAsync(HttpContext httpContext, MemoryStream contentStream, Stream responseStream)
        {
            httpContext.Response.Body = responseStream;
            using (var bufferedStream = new BufferedStream(contentStream))
            {
                await bufferedStream.CopyToAsync(httpContext.Response.Body);
            }
        }
        
        private static async Task WriteJsonToResponse(HttpContext httpContext, ApiResponse apiResult)
        {
            var bytes = Encoding.UTF8.GetBytes(SerializeToJson(apiResult));
            httpContext.Response.StatusCode = 200;
            httpContext.Response.ContentLength = bytes.Length;
            httpContext.Response.ContentType = "application/json";
            
            await httpContext.Response.Body.WriteAsync(bytes);
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