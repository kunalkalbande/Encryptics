using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Encryptics.WebPortal.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class DisabledAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.Result =
                new RedirectToRouteResult(new RouteValueDictionary
                    {
                        {"controller", "Home"},
                        {"action", "AccessDenied"}
                    });
        }
    }
}