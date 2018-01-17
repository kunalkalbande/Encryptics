using System.Globalization;
using Encryptics.WebPortal.IdentityModel;
using System;
using System.Diagnostics;
using System.Web.Mvc;
using System.Linq;

// TODO: this functionality could be captured in a custom AuthorizeAttribute derived implementation
namespace Encryptics.WebPortal.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ValidateEntityContextAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var encrypticsPrincipal = ((EncrypticsPrincipal) filterContext.HttpContext.User);

            Debug.Print("filterContext.ActionParameters: {0}", filterContext.ActionParameters.Select(kvp => kvp.Key + ":" + (kvp.Value != null ? kvp.Value.ToString() : string.Empty)).Aggregate((current, next) => current + ", " + next));

            var entityId = long.Parse(filterContext.ActionParameters["id"].ToString());

            Debug.Print("Entity ID: {0}", entityId);

            Debug.Print("Admin IDs: {0}", encrypticsPrincipal.AdminRoles.Keys.Select(x => x.ToString(CultureInfo.InvariantCulture)).Aggregate((current, next) => current + ", " + next.ToString(CultureInfo.InvariantCulture)));

            Trace.TraceInformation(string.Format("Verifying {0} access to entity with Id {1}", encrypticsPrincipal.Identity.Name, entityId));

            if (!encrypticsPrincipal.AdminRoles.ContainsKey(entityId))
            {
                // TODO: Figure out if the user needs to be redirected somewhere safe, if we need to include an error message or kick them out.
                // User does not have access to this Entity
                throw new UnauthorizedAccessException("The current logged in user does not have the proper authority to make changes to the Entity requested.");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}