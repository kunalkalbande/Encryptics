using Encryptics.WebPortal.ActionResults;
using Encryptics.WebPortal.IdentityModel;
using Microsoft.Owin.Security;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
            Trace.TraceInformation(string.Format("Initializing {0} class.", GetType().Name));
        }

        [HttpGet]
        public ActionResult AjaxSetLanguage(string lang)
        {
            Response.Cookies.Add(new HttpCookie("language", lang));

            Trace.TraceInformation(string.Format("Language is now set to: {0}", lang));

            return new JsonpResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            ViewBag.Message = "Welcome to the Encryptics Web Portal.";

            if (User.Identity.IsAuthenticated)
            {
                try
                {

                    var encrypticsUser = EncrypticsPrincipal.CurrentEncrypticsUser;

                    if (encrypticsUser.CompanyCount > 1)
                    {
                        return RedirectToAction("Dashboard", "CompanyAdminHome", new { area = "CompanyAdmin" });
                    }

                    const string companyDashboardPermission = "Company/CompanyHome/Dashboard";

                    await encrypticsUser.SetViewPermissionsAsync(new[] { companyDashboardPermission });

                    return encrypticsUser.HasPermission(companyDashboardPermission) ?
                        RedirectToAction("Dashboard", "CompanyHome", new { area = "Company", entityId = encrypticsUser.EntityId }) :
                        RedirectToAction("Dashboard", "UserHome", new { area = "UserAccount" });
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.Message);
                    Debug.Print(e.StackTrace);

                    RedirectToAction("Dashboard", "UserHome", new { area = "UserAccount" });
                }
            }

            return RedirectToAction("Login", "Account", new { area = string.Empty, returnUrl = Request.Url == null ? string.Empty : Request.Url.PathAndQuery });
        }
        public ActionResult SignOutCallback()
        {
            //AuthConfig.LogOut = false;
            //AuthConfig.AuthType = string.Empty;
            if (Session["Expire"]!=null ||TempData["Expire"]!=null)
            {
                string returnurl = null;

                if (Session["Expire"] != null)
                {
                    if (Session["returnurl"] !=null)
                    {
                        returnurl= Session["returnurl"].ToString() ;
                        TempData["returnurl"] = returnurl;
                        Session.Remove("returnurl");
                    }
                    else if(TempData["returnurl"] !=null)
                    {
                        returnurl = TempData["returnurl"].ToString();
                    }

                    Session.Remove("Expire");

                    TempData["Expire"] = true;
                }
                return RedirectToAction("SessionEnded", "Account", new { area = string.Empty, returnUrl = returnurl == null ? string.Empty : returnurl});

            }
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        [HttpGet]
        public ActionResult Contact()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Error()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}