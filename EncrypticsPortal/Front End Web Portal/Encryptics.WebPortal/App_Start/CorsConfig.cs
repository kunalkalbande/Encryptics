using Encryptics.WebPortal.Configuration;
using Thinktecture.IdentityModel.Http.Cors.Mvc;

namespace Encryptics.WebPortal
{
    public class CorsConfig
    {
        public static void RegisterCors(MvcCorsConfiguration corsConfig)
        {
            var config = Configuration.Configuration.CorsConfiguration;

            foreach (OriginElement origin in config.OriginsCollection)
            {
                corsConfig.ForResources("Account.JsonRegister")
                          .ForOrigins(origin.Url)
                          .AllowAll();

                corsConfig.ForResources("Account.JsonActivate")
                          .ForOrigins(origin.Url)
                          .AllowAll();

                corsConfig.ForResources("Account.JsonLogin")
                          .ForOrigins(origin.Url)
                          .AllowAll();
            }
        }
    }
}
