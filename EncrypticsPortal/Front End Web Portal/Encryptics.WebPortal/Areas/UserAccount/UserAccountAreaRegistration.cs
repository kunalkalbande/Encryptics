using System.Web.Mvc;

namespace Encryptics.WebPortal.Areas.UserAccount
{
    public class UserAccountAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "UserAccount"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "UserAccount",
                "UserAccount/{controller}/{action}/{entityId}/{userId}",
                new { action = "Dashboard", entityId = UrlParameter.Optional, userId = UrlParameter.Optional }
                );

            context.MapRoute(
                "UserAccountDevice",
                "UserAccount/{controller}/{action}/{deviceId}/{entityId}/{userId}",
                new { action = "ViewDevices", deviceId = UrlParameter.Optional, entityId = UrlParameter.Optional, userId = UrlParameter.Optional }
                );
        }
    }
}