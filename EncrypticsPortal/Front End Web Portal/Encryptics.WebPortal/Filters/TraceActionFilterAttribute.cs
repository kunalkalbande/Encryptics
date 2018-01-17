using Microsoft.Owin.Security;
using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

// TODO: figure out if there's a way to get the the controllers token so we can check if there's a token error and log it and log the user off the site.
namespace Encryptics.WebPortal.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TraceActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var modelState = filterContext.Controller.ViewData.ModelState;
           
            if (modelState.ToList().Any(m => m.Value.Errors.ToList().Any()))
            {
                int i = 0;

                Trace.TraceWarning("ModelState errors detected:");
                modelState.ToList().ForEach(m => m.Value.Errors.ToList().ForEach(e => Trace.TraceError(string.Format("{0}) {1}", ++i, e.ErrorMessage))));
            }

            if (filterContext.Controller.TempData.ContainsKey("ErrorMessage"))
            {
                Trace.TraceWarning("Error message: {0}", new[]{filterContext.Controller.TempData["ErrorMessage"]});
            }
            
            Trace.TraceInformation("Finished executing (" + filterContext.HttpContext.Request.RequestType + ") " + filterContext.HttpContext.Request.Path);
            base.OnActionExecuted(filterContext);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Trace.TraceInformation("Executing (" + filterContext.HttpContext.Request.RequestType + ") " + filterContext.HttpContext.Request.Path);
            base.OnActionExecuting(filterContext);
        }

        //public override void OnResultExecuted(ResultExecutedContext filterContext)
        //{
        //    base.OnResultExecuted(filterContext);
        //    Trace.TraceInformation("======== Entering TraceActionFilterAttribute (OnResultExecuted) ========");
        //    Trace.TraceInformation("TraceActionFilterAttribute (OnResultExecuted): " + filterContext.Result);
        //}

        //public override void OnResultExecuting(ResultExecutingContext filterContext)
        //{
        //    base.OnResultExecuting(filterContext);
        //    Trace.TraceInformation("======== Entering TraceActionFilterAttribute (OnResultExecuting) ========");
        //    Trace.TraceInformation("TraceActionFilterAttribute (OnResultExecuting): " + filterContext.Result);
        //}
    }
}