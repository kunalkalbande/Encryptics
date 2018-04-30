using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Encryptics.WebPortal.Filters
{
    public class AuthActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var modelState = filterContext.Controller.ViewData.ModelState;
            var httpContext = filterContext.HttpContext;

            if (httpContext.Session["auth"] != null)
            {
                
                httpContext.GetOwinContext().Response.Headers["UserName"] = httpContext.Session["UserName"].ToString();
                httpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/Account/GetUserIdentifier" },
                             httpContext.Session["auth"].ToString());

            }

        }
    }
}