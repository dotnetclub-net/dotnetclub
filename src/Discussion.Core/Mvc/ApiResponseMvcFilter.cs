using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Discussion.Core.Mvc
{
    public class ApiResponseMvcFilter : IResultFilter, IActionFilter
    {
        public static bool IsApiRequest(HttpRequest request)
        {
            return request.Path.ToString()
                               .ToLowerInvariant()
                               .StartsWith("/api/");
        }
        
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!IsApiRequest(context.HttpContext.Request))
            {
                return;
            }
            
            if (!context.ModelState.IsValid)
            {
                context.Result = new ObjectResult(ApiResponse.Error(context.ModelState));
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }
        
        
        public void OnResultExecuting(ResultExecutingContext context)
         {
             if (!IsApiRequest(context.HttpContext.Request))
             {
                 return;
             }
             
             if (context.Result is ObjectResult objectResult &&
                 !(objectResult.Value is ApiResponse))
             {
                 objectResult.Value = ApiResponse.ActionResult(objectResult.Value);
             }
         }
    
         public void OnResultExecuted(ResultExecutedContext context)
         {
             
         }
    }
}