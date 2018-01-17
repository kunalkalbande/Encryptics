using System;
using System.Web;

namespace Encryptics.WebPortal.Modules
{
    public class AddHttpHeaders : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += BeginRequest;
        }

        private static void BeginRequest(object sender, EventArgs eventArgs)
        {
            HttpResponse response = HttpContext.Current.Response;

            response.AddHeader("Cache-Control", "private, no-store, max-age=0, no-cache, must-revalidate, post-check=0, pre-check=0");
            response.AddHeader("Pragma", "no-cache");
            response.AddHeader("Expires", "Fri, 01 Jan 1990 00:00:00 GMT");
        }

        public void Dispose()
        {
            
        }
    }
}