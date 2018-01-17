using System.Configuration;

namespace Encryptics.WebPortal.Configuration
{
    public static class Configuration
    {
        public static CorsConfiguration CorsConfiguration
        {
            get
            {
                var config = (CorsConfiguration)ConfigurationManager.GetSection("encryptics/corsconfig");

                return config;
            }
        }
    }

    public class CorsConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("origins", IsDefaultCollection = true)]
        public OriginsCollection OriginsCollection
        {
            get { return (OriginsCollection)this["origins"]; }
            set { this["origins"] = value; }
        }
    }

    public class OriginElement : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("url", DefaultValue = "http://localhost", IsRequired = true)]
        [RegexStringValidator(@"https?\://\S+")]
        public string Url
        {
            get { return (string)this["url"]; }
            set { this["url"] = value; }
        }
    }

    [ConfigurationCollection(typeof(OriginElement))]
    public class OriginsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new OriginElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((OriginElement)element).Name;
        }
    }
}