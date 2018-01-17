using System.Web.Mvc;

namespace Encryptics.WebPortal.Areas.UserAdmin
{
    public class UserAdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "UserAdmin"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "UserAdminUpload",
                "UserAdmin/{controller}/{action}/{entityId}/{importFileId}",
                new { action = "Index", controller = "UploadAccounts", entityId = UrlParameter.Optional, importFileId = UrlParameter.Optional }
                );

            context.MapRoute(
                "UserAdminHome",
                "UserAdmin/{controller}/{action}/{entityId}/{userId}",
                new { action = "ManageAccounts", entityId = UrlParameter.Optional, userId = UrlParameter.Optional }
                );
        }
    }
}