using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ModelStateErrorsTraceAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var modelState = filterContext.Controller.ViewData.ModelState;

            if (!modelState.ToList().Any(m => m.Value.Errors.ToList().Any())) return;

            int i = 0;

            Trace.TraceWarning("ModelState errors detected:");
            Trace.IndentLevel++;
            modelState.ToList().ForEach(m => m.Value.Errors.ToList().ForEach(e => Trace.TraceError(string.Format("{0}) {1}", ++i, e.ErrorMessage))));
            Trace.IndentLevel--;
            
            base.OnActionExecuted(filterContext);
        }
    }
}