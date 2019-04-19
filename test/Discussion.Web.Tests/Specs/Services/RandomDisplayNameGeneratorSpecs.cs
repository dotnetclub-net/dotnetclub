using System.Collections.Generic;
using System.Linq;
using Discussion.Core.Models;
using Discussion.Core.Utilities;
using Discussion.Tests.Common;
using Discussion.Web.Services.ChatHistoryImporting;
using Xunit;

namespace Discussion.Web.Tests.Specs.Services
{
    public class RandomDisplayNameGeneratorSpecs
    {
        [Fact]
        public void should_generate_name()
        {
            var generatedName = new WeChatAccount().DisplayName;

            Assert.NotNull(generatedName);
            Assert.True(generatedName.Length > 0);
        }
        
        [Fact]
        public void should_generate_unique_names()
        {
            var account = new WeChatAccount();
            var names = new List<string>();
            var i = 20;
            do
            {
                names.Add(account.DisplayName);
            } while (--i > 0);

            var distinctNames = names.Distinct().ToList();
            Assert.True(distinctNames.Count >= names.Count / 2);
        }
    }
}