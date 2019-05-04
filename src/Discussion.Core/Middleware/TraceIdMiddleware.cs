using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace Discussion.Core.Middleware
{
    // todo: 添加单元测试；添加到 HttpClient 的集成（将 X-Trace-Id 传递到所依赖的、调用其他服务时的请求）
    public class TraceIdMiddleware
    {
        private readonly TraceIdMiddlewareOptions _options;
        private readonly RequestDelegate _next;

        private readonly ILogger _logger;
        public const string HttpContextTraceIdKey  = TraceIdMiddlewareOptions.DefaultHeader;

        public TraceIdMiddleware(RequestDelegate next, ILogger<TraceIdMiddleware> logger, TraceIdMiddlewareOptions options)
        {
            _next = next;
            _options = options;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            string traceId = null;
            var traceIdFromRequest = false;
            if (context.Request.Headers.TryGetValue(_options.HeaderName, out var traceIdHeaderVal))
            {
                traceIdFromRequest = true;
                traceId = traceIdHeaderVal.ToString();
            }
            else
            {
                traceId = Guid.NewGuid().ToString("D");
            }
            
            _logger.LogDebug($"请求 TraceId 为 {traceId} （{(traceIdFromRequest ? "已指定" : "新生成")}）");
            using (_logger.BeginScope($"TraceId:{traceId}"))
            {
                context.Items[HttpContextTraceIdKey] = traceId;
                context.Response.Headers.Add(_options.HeaderName, traceId);
                    
                await AwaitHttpPipeline(context);
            }
        }

        private async Task AwaitHttpPipeline(HttpContext context)
        {
            var url = context.Request.GetDisplayUrl();
            _logger.LogInformation("请求已开始：{method} {path}", context.Request.Method, ShortenUrlPath(url));
            
            var startTimestamp = Stopwatch.GetTimestamp();

            await _next(context);
            
            var stopTimestamp = Stopwatch.GetTimestamp();
            var elapsed = new TimeSpan((long)(TimestampToTicks * (stopTimestamp - startTimestamp)));
            _logger.LogInformation("请求已完成：{time}ms {code} {type}",
                elapsed.TotalMilliseconds,
                context.Response.StatusCode,
                context.Response.ContentType);
        }
        
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        private static string ShortenUrlPath(string url)
        {
            const int maxUrlLength = 4096;
            if (url.Length > maxUrlLength)
            {
                url = url.Substring(0, maxUrlLength) + "...(shortened)";
            }

            return url;
        }

        public static bool IsEnabledOnRequest(HttpContext httpContext)
        {
            return httpContext?.Items[HttpContextTraceIdKey] != null;
        }
        
        public static string GetTraceId(HttpContext httpContext)
        {
            if (IsEnabledOnRequest(httpContext))
            {
                return (string)httpContext.Items[HttpContextTraceIdKey];
            }

            return null;
        }
    }

    public static class TraceIdMiddlewareExtensions
    {
        public static void UseTracingId(this IApplicationBuilder appBuilder, TraceIdMiddlewareOptions options = null)
        {
            appBuilder.UseMiddleware<TraceIdMiddleware>(options ?? new TraceIdMiddlewareOptions());
        }
    }

    public class TraceIdMiddlewareOptions
    {
        public const string DefaultHeader = "X-Trace-Id";
        public TraceIdMiddlewareOptions()
        {
            HeaderName = DefaultHeader;
        }

        public string HeaderName { get; set; }
    }
}