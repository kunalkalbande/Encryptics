using System.Web.Mvc;

namespace Encryptics.WebPortal.Areas.Reseller
{
    public class ResellerAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "Reseller"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Reseller_default",
                "Reseller/{controller}/{action}/{id}",
                new {action = "Index", id = UrlParameter.Optional}
                );
        }
    }
}