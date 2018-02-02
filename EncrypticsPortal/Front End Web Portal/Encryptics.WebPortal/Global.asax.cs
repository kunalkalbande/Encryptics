using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Thinktecture.IdentityModel.Http.Cors.Mvc;

namespace Encryptics.WebPortal
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        public const string REQUESTVERIFICATIONTOKEN = "X-XSRF-Token";

        protected void Application_Start()
        {
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

            Application["Version"] = string.Format("Version {0}", version);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new WebFormGlobalizationViewEngine());
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
            MappingConfig.RegisterMaps();
            CorsConfig.RegisterCors(MvcCorsConfiguration.Configuration);
            MvcHandler.DisableMvcResponseHeader = true;
            ModelBinders.Binders[typeof(VersionInfo)] = new VersionInfoModelBinder();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            string language;
            CultureInfo culture;
            var languageCookie = Context.Request.Cookies["language"];

            if (null != languageCookie)
            {
                language = languageCookie.Value;
            }
            else
            {
                language = Request.UserLanguages != null ? Request.UserLanguages[0] : "en-US";
            }

            if (IsValidCultureInfoName(language))
                culture = language.Length == 2 ? CultureInfo.CreateSpecificCulture(language) : new CultureInfo(language);
            else
                culture = CultureInfo.InvariantCulture;

            Thread.CurrentThread.CurrentCulture =
                    Thread.CurrentThread.CurrentUICulture = culture;

            if (Request.IsLocal) return;

            switch (Request.Url.Scheme)
            {
                case "https":
                    var hstsLifespan = 300;
                    if (WebConfigurationManager.AppSettings["HSTS"] != null)
                    {
                        if (int.TryParse(WebConfigurationManager.AppSettings["HSTS"], out hstsLifespan) != true)
                            hstsLifespan = 300;
                    }
                    Response.AddHeader("Strict-Transport-Security", "max-age=" + hstsLifespan + "; includeSubDomains");
                    break;
                case "http":
                    var path = "https://" + Request.Url.Host + Request.Url.PathAndQuery;
                    Response.Status = "301 Moved Permanently";
                    Response.AddHeader("Location", path);
                    break;
            }
        }

        protected void Application_SessionEnding(object sender, EventArgs e)
        {
            LogUserOff();
        }

        private void LogUserOff()
        {
            try
            {
                using (var client = new PortalServiceSoapClient())
                {
                    //AuthConfig.LogOut = true;
                    //if (Session["auth"] != null)
                    //{
                    //    AuthConfig.AuthType = Session["auth"].ToString();
                    //}
                   
                    var token = (string)Session["Token"];
                    var userId = (long)Session["UserID"];
                    var entityId = (long)Session["EntityID"];

                    var tokenAuth = new TokenAuth
                    {
                        Token = token
                    };

                    client.UserLogout(ref tokenAuth, user_id: userId, entity_id: entityId);

                }
            }
            catch
            {

            }
        }

        private void Session_End(Object sender, EventArgs e)
        {
            LogUserOff();
        }

        protected static bool IsValidCultureInfoName(string name)
        {
            return
                CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Any(c => c.Name == name);
        }

        protected void Application_PreSendRequestHeaders()
        {
            Response.Headers.Remove("Server");
            Response.Headers.Remove("X-AspNet-Version");
            Response.Headers.Remove("X-AspNetMvc-Version");
            Response.Headers.Remove("X-Powered-By");
            Response.Headers.Remove("Pragma");
            Response.Headers.Remove("Expires");

            Response.AppendHeader("Pragma", "no-cache"); // HTTP 1.0.
            Response.AppendHeader("Expires", "0"); // Proxies.

            Response.Cache.SetCacheability(HttpCacheability.ServerAndNoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
        }
    }
}