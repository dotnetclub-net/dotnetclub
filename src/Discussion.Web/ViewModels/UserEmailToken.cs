using System;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Discussion.Web.ViewModels
{
    public class UserEmailToken
    {
        public int UserId { get; set; }
        public string Token { get; set; }
            
            
        public string EncodeAsUrlQueryString()
        {
            var callbackCode = $"userid={UserId}&token={WebUtility.UrlEncode(Token)}";
            return WebUtility.UrlEncode(Convert.ToBase64String(Encoding.ASCII.GetBytes(callbackCode)));
        }


        public static UserEmailToken ExtractFromUrlQueryString(string queryStringValue)
        {
            int userId = 0;
            string token = null;

            var query = QueryHelpers.ParseQuery(queryStringValue);
            var parsedSucceeded = query.TryGetValue("userid", out var userIdStr);
            parsedSucceeded = parsedSucceeded && int.TryParse(userIdStr, out userId);

            try
            {
                parsedSucceeded = parsedSucceeded && query.TryGetValue("token", out var tokenString);
                token = Encoding.ASCII.GetString(Convert.FromBase64String(tokenString));
                parsedSucceeded = parsedSucceeded && !string.IsNullOrEmpty(token);
            }
            catch
            {
                parsedSucceeded = false;
            }

            return parsedSucceeded
                ? new UserEmailToken {UserId = userId, Token = token}
                : null;
        }
    }
}