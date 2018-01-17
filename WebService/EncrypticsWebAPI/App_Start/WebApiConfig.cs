using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Cors;
using System.Web.Http.Routing;

namespace EncrypticsWebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var enableCorsAttribute = new EnableCorsAttribute("*",
                                               "Origin, Content-Type, Accept, TokenAuth_ID, TokenAuth_Status, Cache-Control, Pragma",
                                               "GET, PUT, POST, DELETE, OPTIONS",
                                               "TokenAuth_ID, TokenAuth_Status");
            config.EnableCors(enableCorsAttribute);

            // Web API configuration and services
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings
                .Add(new RequestHeaderMapping("Accept",
                    "text/html",
                    StringComparison.InvariantCultureIgnoreCase,
                    true,
                    "application/json"));
            
            // Web API routes
            config.MapHttpAttributeRoutes(new CentralizedPrefixProvider("v2"));
        }
    }

    public class CentralizedPrefixProvider : DefaultDirectRouteProvider
    {
        private readonly string _centralizedPrefix;

        public CentralizedPrefixProvider(string centralizedPrefix)
        {
            _centralizedPrefix = centralizedPrefix;
        }

        protected override string GetRoutePrefix(HttpControllerDescriptor controllerDescriptor)
        {
            var existingPrefix = base.GetRoutePrefix(controllerDescriptor);
            if (existingPrefix == null) return _centralizedPrefix;

            return string.Format("{0}/{1}", _centralizedPrefix, existingPrefix);
        }
    }
}
