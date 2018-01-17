using System.Web.Optimization;

namespace Encryptics.WebPortal
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.cookie.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/jquery.unobtrusive*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/shared.css",
                "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/custom-css").Include(
                "~/Content/custom.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));

            bundles.Add(new StyleBundle("~/Content/themes/encryptics/css").Include(
                        "~/Content/themes/encryptics/jquery-ui.css",
                        "~/Content/themes/encryptics/jquery-ui.structure.css",
                        "~/Content/themes/encryptics/jquery-ui.theme.css"));

            bundles.Add(new ScriptBundle("~/bundles/rainbowvis").Include(
                "~/Scripts/rainbowvis.js"));

            bundles.Add(new ScriptBundle("~/bundles/confirm-dialog").Include(
                "~/Scripts/confirm-dialog.js"));

            bundles.Add(new ScriptBundle("~/bundles/message-box").Include(
                "~/Scripts/message-box.js"));

            bundles.Add(new ScriptBundle("~/bundles/info-messages").Include(
                "~/Scripts/info-messages.js"));

            bundles.Add(new ScriptBundle("~/bundles/ajax").Include(
                "~/Scripts/ajax.js"));

            bundles.Add(new ScriptBundle("~/bundles/tabs").Include(
                "~/Scripts/tabs.js"));

            bundles.Add(new ScriptBundle("~/bundles/search").Include(
                "~/Scripts/search.js"));

            //bundles.Add(new ScriptBundle("~/bundles/mobile-check").Include(
            //    "~/Scripts/mobile-check.js"));
        }
    }
}