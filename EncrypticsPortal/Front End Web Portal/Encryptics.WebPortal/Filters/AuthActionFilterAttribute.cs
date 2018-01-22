using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Filters
{
    public class AuthActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var modelState = filterContext.Controller.ViewData.ModelState;
            var httpContext = filterContext.HttpContext;
            if (httpContext.Session["auth"] != null && (!httpContext.Request.IsAuthenticated))
            {
                Trace.TraceInformation(String.Format("Enter in Authentication using {0} so redirecting to {0} authentication page", httpContext.Session["auth"].ToString()));

                httpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/Account/SetUserDetails" },
                             httpContext.Session["auth"].ToString());
                //if (Request.FilePath == "/Account/SessionEnded" && HttpContext.Application["isStarted"] != null)
                //{
                //    HttpContext.Application.Remove("isStarted");
                //}

            }
           
        }
    }
}