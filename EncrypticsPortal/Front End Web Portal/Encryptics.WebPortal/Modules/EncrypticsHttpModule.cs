using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Modules
{
    public class EncrypticsHttpModule
    {
        protected static void RedirectResponse(HttpApplication app)
        {
            var request = HttpContext.Current.Request;
            if (IsAjaxRequest(app))
            {
                var jsonResponse =
                    JsonConvert.SerializeObject(new
                    {
                        success = false,
                        errors = new[] { "Session expired." },
                        redirect = string.Format("/Account/SessionEnded?returnUrl={0}", request.UrlReferrer)
                    });

                Trace.TraceInformation("Returning JSON response.");
                app.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                app.Response.ContentType = "application/json";
                app.Response.ClearContent();
                app.Response.Write(jsonResponse);
                app.Response.SuppressFormsAuthenticationRedirect = true;
                app.CompleteRequest();
            }
            else
            {
                Trace.TraceInformation("Returning redirect.");
                app.Response.StatusCode = (int)HttpStatusCode.Redirect;
                app.Response.SuppressFormsAuthenticationRedirect = true;
                app.Response.RedirectToRoute("Default",
                                             new { controller = "Account", action = "SessionEnded", id = UrlParameter.Optional, returnUrl = request.UrlReferrer });
                app.CompleteRequest();
            }
        }

        protected static bool IsAjaxRequest(HttpApplication app)
        {
            IEnumerable<string> xRequestedWithHeaders = app.Request.Headers.GetValues("X-Requested-With");
            if (xRequestedWithHeaders != null && xRequestedWithHeaders.Any())
            {
                var headerValue = xRequestedWithHeaders.FirstOrDefault();

                if (!String.IsNullOrEmpty(headerValue))
                {
                    return String.Equals(headerValue, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
                }
            }

            return false;
        }
    }
}