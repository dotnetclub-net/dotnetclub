using System;
using System.Linq;
using Discussion.Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Discussion.Admin.Supporting
{
    public static class ApiResponseControllerBaseExtensions
    {
        public static ApiResponse Respond(this ControllerBase controller, object result = null)
        {
            return ApiResponse.ActionResult(result);
        }

        public static ApiResponse Error(this ControllerBase controller, Exception exception)
        {
            return ApiResponse.Error(exception);
        }

        public static ApiResponse Error(this ControllerBase controller, string message)
        {
            return ApiResponse.Error(message);
        }

        public static ApiResponse Error(this ControllerBase controller, ModelStateDictionary modelState)
        {
            if (modelState.IsValid)
            {
                return ApiResponse.NoContent();
            }

            var errors = modelState
                .ToDictionary(state => state.Key,
                    state => state.Value
                        .Errors
                        .Select(err => err.ErrorMessage ?? err.Exception?.Message)
                        .ToList());
            var response = ApiResponse.NoContent(400);
            response.Errors = errors;
            return response;
        }
    }
}