using Microsoft.AspNet.Mvc;
using System;
using System.Reflection;

namespace Discussion.Web.Tests.Specs.Web
{
    public static class ControllerInvokeActionExtensions
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
    }
}
