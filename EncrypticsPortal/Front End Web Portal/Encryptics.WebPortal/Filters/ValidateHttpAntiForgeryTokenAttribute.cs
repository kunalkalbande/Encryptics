using System;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Filters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ValidateHttpAntiForgeryTokenAttribute : AuthorizeAttribute
    {

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                ValidateRequestHeader(filterContext.HttpContext.Request);
            }
            else
            {
                AntiForgery.Validate();
            }
        }

        private static void ValidateRequestHeader(HttpRequestBase request)
        {
            var cookieToken = String.Empty;
            var formToken = String.Empty;

            if (request.Headers.AllKeys.Contains(MvcApplication.REQUESTVERIFICATIONTOKEN))
            {
                var tokenValue = request.Headers[MvcApplication.REQUESTVERIFICATIONTOKEN];
                Debug.Print("tokenValue = {0}", tokenValue);
                if (!String.IsNullOrEmpty(tokenValue))
                {
                    var tokens = tokenValue.Split(':');
                    if (tokens.Length == 2)
                    {
                        cookieToken = tokens[0].Trim();
                        formToken = tokens[1].Trim();
                    }
                }
            }

            AntiForgery.Validate(cookieToken, formToken);
        }
    }
}