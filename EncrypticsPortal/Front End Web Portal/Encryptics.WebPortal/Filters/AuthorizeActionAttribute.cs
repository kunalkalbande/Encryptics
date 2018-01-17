using Encryptics.WebPortal.IdentityModel;
using Encryptics.WebPortal.PortalService;
using StructureMap;
using System;
using System.Diagnostics;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AuthorizeActionAttribute : AuthorizeAttribute
    {
        private readonly PortalServiceSoap _portalService;
        private readonly bool _overrideEnabled;

        public AuthorizeActionAttribute(bool overrideEnabled = false)
        {
            _overrideEnabled = overrideEnabled;
            _portalService = ObjectFactory.GetInstance<PortalServiceSoap>();
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true) || 
                filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
            {
                base.OnAuthorization(filterContext);

                return;
            }

            if (CheckSessionState(filterContext))
            {
                HandleUnauthorizedRequest(filterContext);

                return;
            }

            if (_overrideEnabled)
            {
                base.OnAuthorization(filterContext);

                return;
            }

            long entityId;
            var actionName = filterContext.ActionDescriptor.ActionName;
            var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var area = filterContext.HttpContext.Request.RequestContext.RouteData.DataTokens.ContainsKey("area")
                           ? filterContext.HttpContext.Request.RequestContext.RouteData.DataTokens["area"]
                           : string.Empty;
            var action = string.Format("{0}/{1}/{2}", area, controllerName, actionName);
            var encrypticsUser = EncrypticsPrincipal.CurrentEncrypticsUser;
            var tokenAuth = new TokenAuth
                {
                    Token = encrypticsUser.Token
                };

            Debug.Print("Action: {0}", actionName);
            Debug.Print("Controller: {0}", controllerName);
            Debug.Print("Area: {0}", area);
            Debug.Print("Action: {0}", action);

            long.TryParse(filterContext.HttpContext.Request.RequestContext.RouteData.Values["entityId"] as string ??
                     (filterContext.HttpContext.Request["entityId"]), out entityId);

            if (entityId == 0) entityId = encrypticsUser.EntityId;

            Debug.Print("Entity ID: {0}", entityId);

            var getUserAuthorizedActionRequest = new GetUserAuthorizedActionRequest(tokenAuth, entityId, encrypticsUser.UserId, action);

            bool authorized;

            if (_overrideEnabled)
            {
                authorized = true;
            }
            else
            {
                var response = _portalService.GetUserAuthorizedAction(getUserAuthorizedActionRequest);
                authorized = response.GetUserAuthorizedActionResult;

                //********syn: remove hardcode response//
                authorized = true;
                //********syn: remove hardcode response//
            }

            if (authorized)
                base.OnAuthorization(filterContext);
            else
                HandleUnauthorizedRequest(filterContext);
        }

        private static bool CheckSessionState(ControllerContext filterContext)
        {
            return filterContext.HttpContext.Session == null || (filterContext.HttpContext.Session["UserId"] as long?) == null
                   || (filterContext.HttpContext.Session["EntityId"] as long?) == null
                   || string.IsNullOrEmpty(filterContext.HttpContext.Session["Token"] as string);
        }

        //protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        //{
        //    var app = filterContext.HttpContext.ApplicationInstance;

        //    if (IsAjaxRequest(app))
        //    {
        //        var jsonResponse =
        //            JsonConvert.SerializeObject(new { success = false, errors = new[] { "Session expired." } });

        //        Trace.TraceInformation("Returning JSON response.");

        //        app.Response.StatusCode = 200;
        //        app.Response.ContentType = "application/json";
        //        app.Response.ClearContent();
        //        app.Response.Write(jsonResponse);
        //        app.Response.SuppressFormsAuthenticationRedirect = true;
        //        //app.Response.End();
        //        app.CompleteRequest();
        //    }
        //    base.HandleUnauthorizedRequest(filterContext);
        //}

        //protected static bool IsAjaxRequest(HttpApplication app)
        //{
        //    IEnumerable<string> xRequestedWithHeaders = app.Request.Headers.GetValues("X-Requested-With");
        //    if (xRequestedWithHeaders != null && xRequestedWithHeaders.Any())
        //    {
        //        string headerValue = xRequestedWithHeaders.FirstOrDefault();
        //        if (!String.IsNullOrEmpty(headerValue))
        //        {
        //            return String.Equals(headerValue, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        //        }
        //    }

        //    return false;
        //    //return !app.Request.ContentType.ToLower().Contains("json");
        //}
    }
}