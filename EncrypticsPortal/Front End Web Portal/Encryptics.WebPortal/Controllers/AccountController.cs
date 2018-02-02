using AutoMapper;
using Encryptics.WebPortal.ActionResults;
using Encryptics.WebPortal.Areas.UserAccount.Models;
using Encryptics.WebPortal.Filters;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.IdentityModel;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using Microsoft.Owin.Security;
using Microsoft.Web.WebPages.OAuth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using MyResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Controllers
{
    [AllowAnonymous]
    public class AccountController : PortalServiceAwareController
    {
        public AccountController(PortalServiceSoap portalService)
            : base(portalService)
        {
        }

        private string FormsAuthenticationTicket
        {
            get
            {
                var formsAuthenticationCookie = Response.Cookies[FormsAuthentication.FormsCookieName];

                var formsAuthenticationTicket = formsAuthenticationCookie != null
                                                    ? formsAuthenticationCookie.Value
                                                    : string.Empty;
                return formsAuthenticationTicket;
            }
        }

        private static bool ShowPortalLink
        {
            get
            {
                return false;
            }
        }

        private static DateTime CurrentExpirationTime
        {
            get { return DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes); }
        }

        // GET: /Account/JsonLogin
        [HttpGet, ValidateHttpAntiForgeryToken, Disabled(Order = 0)]
        public ActionResult JsonLogin(string returnUrl)
        {
            return GetJsonpAntiforgeryToken();
        }

        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            HandleMessages();

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Title = "Portal &mdash; Log In";
            ViewBag.SessionEnded = string.Empty;
            if(Session["Model"]!=null)
            {
                return View(Session["Model"] as LoginModel);
            }
            return View();
        }
       
        // POST: /Account/JsonLogin
        [HttpPost, ValidateHttpAntiForgeryToken, Disabled(Order = 0)]
        public async Task<JsonResult> JsonLogin(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (await LoginAsync(model.UserName, model.Password))
                {
                    Trace.TraceInformation("Login successful.");

                    return
                        Json(
                            new
                                {
                                    success = true,
                                    redirect = returnUrl,
                                    showPortalLink = ShowPortalLink,
                                    ticket = FormsAuthenticationTicket
                                });
                }
                Trace.TraceInformation("Login unsuccessful.");
            }

            // If we got this far, something failed
            return Json(new { errors = this.ErrorsFromModelState() });
        }
        
        // POST: /Account/Login
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model, string returnUrl)
        {
            Trace.TraceInformation("Entering Login");
            ViewBag.RetrnUrl = returnUrl;
            ViewBag.Title = "Portal &mdash; Log In";
            ViewBag.SessionEnded = string.Empty;
            Session["Model"] = model;
            string tenant = GetTenantId(model);
            if (!string.IsNullOrEmpty(tenant))
            {
                Session["IsLogin"] = true;
                AuthenticateUser(model);
                return null;
                //return RedirectToLocal(returnUrl);
            }
            else if (string.IsNullOrEmpty(model.Password) && !model.PasswordVisible)
            {
                model.PasswordVisible = true;
                return RedirectToAction("Login", "Account", new { area = string.Empty, returnUrl = "/" });

                // return View(model);
            }
            if (ModelState.IsValid)
            {
                // if (await ValidateCaptchaAsync() && await LoginAsync(model.UserName, model.Password))
                if (await ValidateCaptchaAsync())
                {
                   //Session["Model"] = model;
                   // string tenant = GetTenantId(model);
                   // if (!string.IsNullOrEmpty(tenant))
                   // {

                   //     AuthenticateUser(model);
                   //     return null;
                   //     //return RedirectToLocal(returnUrl);
                   // }
                   // else
                    if (await LoginAsync(model.UserName, model.Password))
                    {
                        
                        Trace.TraceInformation("Login successful.");

                        return RedirectToLocal(returnUrl);
                    }

                }

                Trace.TraceInformation("Login unsuccessful.");
            }

            Trace.TraceInformation("Error. Exiting Login.");

            //ConvertModelStateToErrorMessages();
            ViewData["ErrorMessage"] = "Login failed.";

            return View(model);
        }
       
        private void AuthenticateUser(LoginModel model)
        {
            Session["Model"] = model;
            Session["UserName"] = model.UserName;
            Session["auth"] = model.Tenant;
            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "Account/SetUserDetails" },
                model.Tenant);
        }
       
        private string GetTenantId(LoginModel model)
        {
            var cli = new WebClient();
            var jsonstring = JsonConvert.SerializeObject(model);
            string url = String.Format("http://idtp376/EncrypticsWebAPI/v2/accounts/IsTenantAvailable");
            cli.Headers[HttpRequestHeader.ContentType] = "application/json";
            var response = JsonConvert.DeserializeObject<LoginModel>(cli.UploadString(url, jsonstring));
            model.Tenant = response.Tenant.Trim();
            return model.Tenant;
        }

        //public bool Validate()
        //{
        //    string Response = Request["g-recaptcha-response"];//Getting Response String Append to Post Method
        //    bool Valid = false;
        //    //Request to Google Server
        //    HttpWebRequest req = (HttpWebRequest)WebRequest.Create
        //    (" https://www.google.com/recaptcha/api/siteverify?secret=YOUR SECRATE KEY &response=" + Response);
        //    try
        //    {
        //        //Google recaptcha Response
        //        using (WebResponse wResponse = req.GetResponse())
        //        {

        //            using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
        //            {
        //                string jsonResponse = readStream.ReadToEnd();

        //                JavaScriptSerializer js = new JavaScriptSerializer();
        //                MyObject data = js.Deserialize<MyObject>(jsonResponse);// Deserialize Json

        //                Valid = Convert.ToBoolean(data.success);
        //            }
        //        }

        //        return Valid;
        //    }
        //    catch (WebException ex)
        //    {
        //        throw ex;
        //    }
        //}

        // GET: /Account/TokenLogin
        [HttpGet, Disabled(Order = 0)]
        public async Task<ActionResult> AjaxTokenLogin(string id, string returnUrl)
        {
            var userName = await TokenLoginAsync(new TokenAuth { Token = id });

            if (!string.IsNullOrEmpty(userName))
            {
                Trace.TraceInformation("Login successful.");

                return new JsonpResult
                {
                    Data = new
                    {
                        success = true,
                        userName,
                        redirect = returnUrl,
                        showPortalLink = ShowPortalLink,
                        ticket = FormsAuthenticationTicket
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            Trace.TraceInformation("Login unsuccessful.");

            // If we got this far, something failed
            return new JsonpResult
                {
                    Data = new { errors = this.ErrorsFromModelState() },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
        }

        // GET: /Account/TokenLogin
        [HttpGet, Disabled(Order = 0)]
        public async Task<ActionResult> TokenLogin(string id, string returnUrl)
        {
            var userName = await TokenLoginAsync(new TokenAuth { Token = id });

            if (!string.IsNullOrEmpty(userName))
            {
                Trace.TraceInformation("Login successful.");

                return RedirectToAction("Manage", "UserHome", new { area = "UserAccount" });
            }

            return RedirectToAction("Login", new { returnUrl });
        }

        // POST: /Account/LogOff
        [HttpPost, ValidateHttpAntiForgeryToken, Disabled(Order = 0)]
        public async Task<ActionResult> JsonLogOff()
        {
            Trace.TraceInformation("Entering JsonLogoff");
            Trace.TraceInformation("Logging off " + HttpContext.User.Identity.Name);

            // clear authentication cookie
            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "") { Expires = DateTime.Now.AddYears(-1) };
            Response.Cookies.Add(authCookie);

            // clear session cookie (not necessary for your current problem but i would recommend you do it anyway)
            var sessionCookie = new HttpCookie("ASP.NET_SessionId", "") { Expires = DateTime.Now.AddYears(-1) };
            Response.Cookies.Add(sessionCookie);
            Thread.CurrentPrincipal =
                HttpContext.User = new GenericPrincipal(new LoggedOutIdentity(User.Identity.Name), new string[] { });
            Response.Headers.Remove(MyResources.AuthenticationTicketHeaderName);

            await LogUserOff();

            Trace.TraceInformation("Exiting JsonLogoff");

            return Json(new { success = true });
        }

        // POST: /Account/LogOff
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff()
        {
            Request.Cookies.Clear();

            await LogUserOff();

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // GET: /Account/JsonRegister
        [HttpGet, Disabled(Order = 0)]
        public ActionResult JsonRegister()
        {
            return GetJsonpAntiforgeryToken();
        }

        // POST: /Account/Register
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid && await ValidateCaptchaAsync())
            {
                // Attempt to register the user
                try
                {
                    //List<SecurityQuestion> securityQuestions = BuildSecurityQuestions(model);
                    if (await RegisterAsync(model.UserName, model.Password, true))
                    // , model.FirstName, model.LastName , securityQuestions))
                    {
                        TempData["username"] = model.UserName;

                        return RedirectToAction("Activate");
                    }
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.SecurityQuestionsList = BuildSecurityQuestionsList();

            ConvertModelStateToErrorMessages();

            return View(model);
        }

        // POST: /Account/JsonRegister
        [HttpPost, ValidateHttpAntiForgeryToken, Disabled(Order = 0)]
        public async Task<ActionResult> JsonRegister(RegisterModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    //List<SecurityQuestion> securityQuestions = BuildSecurityQuestions(model);
                    if (await RegisterAsync(model.UserName, model.Password, false))
                    //, model.FirstName, model.LastName, securityQuestions))
                    {
                        Trace.TraceInformation("Registration successful.");

                        return Json(new { success = true, redirect = returnUrl });
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError(string.Format("Error: {0}", e.Message));
                    Debug.Print("Stacktrace: {0}", e.StackTrace);

                    ModelState.AddModelError(string.Empty, MyResources.ActivationTimeOutErrorMessage);
                }
            }

            // If we got this far, something failed
            return Json(new { errors = this.ErrorsFromModelState() });
        }

        // GET: /Account/JsonActivate
        [HttpGet, Disabled(Order = 0)]
        public async Task<ActionResult> JsonActivate(int activationId, string activationCode, string hash)
        {
            var response = await _portalService.GetAccountActivationLinkStatusAsync(new GetAccountActivationLinkStatusRequest
            {
                TokenAuth = _tokenAuth,
                activation_id = activationId,
                activation_code = activationCode,
                hash = hash
            });

            if (response.GetAccountActivationLinkStatusResult.Status != AccountActivationLinkStatus.Pending)
            {
                Trace.TraceInformation("Account activation link status failed.");
                Debug.Print("Result: {0}.", response.GetAccountActivationLinkStatusResult);

                var errorMessage = Mapper.Map<AccountActivationLinkStatus, AccountActivationStatusError>(response.GetAccountActivationLinkStatusResult.Status);

                Debug.Print("Error to display: {0}.", errorMessage.GetDisplay());

                return new JsonpResult
                {
                    Data = new { success = false, error = errorMessage.GetDisplay() },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            Trace.TraceInformation("Account activation link status succeeded.");

            return new JsonpResult
            {
                Data = new
                {
                    success = true,
                    token = this.GetAntiForgeryToken(),
                    getPassword = !response.GetAccountActivationLinkStatusResult.PasswordExists,
                    downloadSoftware = !response.GetAccountActivationLinkStatusResult.IsSoftwarePreinstalled
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        // GET: /Account/AjaxCheckLogin
        [HttpGet]
        public ActionResult AjaxCheckLogin()
        {
            Trace.TraceInformation(string.Format("Checking status for: {0} ({1})", User.Identity.Name, Request.IsAuthenticated ? "authenticated" : "not authenticated"));
            var localPath = Request.Url == null ? string.Empty : Request.Url.LocalPath;
            var redirect = !(localPath.Contains("Login") || localPath.Contains("SessionEnded"));

            return new JsonpResult
            {
                Data = new { success = Request.IsAuthenticated, redirect },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        // GET: /Account/Activate
        [HttpGet, AllowAnonymous]
        public ActionResult Activate()
        {
            ViewBag.UserName = TempData["username"];

            return View();
        }

        // POST: /Account/JsonActivate
        [HttpPost, ValidateHttpAntiForgeryToken, Disabled(Order = 0)]
        public async Task<ActionResult> JsonActivate(int id, string code)
        {
            var userName = await ActivateAccountAsync(id, code);

            if (!string.IsNullOrEmpty(userName))
            {
                Trace.TraceInformation("Activation successful.");

                return Json(new
                    {
                        success = true,
                        userName,
                        showPortalLink = ShowPortalLink,
                        ticket = FormsAuthenticationTicket
                    });
            }

            Trace.TraceInformation("Activation unsuccessful.");

            // If we got this far, something failed
            return Json(new { errors = this.ErrorsFromModelState() });
        }

        // POST: /Account/JsonActivate
        [HttpPost, ValidateHttpAntiForgeryToken, Disabled(Order = 0)]
        public async Task<ActionResult> JsonActivateWithPassword(ActivationWithPasswordModel activationAccount)
        {
            if (ModelState.IsValid)
            {
                var userName = await ActivateWithPasswordAsync(activationAccount);

                if (userName.Length > 0)
                {
                    return
                        Json(
                            new
                                {
                                    success = true,
                                    userName,
                                    showPortalLink = ShowPortalLink,
                                    ticket = FormsAuthenticationTicket
                                });
                }

                ModelState.AddModelError(string.Empty, MyResources.CouldNotLogNewUserInMessage);
            }

            return Json(new { success = false, errors = this.ErrorsFromModelState() });
        }

        // GET: /Account/ActivateAccount
        [HttpGet]
        public async Task<ActionResult> ActivateAccount(int id, string code, string hash)
        {
            var request = new AccountActivationRequest(_tokenAuth, id, code);

            Trace.TraceInformation("Entering ActivateAccount");
            Debug.Print("ID: {0}", id);
            Debug.Print("Code: {0}", code);
            Debug.Print("Hash: {0}", hash);

            var response = await _portalService.AccountActivationAsync(request);

            Debug.Print("result: {0}", response.AccountActivationResult/*.GetDisplay()*/);
            Debug.Print("token status: {0}", response.TokenAuth.Status);
            Debug.Print("token: {0}", response.TokenAuth.Token);
            
            switch (response.AccountActivationResult)
            {
                case AccountActivationStatus.Success:
                    await TokenLoginAsync(response.TokenAuth);
                    
                    Session["autodownload"] = 1;

                    return RedirectToAction("Index", "Home", new { autodownload = 1 });
                case AccountActivationStatus.Needs_Password:
                    var activateAccountModel = new ActivationWithPasswordModel
                    {
                        ActivationCode = code,
                        Hash = hash,
                        UserId = id
                    };

                    return View("ActivatePassword", activateAccountModel);
                case AccountActivationStatus.Link_Expired:
                    TempData["ErrorMessage"] = "Link expired.";
                    break;
                case AccountActivationStatus.Removed:
                    TempData["ErrorMessage"] = "Account removed.";
                    break;
                case AccountActivationStatus.Used:
                    TempData["ErrorMessage"] = "Account already activated.";
                    break;
                default:
                    TempData["ErrorMessage"] = "Account activation failed.";
                    break;
            }

            return RedirectToAction("Login");
        }

        // POST: /Account/ActivateAccount
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> ActivateAccount(ActivationWithPasswordModel model)
        {
            Trace.TraceInformation("Entering ActivateWithPasswordAsync");
            Debug.Print("ID: {0}", model.UserId);
            Debug.Print("Code: {0}", model.ActivationCode);
            Debug.Print("Hash: {0}", model.Hash);

            //if (!(ModelState.IsValid && await ValidateCaptchaAsync()))
            if (!ModelState.IsValid)
            {
                ConvertModelStateToErrorMessages();

                return View("ActivatePassword", model);
            }

            var hashedPassword = PasswordHasher.HashPassword(model.Password);
            var request = new AccountActivationWithPasswordRequest(_tokenAuth, model.UserId,
                                                                   model.ActivationCode,
                                                                   model.Hash, hashedPassword);
            var response = await _portalService.AccountActivationWithPasswordAsync(request);

            Debug.Print("result: {0}", response.AccountActivationWithPasswordResult);
            Debug.Print("token status: {0}", response.TokenAuth.Status);
            Debug.Print("token: {0}", response.TokenAuth.Token);

            return CheckActivationStatus(response.AccountActivationWithPasswordResult,
                                         model.ActivationCode, model.Hash, model.UserId);

            //if (response.TokenAuth.Status == TokenStatus.NewToken)
            //{
            //    switch (response.AccountActivationWithPasswordResult)
            //    {
            //        case AccountActivationStatus.Success:
            //            //await TokenLoginAsync(response.TokenAuth);

            //            return RedirectToAction("Login");
            //            //return RedirectToAction("Index", "Home");
            //        case AccountActivationStatus.Needs_Password:
            //            var activateAccountModel = new ActivationWithPasswordModel
            //            {
            //                ActivationCode = model.ActivationCode,
            //                Hash = model.Hash,
            //                UserId = model.UserId
            //            };

            //            return View("ActivatePassword", activateAccountModel);
            //        case AccountActivationStatus.Link_Expired:
            //            TempData["ErrorMessage"] = "Link expired.";
            //            break;
            //        case AccountActivationStatus.Removed:
            //            TempData["ErrorMessage"] = "Account removed.";
            //            break;
            //        case AccountActivationStatus.Used:
            //            TempData["ErrorMessage"] = "Account already activated.";
            //            break;
            //        default:
            //            TempData["ErrorMessage"] = "Account activation failed.";
            //            break;
            //    }
            //}
            //else
            //{
            //    TempData["ErrorMessage"] = "Account activation failed.";
            //}

            //return RedirectToAction("Login");
        }

        private ActionResult CheckActivationStatus(/*TokenAuth responseToken, */AccountActivationStatus activationStatus, string activationCode, string hash, int userId)
        {
            //if (responseToken.Status == TokenStatus.NewToken)
            //{
            switch (activationStatus)
            {
                case AccountActivationStatus.Success:
                    //await TokenLoginAsync(response.TokenAuth);

                    TempData["SuccessMessage"] = "Activation successful. You may now log in to your account.";

                    return RedirectToAction("Login");
                //return RedirectToAction("Index", "Home");
                case AccountActivationStatus.Needs_Password:
                    var activateAccountModel = new ActivationWithPasswordModel
                    {
                        ActivationCode = activationCode,
                        Hash = hash,
                        UserId = userId
                    };

                    return View("ActivatePassword", activateAccountModel);
                case AccountActivationStatus.Link_Expired:
                    TempData["ErrorMessage"] = "Link expired.";
                    break;
                case AccountActivationStatus.Removed:
                    TempData["ErrorMessage"] = "Account removed.";
                    break;
                case AccountActivationStatus.Used:
                    TempData["ErrorMessage"] = "Account already activated.";
                    break;
                default:
                    TempData["ErrorMessage"] = "Account activation failed.";
                    break;
            }
            //}
            //else
            //{
            //    TempData["ErrorMessage"] = "Account activation failed.";
            //}

            return RedirectToAction("Login");
        }

        // GET: /Account/Resend
        [HttpGet]
        public ActionResult Resend()
        {
            return View();
        }

        // GET: /Account/JsonResend
        [HttpGet, AllowAnonymous, Disabled(Order = 0)]
        public ActionResult JsonResend()
        {
            return GetJsonpAntiforgeryToken();
        }

        // POST: /Account/Resend
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Resend(ResendActivationModel model)
        {
            model.IsCaptchaValid = await ValidateCaptchaAsync();

            if (model.IsCaptchaValid)
            {
                model.Resent = await ResendActivationAsync(model.UserName);
            }

            if (/*model.Resent &&*/ model.IsCaptchaValid)
            {
                ViewData["SuccessMessage"] = string.Format("Activation link successfully sent to {0}", model.UserName);
            }
            else
            {
                ConvertModelStateToErrorMessages();
            }

            return View();
        }

        // POST: /Account/JsonResend
        [HttpPost, ValidateHttpAntiForgeryToken, Disabled(Order = 0)]
        public async Task<ActionResult> JsonResend(ResendActivationModel model)
        {
            model.Resent = await ResendActivationAsync(model.UserName);

            Debug.Print("Resend successful: {0}", model.Resent);

            return !model.Resent ? Json(new { errors = this.ErrorsFromModelState() }) : Json(new { success = true });
        }

        // GET: /Account/ForgotPwd
        [HttpGet, Disabled(Order = 0)]
        public ActionResult AjaxForgotPwd()
        {
            return PartialView("_ForgotPwdPartial");
        }

        // POST: /Account/ForgotPwd
        [HttpPost, Disabled(Order = 0)]
        public async Task<ActionResult> AjaxForgotPwd(ForgotPasswordModel model)
        {
            if (await RequestPasswordResetAsync(model, OriginSite.WEBSITE))
            {
                return PartialView("_FormSubmitSuccessful");
            }

            return PartialView("_ForgotPwdPartial");
        }

        // GET: /Account/ForgotPwd
        [HttpGet]
        public ActionResult ForgotPwd()
        {
            //return RedirectPermanent(WebConfigurationManager.AppSettings["resetPasswordLink"]);

            return View();
        }

        // POST: /Account/ForgotPwd
        [HttpPost, ValidateAntiForgeryToken/*, Disabled*/]
        public async Task<ActionResult> ForgotPwd(ForgotPasswordModel model)
        {
            if (/*ModelState.IsValid &&*/ await ValidateCaptchaAsync())
            {
                if (await RequestPasswordResetAsync(model, OriginSite.PORTAL))
                {
                    TempData["SuccessMessage"] =
                        "Success! An email with a link to reset your password has been sent to your email address.";

                    return RedirectToAction("Login");
                }
            }

            ConvertModelStateToErrorMessages();

            return View(model);
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public async Task<ActionResult> ResetPassword(int id, string code, string hash)
        {
            var response = await _portalService.GetResetPasswordLinkStatusAsync(new GetResetPasswordLinkStatusRequest
                {
                    TokenAuth = _tokenAuth,
                    request_id = id,
                    code = code,
                    hash = hash
                });

            if (response.GetResetPasswordLinkStatusResult == ResetPasswordLinkStatus.Pending)
            {
                var resetPasswordModel = new ResetPasswordModel
                    {
                        ActivationCode = code,
                        Hash = hash,
                        UserId = id
                    };

                return View(resetPasswordModel);
            }

            TempData["ErrorMessage"] = "Unable to reset password. Contact support.";

            return RedirectToAction("Login");
        }

        // POST: /Account/ResetPassword
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordModel model)
        {
            Debug.Print("Model valid: {0}", ModelState.IsValid);

            if (ModelState.IsValid && await ValidateCaptchaAsync())
            {
                Trace.TraceInformation(string.Format("UserID: {0}", model.UserId));
                Trace.TraceInformation(string.Format("ActivationCode: {0}", model.ActivationCode));
                Trace.TraceInformation(string.Format("Hash: {0}", model.Hash));

                var response =
                    await _portalService.UpdatePasswordAsync(new UpdatePasswordRequest
                        {
                            TokenAuth = _tokenAuth,
                            code = model.ActivationCode,
                            request_id = model.UserId,
                            hash = HttpUtility.UrlDecode(model.Hash),
                            password = PasswordHasher.HashPassword(model.Password)
                        });

                Trace.TraceInformation(string.Format("response: {0}", response.UpdatePasswordResult));
                Trace.TraceInformation(string.Format("token status: {0}", response.TokenAuth.Status));

                if (response.UpdatePasswordResult == UpdatePasswordStatus.Success &&
                    (response.TokenAuth.Status == TokenStatus.Succes || response.TokenAuth.Status == TokenStatus.NoError))
                {
                    TempData["SuccessMessage"] = MyResources.PasswordSuccessfullyUpdatedMessage;
                }
                else
                {
                    TempData["ErrorMessage"] = MyResources.PasswordResetErrorMessage;
                }
            }
            else
            {
                ConvertModelStateToErrorMessages();

                return View(model);
            }

            return RedirectToAction("Login");
        }

        // GET: /Account/AjaxResetPwd
        [HttpGet, Disabled(Order = 0)]
        public async Task<ActionResult> AjaxResetPwd(int id, string code, string hash)
        {
            var response = await _portalService.GetResetPasswordLinkStatusAsync(new GetResetPasswordLinkStatusRequest
                {
                    TokenAuth = _tokenAuth,
                    request_id = id,
                    code = code,
                    hash = hash
                });

            if (response.GetResetPasswordLinkStatusResult == ResetPasswordLinkStatus.Pending)
            {
                Trace.TraceInformation("Activation succeeded.");
                ViewBag.AntiForgeryToken = this.GetAntiForgeryToken();
                string cookieToken, formToken;

                AntiForgery.GetTokens(null, out cookieToken, out formToken);
                ViewBag.CookieToken = cookieToken;
                ViewBag.FormToken = formToken;

                return PartialView("_ResetPwdPartial");
            }

            var errorMessage = Mapper.Map<ResetPasswordLinkStatus, ResetPasswordStatusError>(response.GetResetPasswordLinkStatusResult);
            Trace.TraceInformation(string.Format("Activation failed: {0}.", errorMessage.GetDisplay()));

            return PartialView("_ResetPwdError", errorMessage);
        }

        // POST: /Account/AjaxResetPwd
        [HttpPost, ValidateHttpAntiForgeryToken, Disabled(Order = 0)]
        public async Task<ActionResult> AjaxResetPwd(ResetPasswordModel model)
        {
            Debug.Print("Model valid: {0}", ModelState.IsValid);

            if (ModelState.IsValid)
            {
                Trace.TraceInformation(string.Format("UserID: {0}", model.UserId));
                Trace.TraceInformation(string.Format("ActivationCode: {0}", model.ActivationCode));
                Trace.TraceInformation(string.Format("Hash: {0}", model.Hash));

                var response =
                    await _portalService.UpdatePasswordAsync(new UpdatePasswordRequest
                        {
                            TokenAuth = _tokenAuth,
                            code = model.ActivationCode,
                            request_id = model.UserId,
                            hash = HttpUtility.UrlDecode(model.Hash),
                            password = PasswordHasher.HashPassword(model.Password)
                        });

                Trace.TraceInformation(string.Format("response: {0}", response.UpdatePasswordResult));
                Trace.TraceInformation(string.Format("token status: {0}", response.TokenAuth.Status));

                if (response.UpdatePasswordResult != UpdatePasswordStatus.Success ||
                    (response.TokenAuth.Status != TokenStatus.Succes && response.TokenAuth.Status != TokenStatus.NoError))
                {
                    // TODO: Need to have the ability to lookup the UserName for this users based on the 
                    // userid in the model which comes from the portal service.
                    // WebSecurity.ChangePassword(model.)
                    ModelState.AddModelError(string.Empty, MyResources.PasswordResetErrorMessage);
                }
                else
                {
                    ViewBag.SuccessMessage = MyResources.PasswordSuccessfullyUpdatedMessage;
                    return PartialView("_FormSubmitSuccessful");
                }
            }

            Response.AddHeader("RequestToken", this.GetAntiForgeryToken());
            return PartialView("_ResetPwdPartial");
        }

        // GET: /Account/SessionEnded
        [HttpGet,AuthActionFilter]
        public ViewResult SessionEnded(string returnUrl)
        {

            //if (AuthConfig.LogOut && HttpContext.User.Identity.IsAuthenticated)
            if(Session["IsLogin"]==null && HttpContext.User.Identity.IsAuthenticated)
            {
                string username = string.Empty;
                if(string.IsNullOrEmpty(HttpContext.User.Identity.Name))
                {
                   username= ((System.Security.Claims.ClaimsPrincipal)HttpContext.User).Claims.Where(x => x.Type.Contains("email")).FirstOrDefault().Value;
                }
                else
                {
                    username = HttpContext.User.Identity.Name;
                }
                string tenant = GetTenantId(new LoginModel { UserName = username });
                Session["Expire"] = true;
                Session["returnurl"] = returnUrl;
                string callbackUrl = Url.Action("SignOutCallback", "Home", routeValues: null, protocol: Request.Url.Scheme);
                HttpContext.GetOwinContext().Authentication.SignOut(
                    new AuthenticationProperties { RedirectUri = callbackUrl },
                 tenant, Microsoft.Owin.Security.WsFederation.WsFederationAuthenticationDefaults.AuthenticationType, Microsoft.Owin.Security.Cookies.CookieAuthenticationDefaults.AuthenticationType);
               
            }
            //AuthConfig.LogOut = false;
           // AuthConfig.AuthType = string.Empty;

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Title = "Session Expired &mdash; Log In";
            ViewBag.SessionEnded = "For security purposes you must log in to continue";

            return View("Login");
        }

        // GET: /Account/ActivateDevice
        [HttpGet]
        public async Task<ViewResult> ActivateDevice(int id, string code)
        {
            var dummyToken = _tokenAuth;
            var request = new UpdateDeviceActivationRequest(dummyToken, id, code);

            var response = await _portalService.UpdateDeviceActivationAsync(request);

            if (response.TokenAuth.Status != TokenStatus.Succes)
            {
                ViewBag.Message = "Token error returned. Device not activated.";
            }
            else
            {
                switch (response.UpdateDeviceActivationResult)
                {
                    case UpdateDeviceActivationStatus.Bad_Data:
                        ViewBag.Message = "Bad data error returned. Device not activated.";
                        break;
                    case UpdateDeviceActivationStatus.Failed:
                        ViewBag.Message = "Fail error returned. Device not activated.";
                        break;
                    case UpdateDeviceActivationStatus.Link_Expired:
                        ViewBag.Message = "Link expired error returned. Device not activated.";
                        break;
                    case UpdateDeviceActivationStatus.Removed:
                        ViewBag.Message = "Activation code removed. Device not activated.";
                        break;
                    case UpdateDeviceActivationStatus.Success:
                        ViewBag.Message = "Device successfully activated.";
                        break;
                    case UpdateDeviceActivationStatus.Used:
                        ViewBag.Message = "Code already in use. Device not activated.";
                        break;
                }
            }

            return View();
        }

        #region OAuth Stuff

        /*
        // POST: /Account/Disassociate
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            MessageHelper.ChangePasswordMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (
                    var scope = new TransactionScope(TransactionScopeOption.Required,
                                                     new TransactionOptions
                                                         {
                                                             IsolationLevel = IsolationLevel.Serializable
                                                         }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = MessageHelper.ChangePasswordMessageId.REMOVE_LOGIN_SUCCESS;
                    }
                }
            }

            return RedirectToAction("Manage", new {Message = message});
        }

        // POST: /Account/ExternalLogin
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new {ReturnUrl = returnUrl}));
        }

        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result =
                OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new {ReturnUrl = returnUrl}));
            
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }

            // User is new, ask for their desired membership name
            string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;

            return View("ExternalLoginConfirmation",
                        new RegisterExternalLoginModel {UserName = result.UserName, ExternalLoginData = loginData});
        }

        // POST: /Account/ExternalLoginConfirmation
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider;
            string providerUserId;

            if (User.Identity.IsAuthenticated ||
                !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (var db = new UsersContext())
                {
                    UserProfile user =
                        db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile {UserName = model.UserName});
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, false);

                        return RedirectToLocal(returnUrl);
                    }

                    ModelState.AddModelError("UserName",
                                             Resources
                                                 .AccountController_ExternalLoginConfirmation_User_name_already_exists__Please_enter_a_different_user_name_);
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // GET: /Account/ExternalLoginFailure
        [HttpGet]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
        
        [HttpGet, ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [Authorize, HttpGet, ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = (from account in accounts
                                                  let clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider)
                                                  select new ExternalLogin
                                                      {
                                                          Provider = account.Provider,
                                                          ProviderDisplayName = clientData.DisplayName,
                                                          ProviderUserId = account.ProviderUserId,
                                                      }).ToList();

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 ||
                                       OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }
        */

        #endregion

        #region Helpers

        private async Task LogUserOff()
        {
            // string callbackUrl = Url.Action("Index", "Home", routeValues: null, protocol: Request.Url.Scheme);

            //System.IdentityModel.Services.WSFederationAuthenticationModule.FederatedSignOut(null, new Uri("https://localhost:44356/Home/Index"));

            //  HttpContext.GetOwinContext().Authentication.SignOut(Session["auth"].ToString(), Microsoft.Owin.Security.Cookies.CookieAuthenticationDefaults.AuthenticationType);
            if (Session["auth"]!=null)
            {
               
               
                string callbackUrl = Url.Action("SignOutCallback", "Home", routeValues: null, protocol: Request.Url.Scheme);
                HttpContext.GetOwinContext().Authentication.SignOut(
                    new AuthenticationProperties { RedirectUri = callbackUrl },
                   Session["auth"].ToString(), Microsoft.Owin.Security.WsFederation.WsFederationAuthenticationDefaults.AuthenticationType, Microsoft.Owin.Security.Cookies.CookieAuthenticationDefaults.AuthenticationType);
            }
            Session.Abandon();
         //   Session["Logoff"] = true;
            FormsAuthentication.SignOut();

            var request = new UserLogoutRequest
            {
                TokenAuth = _tokenAuth,
                entity_id = _encrypticsUser.EntityId,
                user_id = _encrypticsUser.UserId
            };

            var response = await _portalService.UserLogoutAsync(request);

            if (response.TokenAuth.Status != TokenStatus.Succes || !response.UserLogoutResult)
            {
                // TODO: Add error messages that get displayed after redirect
                Trace.TraceWarning("Error logging out.");
            }
        }
        
        private void CreateFormsAuthenticationTicket(string userName, string token, long entityId, long userId)
        {
            Trace.TraceInformation("Entering CreateFormsAuthenticationTicket.");

            Debug.Print("User authenticated: {0}", User.Identity.IsAuthenticated);

            var userData = JsonConvert.SerializeObject(new { Token = token, EntityId = entityId, UserId = userId ,AuthenticationType=Session["auth"]});

            Trace.TraceInformation("Creating FormsAuthenticationTicket.");

            var authTicket =
                new FormsAuthenticationTicket(2, userName, DateTime.Now, CurrentExpirationTime, false, userData);

            Debug.Print("User authenticated: {0}", User.Identity.IsAuthenticated);

            string encryptedToken = FormsAuthentication.Encrypt(authTicket);

            HttpContext.User = Thread.CurrentPrincipal = new RolePrincipal(new FormsIdentity(authTicket));


            if (encryptedToken == null)
            {
                Trace.TraceWarning("FormsAuthentication token is null.");
                ModelState.AddModelError("", MyResources.PasswordProvidedIsIncorrectMessage);
            }
            else
            {
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedToken)
                {
                    HttpOnly = true
                };

                Response.AppendCookie(authCookie);
            }

            Trace.TraceInformation("Exiting CreateFormsAuthenticationTicket.");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return
                        MyResources
                            .UserNameExistsMessage;

                case MembershipCreateStatus.DuplicateEmail:
                    return
                        MyResources.MembershipDuplicateEmailErrorMessage;

                case MembershipCreateStatus.InvalidPassword:
                    return MyResources.MembershipInvalidPasswordErrorMessage;

                case MembershipCreateStatus.InvalidEmail:
                    return MyResources.MembershipInvalidEmailErrorMessage;

                case MembershipCreateStatus.InvalidAnswer:
                    return MyResources.MembershipInvalidAnswerErrorMessage;

                case MembershipCreateStatus.InvalidQuestion:
                    return MyResources.MembershipInvalidQuestionErrorMessage;

                case MembershipCreateStatus.InvalidUserName:
                    return MyResources.MembershipInvalidUserNameErrorMessage;

                case MembershipCreateStatus.ProviderError:
                    return
                        MyResources.MembershipProviderErrorErrorMessage;

                case MembershipCreateStatus.UserRejected:
                    return
                        MyResources.MembershipUserRejectedErrorMessage;

                default:
                    return
                        MyResources.MembershipDefaultUnknownErrorMessage;
            }
        }

        /*private List<SecurityQuestion> BuildSecurityQuestions(RegisterModel model)
        {
            string answer01Hash = GetSecurityQuestionAnswerHash(model.SecurityAnswer01);
            string answer02Hash = GetSecurityQuestionAnswerHash(model.SecurityAnswer02);
            string answer03Hash = GetSecurityQuestionAnswerHash(model.SecurityAnswer03);

            var securityQuestions = new List<SecurityQuestion>
                {
                    new SecurityQuestion
                        {
                            AnswerHash = answer01Hash,
                            QuestionNumber = 1,
                            QuestionText = model.SecurityQuestion01.Trim()
                        },
                    new SecurityQuestion
                        {
                            AnswerHash = answer02Hash,
                            QuestionNumber = 2,
                            QuestionText = model.SecurityQuestion02.Trim()
                        },
                    new SecurityQuestion
                        {
                            AnswerHash = answer03Hash,
                            QuestionNumber = 3,
                            QuestionText = model.SecurityQuestion03.Trim()
                        }
                };
            return securityQuestions;
        }

        private string GetSecurityQuestionAnswerHash(string answer)
        {
            byte[] utf8EncodedAnswer = _hasher.Digest(Encoding.UTF8.GetBytes(answer));

            return Convert.ToBase64String(utf8EncodedAnswer).Trim();
        }*/

        private static SelectList BuildSecurityQuestionsList()
        {
            var listItems = new List<SelectListItem>
                {
                    new SelectListItem
                        {
                            Text =
                                MyResources.SecurityQuestion1,
                            Value =
                                MyResources.SecurityQuestion1
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion2,
                            Value =
                                MyResources
                                    .SecurityQuestion2
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion3,
                            Value =
                                MyResources
                                    .SecurityQuestion3
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion4,
                            Value =
                                MyResources
                                    .SecurityQuestion4
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion5,
                            Value =
                                MyResources
                                    .SecurityQuestion5
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion6,
                            Value =
                                MyResources
                                    .SecurityQuestion6
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion7,
                            Value =
                                MyResources
                                    .SecurityQuestion7
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion8,
                            Value =
                                MyResources
                                    .SecurityQuestion8
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion9,
                            Value =
                                MyResources
                                    .SecurityQuestion9
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion10,
                            Value =
                                MyResources
                                    .SecurityQuestion10
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion11,
                            Value =
                                MyResources
                                    .SecurityQuestion11
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion12,
                            Value =
                                MyResources
                                    .SecurityQuestion12
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion13,
                            Value =
                                MyResources
                                    .SecurityQuestion13
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion14,
                            Value =
                                MyResources
                                    .SecurityQuestion14
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion15,
                            Value =
                                MyResources
                                    .SecurityQuestion15
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion16,
                            Value =
                                MyResources
                                    .SecurityQuestion16
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion17,
                            Value =
                                MyResources
                                    .SecurityQuestion17
                        },
                    new SelectListItem
                        {
                            Text =
                                MyResources
                                    .SecurityQuestion18,
                            Value =
                                MyResources
                                    .SecurityQuestion18
                        }
                };
            var list = new SelectList(listItems, "Value", "Text");
            return list;
        }

        private async Task<bool> LoginAsync(string userName, string password)
        {
            Trace.TraceInformation("Entering LoginAsync");
            var hashedPassword = PasswordHasher.HashPassword(password);
            LoginModel m = Session["Model"] as LoginModel;
            m.Password = hashedPassword;
            Trace.TraceInformation(string.Format("UserName: {0}", userName));

            try
            {
                var loginRequest = new UserLoginRequest(_tokenAuth, userName, hashedPassword);
               // var loginResponse = await _portalService.UserLoginAsync(loginRequest);
                var cli = new WebClient();
                var jsonstring = JsonConvert.SerializeObject(m);
                string url = String.Format("http://idtp376/EncrypticsWebAPI/v2/accounts/login");
                cli.Headers[HttpRequestHeader.ContentType] = "application/json";
                var loginResponse = cli.UploadString(url, jsonstring);
                var uAccount = JsonConvert.DeserializeObject<UserAccount>(loginResponse);
                WebHeaderCollection myWebHeaderCollection = cli.ResponseHeaders;
                var userAccount = Mapper.Map<UserAccount, UserAccountModel>(uAccount);
                    var tokenAuth = new TokenAuth();
                if (myWebHeaderCollection.GetValues("tokenauth_id") != null)
                {
                    tokenAuth.Token = myWebHeaderCollection.GetValues("tokenauth_id")[0];
                    tokenAuth.Status = Convert.ToInt32(myWebHeaderCollection.GetValues("tokenauth_status")[0]);
                }
                Debug.Print("Token Status: {0}", tokenAuth.Status);
                if (userAccount != null)
                {

                    if (tokenAuth.Status == TokenStatus.NewToken)
                    {
                        CreateFormsAuthenticationTicket(userName, tokenAuth.Token, userAccount.EntityId,
                                                        userAccount.UserId);
                        InitializeSession(userAccount.UserId, userAccount.EntityId, tokenAuth.Token);

                        Trace.TraceInformation("LoginAsync: login successful.");

                        return true;
                    }

                    if (userAccount.IsLockedOut)
                    {
                        Trace.TraceInformation(string.Format("LoginAsync: user ({0}) could not log in (locked out).", userName));

                        ModelState.AddModelError(string.Empty, MyResources.ErrorMessageAccontLockedOut);

                        return false;
                    }
                }
            }
            catch (EndpointNotFoundException endpointNotFoundException)
            {
                Trace.TraceWarning("Could not log in to server (endpoint unreachable).");
                Trace.TraceError(endpointNotFoundException.Message);

                ModelState.AddModelError(string.Empty, MyResources.ErrorMessageCoultNotLogIn);

                return false;
            }

            Trace.TraceInformation("LoginAsync: login NOT successful.");
            ModelState.AddModelError("", MyResources.PasswordProvidedIsIncorrectMessage);

            return false;
        }

        private async Task<bool> RegisterAsync(string userName, string password, bool registerFromPortal, string firstName = null,
                                               string lastName = null/*,
                                               IEnumerable<SecurityQuestion> questions*/)
        {
            Trace.TraceInformation("Entering RegisterAsync");
            Trace.TraceInformation(string.Format("Registering {0}", userName));
            var hashedPassword = PasswordHasher.HashPassword(password);

            var status = await _portalService.AccountRegistrationAsync(userName, firstName, lastName, hashedPassword, null, registerFromPortal ? OriginSite.PORTAL : OriginSite.WEBSITE);
            //questions.ToArray());

            Trace.WriteLine(string.Format("Registration status: {0}", status));

            switch (status)
            {
                case AccountRegistrationStatus.Success:
                    Trace.WriteLine(string.Format("Registered username: {0}.", userName));
                    Trace.TraceInformation("Exiting RegisterAsync -- returning true");
                    //return true;
                    break;
                case AccountRegistrationStatus.Failed:
                    Trace.WriteLine(string.Format("Failed registering {0}.", userName));
                    ModelState.AddModelError("", string.Format(MyResources.ErrorRegisteringAccountMessage, ConfigurationManager.AppSettings["SupportEmail"]));
                    return false;
                case AccountRegistrationStatus.Exists:
                    Trace.WriteLine(string.Format("Account {0} exists.", userName));
                    ModelState.AddModelError("", MyResources.AccountAlreadyExistsMessage);
                    ModelState.AddModelError("", MyResources.PleaseResendActivationMessage);
                    break;
            }

            //Trace.TraceInformation("Exiting RegisterAsync -- returning false");

            return true;
        }

        private async Task<bool> ResendActivationAsync(string userName)
        {
            Trace.IndentLevel++;
            Trace.TraceInformation("Entering ResendActivationAsync");
            bool resent = false;
            try
            {
                resent = await _portalService.ResendActivationAsync(userName);

                Debug.Print("resent = {0}", resent);

                if (!resent)
                {
                    ModelState.AddModelError(string.Empty, MyResources.CouldNotResendActivationMessage);
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, MyResources.ErrorSendingAccountActivationMessage);
                Trace.WriteLine("  " + e.Message);
                Debug.Print("  " + e.StackTrace);
            }

            Trace.TraceInformation(string.Format("Exiting ResendActivationAsync: {0}", resent));


            Trace.IndentLevel--;

            return resent;
        }

        private async Task<string> TokenLoginAsync(TokenAuth tokenAuth)
        {
            var tokenLoginRequest = new TokenLoginRequest(tokenAuth,
                                                          tokenAuth.Token);

            var tokenLoginResponse =
                await _portalService.TokenLoginAsync(tokenLoginRequest);

            var userAccount = Mapper.Map<UserAccount, UserAccountModel>(tokenLoginResponse.TokenLoginResult);

            Debug.Print("Token Status: {0}",
                        TokenStatus.ErrorMessageDictionary[tokenLoginResponse.TokenAuth.Status]);

            if (userAccount != null && userAccount.UserName != null &&
                tokenLoginResponse.TokenAuth.Status == TokenStatus.NewToken)
            {
                Debug.Print("TokenLogin succesful.");
                var userName = userAccount.UserName;

                Debug.Print("User Account: {0}", userName);
                var token = tokenLoginResponse.TokenAuth.Token;

                CreateFormsAuthenticationTicket(userName, token, userAccount.EntityId, userAccount.UserId);
                InitializeSession(userAccount.UserId, userAccount.EntityId, token);

                Trace.TraceInformation(string.Format("Exiting ActivateAccountAsync. Returning username {0}.", userName));

                return userName;
            }

            ModelState.AddModelError("", MyResources.ActivationWasSuccessfullButLogInFailedMessage);

            return string.Empty;
        }

        private async Task<string> ActivateAccountAsync(int id, string code)
        {
            Trace.TraceInformation("Entering ActivateAccountAsync");

            Debug.Print("Id: {0}", id);
            Debug.Print("Code: {0}", code);

            var accountActivationRequest = new AccountActivationRequest(_tokenAuth, id, code);

            var accountActivationResponse =
                await _portalService.AccountActivationAsync(accountActivationRequest);

            Debug.Print("Activation Result: {0}",
                        accountActivationResponse.AccountActivationResult);
            Debug.Print("Token Status: {0}",
                        TokenStatus.ErrorMessageDictionary[accountActivationResponse.TokenAuth.Status]);

            if (accountActivationResponse.AccountActivationResult == AccountActivationStatus.Success &&
                accountActivationResponse.TokenAuth.Status == TokenStatus.NewToken)
            {
                Trace.TraceInformation("Exiting ActivateAccountAsync. Returning token login results.");
                return await TokenLoginAsync(accountActivationResponse.TokenAuth);
            }

            ModelState.AddModelError("", MyResources.AccountActivationFailedErrorMessage);
            ModelState.AddModelError("", MyResources.AccountMayAlreadyBeActivatedErrorMessage);

            Trace.TraceInformation("Exiting ActivateAccountAsync. Returning empty string.");

            return string.Empty;
        }

        private async Task<string> ActivateWithPasswordAsync(ActivationWithPasswordModel activationAccount)
        {
            Trace.TraceInformation("Entering ActivateWithPasswordAsync");
            var hashedPassword = PasswordHasher.HashPassword(activationAccount.Password);

            Debug.Print("ID: {0}", activationAccount.UserId);
            Debug.Print("Code: {0}", activationAccount.ActivationCode);
            Debug.Print("Hash: {0}", activationAccount.Hash);

            var request = new AccountActivationWithPasswordRequest(_tokenAuth, activationAccount.UserId,
                                                                   activationAccount.ActivationCode,
                                                                   activationAccount.Hash, hashedPassword);

            var response =
                await _portalService.AccountActivationWithPasswordAsync(request);

            Debug.Print("result: {0}", response.AccountActivationWithPasswordResult);
            Debug.Print("token status: {0}", response.TokenAuth.Status);
            Debug.Print("token: {0}", response.TokenAuth.Token);
            if (response.TokenAuth.Status == TokenStatus.NewToken)
            {
                switch (response.AccountActivationWithPasswordResult)
                {
                    case AccountActivationStatus.Bad_Data:
                    case AccountActivationStatus.Failed:
                        ModelState.AddModelError(string.Empty, MyResources.AccountActivationFailedTryAgainMessage);
                        break;
                    case AccountActivationStatus.Needs_Password:
                        ModelState.AddModelError(string.Empty, MyResources.EnterPasswordAgainMessage);
                        break;
                    case AccountActivationStatus.Success:
                        Trace.TraceInformation("Activating with password successful.");
                        return await TokenLoginAsync(response.TokenAuth);
                }
            }

            Trace.TraceInformation("Activating with password had errors.");

            return string.Empty;
        }

        private async Task<bool> RequestPasswordResetAsync(ForgotPasswordModel model, OriginSite originSite)
        {
            if (ModelState.IsValid)
            {
                Trace.TraceInformation(string.Format("Sending password reset email to: {0}", model.UserName));

                var response =
                    await _portalService.InsertResetPasswordRequestAsync(new InsertResetPasswordRequestRequest
                        {
                            TokenAuth = _tokenAuth,
                            email_address = model.UserName,
                            origin_site = originSite
                        });

                Debug.Print("result: {0}", response.InsertResetPasswordRequestResult);
                Debug.Print("token status: {0}", response.TokenAuth.Status);

                if (!response.InsertResetPasswordRequestResult ||
                    (response.TokenAuth.Status != TokenStatus.Succes && response.TokenAuth.Status != TokenStatus.NoError))
                {
                    ModelState.AddModelError(string.Empty, MyResources.PasswordResetUnsuccessfulErrorMessage);
                    return false;
                }

                ViewBag.SuccessMessage =
                    MyResources.PasswordChangeSuccessfullMessage;

                return true;
            }

            return false;
        }

        private void InitializeSession(long userId, long entityId, string token)
        {
            Trace.TraceInformation("Initializing session");

            Session["UserId"] = userId;
            Session["EntityId"] = entityId;
            Session["Token"] = token;

        }
       
        protected override void Dispose(bool disposing)
        {
           // if (Session["auth"] != null && (!Request.IsAuthenticated))
            {

                //HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/Account/SetUserDetails" },
                //         "adfs");
                //    //if (Request.FilePath == "/Account/SessionEnded" && HttpContext.Application["isStarted"] != null)
                //    //{
                //    //    HttpContext.Application.Remove("isStarted");
                //    //}

            }
            Trace.TraceInformation(string.Format("Disposing {0} class.", GetType().Name));

            if (disposing)
            {
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
            }

            Trace.TraceInformation(string.Format("{0} disposed of.", GetType().Name));

            base.Dispose(disposing);
        }
        public ActionResult SetUserDetails()
        {
            var cli = new WebClient();
            var jsonstring = JsonConvert.SerializeObject(Session["Model"]);
            string url = String.Format("http://idtp376/EncrypticsWebAPI/v2/accounts/getUserIdentifier");
            cli.Headers[HttpRequestHeader.ContentType] = "application/json";
            try
            {
                var uAccount = JsonConvert.DeserializeObject<UserAccount>(cli.UploadString(url, jsonstring));

                var userAccount = Mapper.Map<UserAccount, UserAccountModel>(uAccount);
                WebHeaderCollection myWebHeaderCollection = cli.ResponseHeaders;
                var tokenAuth = new TokenAuth();
                if (myWebHeaderCollection.GetValues("tokenauth_id") != null)
                {
                    tokenAuth.Token = myWebHeaderCollection.GetValues("tokenauth_id")[0];
                    tokenAuth.Status = Convert.ToInt32(myWebHeaderCollection.GetValues("tokenauth_status")[0]);
                }
                Debug.Print("Token Status: {0}", tokenAuth.Status);

                if (userAccount != null)
                {
                    if (tokenAuth.Status == TokenStatus.NewToken)
                    {
                        CreateFormsAuthenticationTicket(userAccount.UserName, tokenAuth.Token, userAccount.EntityId,
                                                        userAccount.UserId);
                        InitializeSession(userAccount.UserId, userAccount.EntityId, tokenAuth.Token);

                        Trace.TraceInformation("LoginAsync: login successful.");
                        return RedirectToAction("Index", "Home");

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
       
    
        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        #endregion
    }
}