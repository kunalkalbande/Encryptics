using Encryptics.WebPortal.ActionResults;
using Encryptics.WebPortal.Helpers;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Controllers
{
    public abstract class WebPortalController : Controller
    {
        protected void HandleMessages()
        {
            if (TempData.ContainsKey("ErrorMessage"))
            {
                ViewData.Add("ErrorMessage", TempData["ErrorMessage"]);
            }

            if (TempData.ContainsKey("SuccessMessage"))
            {
                ViewData.Add("SuccessMessage", TempData["SuccessMessage"]);
            }

            TempData.Clear();
        }

        protected JsonpResult GetJsonpAntiforgeryToken()
        {
            return new JsonpResult
                {
                    Data = new { success = true, token = this.GetAntiForgeryToken() },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
        }
    }
}
