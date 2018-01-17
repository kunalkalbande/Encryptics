using System;
using System.Web.Mvc;

namespace Encryptics.WebPortal.ActionResults
{
    public class JsonpResult : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;
            var jsoncallback = (context.RouteData.Values["jsoncallback"] as string) ?? request["jsoncallback"];
            if (!string.IsNullOrEmpty(jsoncallback))
            {
                if (string.IsNullOrEmpty(ContentType))
                {
                    ContentType = "application/x-javascript";
                }
                response.Write(string.Format("{0}(", jsoncallback));
            }
            base.ExecuteResult(context);
            if (!string.IsNullOrEmpty(jsoncallback))
            {
                response.Write(")");
            }
        }
    }
}