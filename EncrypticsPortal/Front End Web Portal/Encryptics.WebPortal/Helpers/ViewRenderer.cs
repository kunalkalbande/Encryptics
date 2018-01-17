using System.IO;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Helpers
{
    public class ViewRenderer
    {
        public static string RenderViewToString(string viewPath, ControllerContext context, object model = null,
                                                bool partial = false)
        {
            // first find the ViewEngine for this view
            ViewEngineResult viewEngineResult = partial
                                                    ? ViewEngines.Engines.FindPartialView(context, viewPath)
                                                    : ViewEngines.Engines.FindView(context, viewPath, null);

            if (viewEngineResult == null)
                throw new FileNotFoundException("View cannot be found.");

            // get the view and attach the model to view data
            IView view = viewEngineResult.View;
            context.Controller.ViewData.Model = model;

            string result;

            using (var sw = new StringWriter())
            {
                var ctx = new ViewContext(context, view,
                                          context.Controller.ViewData,
                                          context.Controller.TempData,
                                          sw);
                view.Render(ctx, sw);
                result = sw.ToString();
            }

            return result;
        }
    }
}