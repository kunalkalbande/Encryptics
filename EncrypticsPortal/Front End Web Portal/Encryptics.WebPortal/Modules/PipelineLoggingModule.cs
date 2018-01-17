using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;

namespace Encryptics.WebPortal.Modules
{
    public class PipelineLoggingModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            Trace.TraceInformation(string.Format("{1} Initializing {0} class. {1}", GetType().Name, new string('*', 8)));
            context.BeginRequest += OnBeginRequest;
            context.EndRequest += OnEndRequest;
        }

        public void Dispose()
        {
            Trace.TraceInformation(string.Format("{1} Disposing {0} class. {1}", GetType().Name, new string('*', 8)));
        }

        private static void OnBeginRequest(object sender, EventArgs eventArgs)
        {
            var app = sender as HttpApplication;
            if (app == null) return;

            try
            {
                if (app.Request.UserLanguages != null && app.Request.UserLanguages.Length > 0)
                {
                    app.Request.UserLanguages.ToList().ForEach(s => Debug.Print("language: {0}", s));
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(app.Request.UserLanguages.First());
                    Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
                }
                    
                Debug.Print("Request Type: {0}", app.Request.RequestContext.HttpContext.Request.IsAjaxRequest());
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                Trace.TraceError(e.StackTrace);
            }

            Trace.TraceInformation(string.Format("{1} Beginning request. {1}: ({2}){0}", app.Context.Request.RawUrl,
                                                 new string('*', 8), app.Request.RequestType));

            PrintRequestData("QueryString", app.Request.QueryString);
            PrintRequestData("Form", app.Request.Form);
            PrintRequestData("Cookies", app.Request.Cookies);
        }

        private static void PrintRequestData(string info, NameValueCollection nameValueCollection)
        {
            try
            {
                Debug.Print("{0}:", info);
                nameValueCollection.AllKeys.ToList()
                                   .ForEach(
                                       key => Debug.Print("{2}{0} = {1}", key, nameValueCollection[key], new string(' ', 4)));
            }
            catch
            {

            }
           
        }

        private static void PrintRequestData(string info, HttpCookieCollection nameValueCollection)
        {
            Debug.Print("{0}:", info);
            nameValueCollection.AllKeys.ToList()
                               .ForEach(name =>
                                   {
                                       HttpCookie httpCookie = nameValueCollection[name];

                                       if (httpCookie != null)
                                           Debug.Print("    {0} = {1}", name, httpCookie.Value);
                                   });
        }

        private static void OnEndRequest(object sender, EventArgs eventArgs)
        {
            var app = sender as HttpApplication;

            Trace.TraceInformation(app != null
                                       ? string.Format("{1} Ending request. {1}: ({2}){0}", app.Context.Request.RawUrl,
                                                       new string('*', 8), app.Request.RequestType)
                                       : string.Format("{0} Ending request. {0}", new string('*', 8)));
        }
    }
}