using System;
using System.Diagnostics;
using System.Web.Mvc;
using System.Web.Routing;

namespace Encryptics.WebPortal.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ExceptionHandlingFilterAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            Trace.TraceError("Unandled Exception");
            Trace.TraceInformation(string.Format("Logged in user: {0}", filterContext.Controller.ControllerContext.HttpContext.User.Identity.Name));
            Trace.TraceError(string.Format("Exception: {0}", filterContext.Exception.Message));
            Debug.Print("Exception stack trace: {0}", filterContext.Exception.StackTrace);
            Elmah.ErrorSignal.FromCurrentContext().Raise(filterContext.Exception);

            var exception = filterContext.Exception as HttpAntiForgeryException;
            if (exception == null) return;
            var routeValues = new RouteValueDictionary();
            routeValues["controller"] = "Account";
            routeValues["action"] = "Login";
            filterContext.Result = new RedirectToRouteResult(routeValues);
            filterContext.ExceptionHandled = true;
        }
    }
}