using System.Web.Mvc;

namespace Encryptics.WebPortal.Areas.CompanyAdmin
{
    public class CompanyAdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "CompanyAdmin"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "CompanyAdmin",
                "CompanyAdmin/{controller}/{action}/{entityId}",
                new { action = "Dashboard", entityId = UrlParameter.Optional }
                );
        }
    }
}