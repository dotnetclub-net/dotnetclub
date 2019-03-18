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
        internal int UserId { get; set; }
        internal string Token { get; set; }

        internal string EncodeAsQueryString()
        {
            var callbackCode = $"userid={UserId}&token={WebUtility.UrlEncode(Token)}";

            return Convert.ToBase64String(Encoding.ASCII.GetBytes(callbackCode));
        }

        internal static UserEmailToken ExtractFromQueryString(string queryString)
        {
            if (string.IsNullOrWhiteSpace(queryString)) return null;

            Dictionary<string, StringValues> query = null;
            try
            {
                var str = Encoding.ASCII.GetString(Convert.FromBase64String(queryString));
                query = QueryHelpers.ParseQuery(str);
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
                ? new UserEmailToken
                {
                    UserId = userId,
                    Token = WebUtility.UrlDecode(tokenString).Replace(" ", "+")
                }
                : null;
        }
    }
}
