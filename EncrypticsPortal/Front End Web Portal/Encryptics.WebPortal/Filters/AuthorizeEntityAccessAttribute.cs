using Encryptics.WebPortal.IdentityModel;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Encryptics.WebPortal.Filters
{
    // TODO: Remove this from the project.
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class AuthorizeEntityAccessAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = filterContext.HttpContext.User.GetType() != typeof (EncrypticsPrincipal) ? new RedirectToRouteResult(new RouteValueDictionary { { "area", string.Empty }, { "action", "SessionEnded" }, { "controller", "Account" }, {"returnUrl", filterContext.HttpContext.Request.UrlReferrer} }) : new RedirectToRouteResult(new RouteValueDictionary { { "area", string.Empty }, { "action", "AccessDenied" }, { "controller", "Home" } });
        }

        // TODO: Is there anything that needs to be done for AJAX requests?
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            Trace.TraceInformation("Authorizing Entity Access");
            if (httpContext.User.GetType() != typeof(EncrypticsPrincipal))
            {
                Trace.TraceWarning("Access Denied: Not Encryptics Principal");

                if (httpContext.Request.RequestType == "POST") // TODO: Does this need to be there?
                    httpContext.Response.SuppressFormsAuthenticationRedirect = true;

                return false;
            }

            var encrypticsPrincipal = ((EncrypticsPrincipal)httpContext.User);

            long entityId;

            long.TryParse(httpContext.Request.RequestContext.RouteData.Values["id"] as string ??
                     (httpContext.Request["id"]), out entityId);

            if (entityId == 0) entityId = encrypticsPrincipal.EntityId;

            Debug.Print("Entity ID: {0}", entityId);

            Debug.Print("Admin IDs: {0}{1}", encrypticsPrincipal.AdminRoles.Count, encrypticsPrincipal.AdminRoles.Aggregate("\n", (s, pair) => string.Format("{0}{1,4} : {2,-10}\n", s, pair.Key.ToString(CultureInfo.InvariantCulture), pair.Value)));

            Debug.Print("Roles: {0}", Roles);

            Trace.TraceInformation(string.Format("Verifying {0} access to entity with Id {1}", encrypticsPrincipal.Identity.Name, entityId));

            var roles = Roles.Split(new[] { ", " }, StringSplitOptions.None);

            if (!(encrypticsPrincipal.AdminRoles.ContainsKey(entityId) && roles.Contains(encrypticsPrincipal.AdminRoles[entityId])))
            {
                // User does not have access to this Entity
                Trace.WriteLineIf(!encrypticsPrincipal.AdminRoles.ContainsKey(entityId), string.Format("Access Denied: No access to entity {0}", entityId), "WARN");

                if (encrypticsPrincipal.AdminRoles.ContainsKey(entityId))
                {
                    Trace.WriteLineIf(!roles.Contains(encrypticsPrincipal.AdminRoles[entityId]), string.Format("Access Denied: Role \"{1}\" not found for entity {0}", entityId, encrypticsPrincipal.AdminRoles[entityId]), "WARN");
                }

                return false;
            }

            return base.AuthorizeCore(httpContext);
        }
    }
}