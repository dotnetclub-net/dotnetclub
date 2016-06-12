using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Discussion.Web.Tests.Specs
{
    public static class ControllerExtensions
    {

        public static IActionResult InvokeAction(this Controller controller, string actionName, object[] parameters)
        {
            var ctrlType = controller.GetType();
            var actionMethod = ctrlType.GetMethod(actionName, BindingFlags.Public | BindingFlags.Instance);
            if (actionMethod == null)
            {
                throw new MissingMethodException(ctrlType.FullName, actionName);
            }

            return actionMethod.Invoke(controller, parameters) as IActionResult;
        }

        public static void ValidateModel(this Controller controller, object model, IServiceProvider services = null)
        {
            var validationContext = new ValidationContext(model, services, null);
            var validationResults = new List<ValidationResult>();


            Validator.TryValidateObject(model, validationContext, validationResults);
            foreach (var validationResult in validationResults)
            {
                controller.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
            }
        }
    }
}
