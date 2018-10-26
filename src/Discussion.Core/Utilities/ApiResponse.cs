using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Discussion.Core.Utilities
{
    public class ApiResponse
    {
        private const char ErrorMsgKVDelimiter = ':';
        private const char ErrorDelimiter = '\n';
        private const string ErrorMsgDelimiter = ";";

        public int Code { get; set; } = 200;
        public object Result { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; }

        public string ErrorMessage
        {
            get
            {
                return Errors?.Aggregate(string.Empty, (prev, err) =>
                {
                    var key = string.IsNullOrWhiteSpace(err.Key) ? string.Empty : string.Concat(err.Key, ErrorMsgKVDelimiter);
                    return string.Concat(prev, ErrorDelimiter, key, string.Join(ErrorMsgDelimiter, err.Value));
                }).Trim();
            }
        }

        public bool HasSucceeded => Code >= 200 && Code <= 399;

        public static ApiResponse ActionResult(object result)
        {
            return new ApiResponse { Result = result };
        }

        public static ApiResponse Error(Exception error)
        {
            return new ApiResponse
            {
                Code = 500,
                Errors = new Dictionary<string, List<string>>
                {
                    {string.Empty, new List<string> {error.Message}}
                }
            };
        }

        public static ApiResponse Error(string errorKey, string errorMessage)
        {
            return new ApiResponse
            {
                Code = 400,
                Errors = new Dictionary<string, List<string>>
                {
                    {errorKey, new List<string> {errorMessage}}
                }
            };
        }

        public static ApiResponse Error(string errorMessage)
        {
            return new ApiResponse
            {
                Code = 400,
                Errors = new Dictionary<string, List<string>>
                {
                    {string.Empty, new List<string> {errorMessage}}
                }
            };
        }

        public static ApiResponse NoContent(int code)
        {
            return new ApiResponse
            {
                Code = code
            };
        }

        public static ApiResponse NoContent()
        {
            return new ApiResponse();
        }

        public static ApiResponse NoContent(HttpStatusCode statusCode)
        {
            return new ApiResponse
            {
                Code = (int)statusCode
            };
        }
    }
}