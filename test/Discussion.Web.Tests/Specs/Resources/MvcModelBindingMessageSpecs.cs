using System.Linq;
using Discussion.Web.Resources;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Xunit;
using static Discussion.Web.Tests.Specs.Resources.IdentityErrorDescriberSpecs;

namespace Discussion.Web.Tests.Specs.Resources
{
    public class MvcModelBindingMessageSpecs
    {
        [Fact]
        public void should_use_zh_cn_model_binding_messages()
        {
            var messageProvider = new DefaultModelBindingMessageProvider();
            messageProvider.UseTranslatedResources();

            var messages = new[]
            {
                messageProvider.ValueIsInvalidAccessor("val"),
                messageProvider.AttemptedValueIsInvalidAccessor("abcd", "defg"),
                messageProvider.MissingBindRequiredValueAccessor("field"),
                messageProvider.MissingKeyOrValueAccessor(),
                messageProvider.UnknownValueIsInvalidAccessor("value"),
                messageProvider.MissingRequestBodyRequiredValueAccessor(),
                messageProvider.ValueMustBeANumberAccessor("value"),
                messageProvider.ValueMustNotBeNullAccessor("value"),
                messageProvider.NonPropertyAttemptedValueIsInvalidAccessor("prop"),
                messageProvider.NonPropertyUnknownValueIsInvalidAccessor(),
                messageProvider.NonPropertyValueMustBeANumberAccessor()
            };
            
            
            messages.ToList().ForEach(msg => Assert.True(HasChinese(msg)));
            
        }
    }
}