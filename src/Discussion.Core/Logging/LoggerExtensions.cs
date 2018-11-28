using Discussion.Core.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Discussion.Core.Logging
{
    public static class LoggerExtensions
    {
        public static void LogModelState<T>(this ILogger<T> logger, string action, ModelStateDictionary modelState, string userName = null)
        {
            var errorMessage = modelState.IsValid ? null : ApiResponse.Error(modelState).ErrorMessage;
            var resultDesc = modelState.IsValid ? "格式正确" : "数据格式不正确";
            var logLevel = modelState.IsValid ? LogLevel.Information : LogLevel.Warning;
            logger.Log(logLevel, $"{action}{resultDesc}：{userName}：{errorMessage}");
        }

        public static void LogIdentityResult<T>(this ILogger<T> logger, string action, IdentityResult result, string userName = null)
        {
            var resultDesc = result.Succeeded ? "成功" : "失败";
            var message = string.Join(";", result.Errors);
            var logLevel = result.Succeeded ? LogLevel.Information : LogLevel.Warning;
            logger.Log(logLevel, $"{action}{resultDesc}：{userName}：{message}");
        }
    }
}