using System.Web.Mvc;

namespace Encryptics.WebPortal.Areas.Company
{
    public class CompanyAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "Company"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Company",
                "Company/{controller}/{action}/{entityId}",
                new {action = "Dashboard"}
                );

            context.MapRoute(
                "DistributionGroupMember",
                "Company/{controller}/{action}/{entityId}/{groupId}",
                new { controller="DistributionGroupMembers" }
                );

            context.MapRoute(
               "CompanyProductVersion",
               "Company/{controller}/{action}/{id}",
               new { controller = "CompanyProductVersion", id =  UrlParameter.Optional }
               );
        }
    }
}