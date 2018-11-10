using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Discussion.Web.ViewModels
{
    public class UserEmailToken
    {
        public int UserId { get; set; }
        public string Token { get; set; }
            
            
        public string EncodeAsUrlQueryString()
        {
            var callbackCode = $"userid={UserId}&token={WebUtility.UrlEncode(Token)}";

            return Convert.ToBase64String(Encoding.ASCII.GetBytes(callbackCode));
        }


        public static UserEmailToken ExtractFromUrlQueryString(string queryStringToken)
        {
            Dictionary<string, StringValues> query = null;

            try
            {
                var queryString = Encoding.ASCII.GetString(Convert.FromBase64String(queryStringToken));
                query = QueryHelpers.ParseQuery(queryString);
            }
            catch
            {
                return null;
            }

            var userId = 0;
            var parsedSucceeded = query.TryGetValue("userid", out var userIdStr);
            parsedSucceeded = parsedSucceeded && int.TryParse(userIdStr, out userId);
            parsedSucceeded = parsedSucceeded && query.TryGetValue("token", out var tokenString);
            parsedSucceeded = parsedSucceeded && !string.IsNullOrEmpty(tokenString);

            return parsedSucceeded
                ? new UserEmailToken { UserId = userId, Token = WebUtility.UrlDecode(tokenString).Replace(" ", "+") }
                : null;
        }
    }
}