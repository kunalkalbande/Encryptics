using System;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CorsOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //if(!filterContext.HttpContext.Request.Headers.AllKeys)
            base.OnActionExecuting(filterContext);
        }
    }
}