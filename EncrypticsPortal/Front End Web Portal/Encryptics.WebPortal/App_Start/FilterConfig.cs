using System.Web.Mvc;
using Encryptics.WebPortal.Filters;

namespace Encryptics.WebPortal
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ExceptionHandlingFilterAttribute());
            filters.Add(new TraceActionFilterAttribute());
            filters.Add(new NoCacheGlobalActionFilter());
        }
    }
}