using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Microsoft.Web.Mvc;

namespace Encryptics.WebPortal.Helpers
{
    public class WebFormGlobalizationViewEngine : FixedRazorViewEngine
    {
        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            partialPath = GlobalizeViewPath(controllerContext, partialPath);
            return base.CreatePartialView(controllerContext, partialPath);
                //new WebFormView(controllerContext, partialPath, null, ViewPageActivator);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            viewPath = GlobalizeViewPath(controllerContext, viewPath);
            return base.CreateView(controllerContext, viewPath, masterPath);
        }

        private static string GlobalizeViewPath(ControllerContext controllerContext, string viewPath)
        {
            var request = controllerContext.HttpContext.Request;
            string lang = string.Empty;

            if (request.Cookies["language"] == null)
            {
                if (controllerContext.RequestContext.HttpContext.Request.UserLanguages != null &&
                    controllerContext.RequestContext.HttpContext.Request.UserLanguages.Length > 0)
                    lang = controllerContext.RequestContext.HttpContext.Request.UserLanguages[0].Substring(0, 2);
            }
            else
            {
                lang = request.Cookies["language"].Value.Substring(0, 2);
            }

            if (!string.IsNullOrEmpty(lang) &&
                !string.Equals(lang, "en", StringComparison.InvariantCultureIgnoreCase))
            {
                string localizedViewPath = Regex.Replace(viewPath, "/Views/", string.Format("/Views/Globalization/{0}/",lang));
                if (File.Exists(request.MapPath(localizedViewPath)))
                { viewPath = localizedViewPath; }
            }
            return viewPath;
        }
    }
}