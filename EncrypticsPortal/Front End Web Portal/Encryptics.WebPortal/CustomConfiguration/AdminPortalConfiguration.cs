using System.Configuration;

namespace Encryptics.WebPortal.CustomConfiguration
{
    public class AdminPortalConfiguration : ConfigurationSection
    {
        public static AdminPortalConfiguration CustomConfiguration { get { return ConfigurationManager.GetSection("encryptics.adminPortal") as AdminPortalConfiguration; } }
    }
}