using System.Web;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Filters
{
    public class NoCacheGlobalActionFilter : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var cache = filterContext.HttpContext.Response.Cache;
            cache.SetCacheability(HttpCacheability.NoCache);
        
            base.OnResultExecuted(filterContext);
        }
    }
}