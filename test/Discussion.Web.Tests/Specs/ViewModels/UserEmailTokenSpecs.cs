using System;
using System.Text;
using Discussion.Core.Utilities;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace Discussion.Web.Tests.Specs.ViewModels
{
    public class UserEmailTokenSpecs
    {
        [Fact]
        public void should_encode_as_url_query_string()
        {
            var emailToken = new UserEmailToken
            {
                UserId = 35,
                Token = StringUtility.Random(32)
            };

            var encoded = emailToken.EncodeAsUrlQueryString();
            
            
            var queryString = Encoding.ASCII.GetString(Convert.FromBase64String(encoded));
            var query = QueryHelpers.ParseQuery(queryString);
            Assert.Equal("35", query["userid"]);
            Assert.Equal(emailToken.Token, query["token"]);
        }
        
        [Fact]
        public void should_decode_encoded_url_query_string()
        {
            var emailToken = new UserEmailToken
            {
                UserId = 35,
                Token = StringUtility.Random(32)
            };
            var encoded = emailToken.EncodeAsUrlQueryString();
            
            
            var extracted = UserEmailToken.ExtractFromUrlQueryString(encoded);
            
            
            Assert.Equal(35, extracted.UserId);
            Assert.Equal(emailToken.Token, extracted.Token);
        }
    }
}