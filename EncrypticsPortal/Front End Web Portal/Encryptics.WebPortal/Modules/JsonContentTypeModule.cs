using System.Diagnostics;
using System.Web;

namespace Encryptics.WebPortal.Modules
{
    public class JsonContentTypeModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += (sender, args) =>
                {
                    var app = sender as HttpApplication;
                    if (app != null && app.Request.ContentType == string.Empty)
                    {
                        app.Request.ContentType = @"application/json";
                    }
                };
        }

        public void Dispose()
        {
            Trace.TraceInformation(string.Format("{1} Disposing {0} class. {1}", GetType().Name, new string('*', 8)));
        }
    }
}