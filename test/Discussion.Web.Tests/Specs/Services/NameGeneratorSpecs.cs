using System.Collections.Generic;
using System.Linq;
using Discussion.Tests.Common;
using Discussion.Web.Services.ChatHistoryImporting;
using Xunit;

namespace Discussion.Web.Tests.Specs.Services
{
    [Collection("WebSpecs")]
    public class NameGeneratorSpecs
    {
        private readonly INameGenerator _nameGenerator;

        public NameGeneratorSpecs(TestDiscussionWebApp app)
        {
            _nameGenerator = app.GetService<INameGenerator>();
        }


        [Fact]
        public void should_generate_name()
        {
            var generatedName = _nameGenerator.GenerateName();


            Assert.NotNull(generatedName);
            Assert.True(generatedName.Length > 0);
        }
        
        [Fact]
        public void should_generate_unique_names()
        {
            var names = new List<string>();
            var i = 10;
            do
            {
                names.Add(_nameGenerator.GenerateName());
            } while (--i > 0);

            var distinctNames = names.Distinct().ToList();
            Assert.Equal(distinctNames.Count, names.Count);
        }
    }
}