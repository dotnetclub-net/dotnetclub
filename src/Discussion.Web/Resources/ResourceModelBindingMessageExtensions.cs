using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Discussion.Web.Resources
{
    public static class ResourceModelBindingMessageExtensions
    {
        public static void UseTranslatedResources(this DefaultModelBindingMessageProvider messageProvider)
        {
            messageProvider.SetMissingBindRequiredValueAccessor(One(MvcModelBindingResource.MissingBindRequiredValueAccessor));
            messageProvider.SetMissingKeyOrValueAccessor(Zero(MvcModelBindingResource.MissingKeyOrValueAccessor));
            messageProvider.SetMissingRequestBodyRequiredValueAccessor(Zero(MvcModelBindingResource.MissingRequestBodyRequiredValueAccessor));
            messageProvider.SetValueMustNotBeNullAccessor(One(MvcModelBindingResource.ValueMustNotBeNullAccessor));
            messageProvider.SetAttemptedValueIsInvalidAccessor(Two(MvcModelBindingResource.AttemptedValueIsInvalidAccessor));
            messageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(One(MvcModelBindingResource.NonPropertyAttemptedValueIsInvalidAccessor));
            messageProvider.SetUnknownValueIsInvalidAccessor(One(MvcModelBindingResource.UnknownValueIsInvalidAccessor));
            messageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(Zero(MvcModelBindingResource.NonPropertyUnknownValueIsInvalidAccessor));
            messageProvider.SetValueIsInvalidAccessor(One(MvcModelBindingResource.ValueIsInvalidAccessor));
            messageProvider.SetValueMustBeANumberAccessor(One(MvcModelBindingResource.ValueMustBeANumberAccessor));
            messageProvider.SetNonPropertyValueMustBeANumberAccessor(Zero(MvcModelBindingResource.NonPropertyValueMustBeANumberAccessor));
        }


        private static Func<string> Zero(string resource)
        {
            return () => resource;
        }

        private static Func<string, string> One(string resource)
        {
            return arg => string.Format(resource, arg);
        }


        private static Func<string, string, string> Two(string resource)
        {
            return (arg1, arg2) => string.Format(resource, arg1, arg2);
        }
    }
}