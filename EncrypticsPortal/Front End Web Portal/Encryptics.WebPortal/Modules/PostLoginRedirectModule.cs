using System;
using System.Diagnostics;
using System.Web;
using Encryptics.WebPortal.IdentityModel;

namespace Encryptics.WebPortal.Modules
{
    public class PostLoginRedirectModule : EncrypticsHttpModule, IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.EndRequest += ContextOnEndRequest;
        }

        private static void ContextOnEndRequest(object sender, EventArgs eventArgs)
        {
            Trace.TraceInformation("ContextOnEndRequest");

            var request = HttpContext.Current.Request;
            var response = HttpContext.Current.Response;

            Debug.Print("RequestType: {0}", request.RequestType);
            Debug.Print("StatusCode: {0}", response.StatusCode);

            if (response.StatusCode == 302 && IsAjaxRequest(HttpContext.Current.ApplicationInstance) && !(HttpContext.Current.User is EncrypticsPrincipal))
            {
                RedirectResponse(HttpContext.Current.ApplicationInstance);
            }

            if ((request.RequestType != "POST" || response.StatusCode != 401)) return;

            RedirectResponse(HttpContext.Current.ApplicationInstance);
        }

        public void Dispose()
        {

        }
    }
}