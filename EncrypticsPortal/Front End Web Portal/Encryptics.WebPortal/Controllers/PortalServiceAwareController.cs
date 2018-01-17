using System.Collections.Specialized;
using System.Web.Helpers;
using Encryptics.WebPortal.Filters;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.IdentityModel;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Routing;
using MyResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Controllers
{
    [ExceptionHandlingFilter, TraceActionFilter, AuthorizeAction]
    public class PortalServiceAwareController : WebPortalController
    {
        protected PortalServiceSoap _portalService;
        protected TokenAuth _tokenAuth;
        protected EncrypticsPrincipal _encrypticsUser;
        private static readonly string[] _basePermissions = new[] { "CompanyAdmin/Search/SearchResults" };
        protected bool _hasPBPAccess;
        protected bool _hasZDPAccess;

        public PortalServiceAwareController(PortalServiceSoap portalService)
        {
            _portalService = portalService;
        }

        protected override IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
        {
            var httpRequest = requestContext.HttpContext.Request;
            var requesetHeaders = httpRequest.Headers;
            var requestForm = httpRequest.Form;

            if (requestContext.HttpContext.Session != null)
                _tokenAuth = new TokenAuth
                {
                    Token = (string)requestContext.HttpContext.Session["token"],
                    Nonce = GetNonce(requesetHeaders, requestForm)
                };

            _encrypticsUser = EncrypticsPrincipal.CurrentEncrypticsUser;

            if (_encrypticsUser != null)
            {
                var requestCookies = httpRequest.Cookies;
                if ((requestCookies["tzo"] != null || requesetHeaders["x-tzo"] != null))
                    _encrypticsUser.UTCOffset = -1 * int.Parse(requestCookies["tzo"] != null ? requestCookies["tzo"].Value : requesetHeaders["x-tzo"]);

                if ((requestCookies["dst"] != null || requesetHeaders["x-dst"] != null))
                    _encrypticsUser.UsesDST = bool.Parse(requestCookies["dst"] != null ? requestCookies["dst"].Value : requesetHeaders["x-dst"]);


                _encrypticsUser.SetViewPermissions(_basePermissions);
            }

            return base.BeginExecute(requestContext, callback, state);
        }

        protected static string GetNonce(NameValueCollection requesetHeaders, NameValueCollection requestForm)
        {
            return requesetHeaders.AllKeys.Contains(MvcApplication.REQUESTVERIFICATIONTOKEN) ? requesetHeaders[MvcApplication.REQUESTVERIFICATIONTOKEN] : requestForm[AntiForgeryConfig.CookieName];
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Trace.TraceInformation(string.Format("Disposing {0} class.", GetType().Name));
                try
                {
                    ((PortalServiceSoapClient)_portalService).Close();
                }
                catch (Exception closeException)
                {
                    try
                    {
                        Trace.TraceInformation("Close failed. Attempting Abort.");
                        Trace.TraceError(closeException.Message);
                        Debug.Print(closeException.StackTrace);
                        ((PortalServiceSoapClient)_portalService).Abort();
                    }
                    catch (Exception abortException)
                    {
                        Trace.TraceInformation("Abort failed. Giving up.");
                        Trace.TraceError(abortException.Message);
                        Debug.Print(abortException.StackTrace);
                    }
                }

                Trace.TraceInformation(string.Format("{0} disposed of.", GetType().Name));
            }

            base.Dispose(disposing);
        }

        protected async Task InitializeViewBagAsync(long entityId)
        {
            var companyName = await GetCompanyName(entityId);

            InitializeViewBag(entityId, companyName);
        }

        //protected void InitializeViewBagAsync(long entityId, string companyName)
        //{
        //    ViewBag.CompanyId = entityId;
        //    ViewBag.CompanyName = companyName;
        //}

        protected void InitializeViewBag(long entityId, string companyName)
        {
            ViewBag.CompanyId = entityId;
            ViewBag.CompanyName = companyName;
        }

        protected DateTime GetUTCOffsetToday()
        {
            return UTCHelper.GetUTCOffsetToday(_encrypticsUser.UTCOffset);
        }

        protected async Task SetViewPermissionsAsync(long entityId, IEnumerable<string> permissions)
        {
            await _encrypticsUser.SetViewPermissionsAsync(entityId, permissions);
        }

        private async Task<string> GetCompanyName(long entityId)
        {
            var companyDetailsResponse =
                await _portalService.GetCompanyDetailsAsync(new GetCompanyDetailsRequest(_tokenAuth, entityId));

            if (companyDetailsResponse.TokenAuth.Status == TokenStatus.Succes &&
                companyDetailsResponse.GetCompanyDetailsResult != null)
            {
                _hasPBPAccess = companyDetailsResponse.GetCompanyDetailsResult.IsDLPVisible;
                _hasZDPAccess = companyDetailsResponse.GetCompanyDetailsResult.IsAntiMalwareVisible;

                return companyDetailsResponse.GetCompanyDetailsResult.Name;
            }

            if (!ViewData.ContainsKey("ErrorMessage"))
                ViewData.Add("ErrorMessage", "Could not retrieve company details.");

            return string.Empty;
        }

        protected void ConvertModelStateToErrorMessages()
        {
            if (ModelState.Values.Any(v => v.Errors.Count > 0))
            {
                ViewData["ErrorMessage"] =
                    AggregateModelStateErrors();

            }
        }

        protected /*IEnumerable<*/string/*>*/ AggregateModelStateErrors()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage)).Aggregate((c, s) => c.EndsWith(".") ? c + s : c + ". " + s);
            //return ModelState.Values.Select(v => v.Errors.Select(m => m.ErrorMessage).Aggregate((c, s) => c + ". " + s));
        }

        protected string AggregateModelStateErrorsToHtmlString()
        {
            return ModelState.Values.SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage)
                                            .Aggregate((current, next) => current + "<br />" + next);
        }

        // validate Google reCAPTCHA response
        protected async Task<bool> ValidateCaptchaAsync()
        {
            var secret = WebConfigurationManager.AppSettings["CaptchaSecret"];
            var captchaResponse = Request.Form["g-recaptcha-response"];
            var captchaRequestUrl = string.Format(@"https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, captchaResponse);
            string responseJson;
            bool captchaEnabled;

            bool.TryParse(WebConfigurationManager.AppSettings["CaptchaEnabled"], out captchaEnabled);

            if (!captchaEnabled) return true;

            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(captchaRequestUrl);
                
                //Google recaptcha Response
                using (var webResponse = await webRequest.GetResponseAsync())
                {
                    var responseStream = webResponse.GetResponseStream();

                    if (responseStream == null) return false;

                    using (var readStream = new StreamReader(responseStream))
                    {
                        responseJson = readStream.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("ValidateCaptchaAsync: " + ex.Message);
                Trace.TraceError("ValidateCaptchaAsync: " + ex.StackTrace);

                return false;
            }

            dynamic responseObject = JsonConvert.DeserializeObject(responseJson);

            if (!(bool)responseObject.success) ModelState.AddModelError(string.Empty, MyResources.ErrorMessageCouldNotValidateCaptcha);

            return responseObject.success;
        }
    }
}
