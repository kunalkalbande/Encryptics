using AutoMapper;
using Encryptics.WebPortal.ActionResults;
using Encryptics.WebPortal.Areas.UserAccount.Models;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.Filters;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.WsFederation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.SessionState;
using MyResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Areas.UserAccount.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly), Authorize]
    public class UserHomeController : PortalServiceAwareController
    {
        public string MobileDeviceModel
        {
            get { return Request.Browser.MobileDeviceModel.ToLower(); }
        }

        public int UsageTablePageSize
        {
            get { return MobileDeviceModel == "unknown" ? 9 : 7; }
        }

        public UserHomeController(PortalServiceSoap portalService)
            : base(portalService)
        {
        }

        // GET: /UserAccount/UserHome
        [HttpGet]
        public async Task<ActionResult> Dashboard()
        {
            UserAccountModel accountModel = await RetrieveUserAccountDetails();

            HandleMessages();

            ViewBag.AutoDownload = 0;

            if (accountModel != null)
            {
                ViewBag.UserNameMD5Hash = CalculateMd5Hash(accountModel.UserName);

                if (Session["autodownload"] != null && (int)Session["autodownload"] == 1)
                {
                    ViewBag.AutoDownload = 1;
                    Session.Remove("autodownload");

                    ViewData["SuccessMessage"] = "";
                }
                
                return View(accountModel);
            }

            ViewBag.Message(MyResources.CouldNotRetrieveAccountDetailsErrorMessage);

            return View();
        }

        // GET: /UserAccount/UserHome/JsonAccountDetails/
        [HttpGet]
        public async Task<ActionResult> JsonAccountDetails()
        {
            UserAccountModel accountModel = await RetrieveUserAccountDetails();

            if (accountModel != null)
            {
                string partialViewHtml =
                    ViewRenderer.RenderViewToString("_UserAccountDetailsPartial", ControllerContext,
                                                    accountModel, true);

                Debug.Print("Returning HTML: {0}", partialViewHtml);

                return Json(new { success = true, Data = partialViewHtml }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { errors = new[] { MyResources.CouldNotRetrieveAccountDetailsErrorMessage } });
        }

        [HttpGet]
        public JsonpResult AjaxUpdateAccount()
        {
            return GetJsonpAntiforgeryToken();
        }

        // POST: /UserAccount/UserHome/AjaxUpdateAccount
        [HttpPost, ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxUpdateAccount(EditableUserAccountModel userAccount)
        {
            if (ModelState.IsValid)
            {
                var updateUserResponse = await UpdateUserAccountDetails(userAccount);

                if (!updateUserResponse.UpdateUserResult)
                {
                    ModelState.AddModelError(string.Empty, MyResources.AccountDetailsNotUpdatedErrorMessage);
                }
                else
                {
                    return Json(new { success = true });
                }
            }

            // If we got this far, something failed
            return Json(new { success = false, errors = this.ErrorsFromModelState() });
        }

        // GET: /UserAccount/UserHome/JsonUsage/
        [HttpGet]
        public ActionResult JsonUsage(int? page)
        {
            var devices = RetrieveUsageDetails();

            var pageableData = new PageableViewModel<UsageSummaryModel>
                {
                    PageSize = UsageTablePageSize,
                    CurrentPage = page ?? 1,
                    DataItems = devices,
                    TableBodyPartialView = "_UsageSummaryListPartial"
                };

            CalculateReferencePath();

            string partialViewHtml =
                ViewRenderer.RenderViewToString("_UsageSummaryPartial", ControllerContext, pageableData, true);

            Debug.Print("Returning HTML: {0}", partialViewHtml);

            return Json(new { success = true, Data = partialViewHtml }, JsonRequestBehavior.AllowGet);
        }

        // GET: /UserAccount/UserHome/JsonDownloads
        [HttpGet, AllowAnonymous]
        public ActionResult JsonDownloads()
        {
            InitializeDownloadLinks();

            string partialViewHtml = ViewRenderer.RenderViewToString("_DownloadsPartial", ControllerContext,
                                                                     partial: true);

            Debug.Print("Returning HTML: {0}", partialViewHtml);

            return Json(new { success = true, Data = partialViewHtml }, JsonRequestBehavior.AllowGet);
        }

        // GET: /UserAccount/UserHome/Manage
        [HttpGet]
        public async Task<ActionResult> Manage()
        {
            ViewData["ErrorMessage"] = TempData["ErrorMessage"];

            var request = new GetAccountDetailsRequest(_tokenAuth, _encrypticsUser.EntityId,
                                                       _encrypticsUser.UserId);

            GetAccountDetailsResponse response = await _portalService.GetAccountDetailsAsync(request);
            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetAccountDetailsResult != null)
            {
                EditableUserAccountModel accountDetails =
                    Mapper.Map<PortalService.UserAccount, EditableUserAccountModel>(response.GetAccountDetailsResult);

                return View(accountDetails);
            }

            TempData["ErrorMessage"] = MyResources.CouldNotRetrieveUserAccount;

            return RedirectToAction("Index", "Home", new { area = string.Empty });
        }

        // POST: /UserAccount/UserHome/Manage
        [HttpPost]
        public async Task<ActionResult> Manage(EditableUserAccountModel model)
        {
            if (ModelState.IsValid)
            {
                var request = new UpdateUserContactInfoRequest
                    {
                        TokenAuth = _tokenAuth,
                        first_name = model.FirstName,
                        last_name = model.LastName,
                        user_contact_info = Mapper.Map<ContactInfoModel, ContactInfo>(model.ContactInfo),
                        user_id = _encrypticsUser.UserId
                    };

                var response = await _portalService.UpdateUserContactInfoAsync(request);

                if (response.UpdateUserContactInfoResult && response.TokenAuth.Status == TokenStatus.Succes)
                {
                    return RedirectToAction(string.Empty);
                }

                ViewData["ErrorMessage"] = "Could not update account details.";
            }
            else
            {
                ViewData["ErrorMessage"] = AggregateModelStateErrorsToHtmlString();
            }

            return View(model);
        }

        // GET: /UserAccount/UserHome/JsonChangePassword
        [HttpGet]
        public ActionResult JsonChangePassword(ChangePasswordMessageId? message)
        {
            Debug.Print("messageId = {0}", message);

            ViewBag.StatusMessage = message != null ? message.GetDisplay() : string.Empty;
            string viewBagMessage = ViewBag.StatusMessage ?? string.Empty;
            Debug.Print("message = {0}", viewBagMessage);

            CalculateReferencePath();
            ViewBag.Image = "Security";

            string partialViewHtml =
                ViewRenderer.RenderViewToString("_ManagePasswordPartial", ControllerContext, new LocalPasswordModel(),
                                                true);

            string antiForgeryToken = this.GetAntiForgeryToken();
            Debug.Print("antiForgeryToken = {0}", antiForgeryToken);
            Debug.Print("partialViewHtml = {0}", partialViewHtml);

            return Json(new { success = true, token = antiForgeryToken, html = partialViewHtml },
                        JsonRequestBehavior.AllowGet);
        }

        // POST: /UserAccount/UserHome/JsonChangePassword
        [HttpPost, ValidateAntiForgeryToken, ValidateHttpAntiForgeryToken]
        public async Task<JsonResult> JsonChangePassword(LocalPasswordModel model)
        {
            Debug.Print("UserName = {0}", User.Identity.Name);

            if (ModelState.IsValid && await ValidateCaptchaAsync())
            {
                var updatePasswordRequest = new UpdateUserPasswordRequest
                    {
                        TokenAuth = _tokenAuth,
                        new_password = PasswordHasher.HashPassword(model.NewPassword),
                        old_password = PasswordHasher.HashPassword(model.OldPassword),
                        username = User.Identity.Name
                    };

                var response =
                    await _portalService.UpdateUserPasswordAsync(updatePasswordRequest);

                if (response.UpdateUserPasswordResult != UpdateUserPasswordStatus.Success || response.TokenAuth.Status != TokenStatus.Succes)
                {
                    string errorMessage;
                    switch (response.UpdateUserPasswordResult)
                    {
                        case UpdateUserPasswordStatus.History_Violation:
                            errorMessage = "Cannot reuse previous password.";
                            break;
                        case UpdateUserPasswordStatus.Old_Password_Incorrect:
                            errorMessage = "Old password does not match";
                            break;
                        default:
                            errorMessage = MyResources.CurrentPasswordIsIncorrectOrTheNewPasswordIsInvalidMessage;
                            break;
                    }
                    Trace.TraceError(string.Format("Token error = {0}",
                                                   TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]));
                    ModelState.AddModelError("", errorMessage);
                }
                else
                {
                    ViewBag.StatusMessage = ChangePasswordMessageId.ChangePasswordSuccess.GetDisplay();

                    ViewBag.Image = "Success";

                    var partialViewHtml =
                        ViewRenderer.RenderViewToString("_ManagePasswordPartial",
                                                        ControllerContext, new LocalPasswordModel(), true);

                    Debug.Print("Returning HTML: {0}", partialViewHtml);

                    return Json(new { success = true, Data = partialViewHtml });
                }
            }

            // If we got this far, something failed
            return Json(new { success = false, errors = this.ErrorsFromModelState() });
        }

        // GET: /UserAccount/UserHome/ManagePassword
        public ActionResult ManagePassword()
        {
            ViewBag.Image = "Security";

            return PartialView("_ManagePasswordPartial");
        }

        // GET: /UserAccount/UserHome/ChangePassword
        [HttpGet]
        public ActionResult ChangePassword(ChangePasswordMessageId? message)
        {
            ViewBag.StatusMessage = message != null ? message.GetDisplay() : string.Empty;
            ViewBag.ReturnUrl = Url.Action("ChangePassword");

            return View();
        }

        // POST: /UserAccount/UserHome/ChangePassword
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(LocalPasswordModel model)
        {
            Debug.WriteLine("UserName = {0}", (object)User.Identity.Name);
            Debug.WriteLine("Old Password = {0}", (object)model.OldPassword);
            Debug.WriteLine("New Password = {0}", (object)model.NewPassword);

            if (ModelState.IsValid)
            {
                var updatePasswordRequest = new UpdateUserPasswordRequest
                    {
                        TokenAuth = new TokenAuth
                            {
                                Token = (string)Session["token"]
                            },
                        new_password = PasswordHasher.HashPassword(model.NewPassword),
                        old_password = PasswordHasher.HashPassword(model.OldPassword),
                        username = User.Identity.Name
                    };

                var response =
                    await _portalService.UpdateUserPasswordAsync(updatePasswordRequest);

                if (response.TokenAuth.Status != TokenStatus.Succes)
                {
                    Trace.TraceWarning(string.Format("Token status: {0}",
                                                     TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]));
                    ModelState.AddModelError("", MyResources.CouldNotVerifyPasswordErrorMessage);
                }
                else if (response.UpdateUserPasswordResult != UpdateUserPasswordStatus.Success)
                {
                    string errorMessage;
                    switch (response.UpdateUserPasswordResult)
                    {
                        case UpdateUserPasswordStatus.History_Violation:
                            errorMessage = "Cannot reuse previous password.";
                            break;
                        case UpdateUserPasswordStatus.Old_Password_Incorrect:
                            errorMessage = "Old password does not match.";
                            break;
                        default:
                            errorMessage = MyResources.CurrentPasswordIsIncorrectOrTheNewPasswordIsInvalidMessage;
                            break;
                    }
                    Trace.TraceInformation(errorMessage);
                    ModelState.AddModelError("", errorMessage);
                }
                else
                {
                    Trace.TraceInformation("Password change successful.");
                    return RedirectToAction("ChangePassword", new
                        {
                            message = ChangePasswordMessageId.ChangePasswordSuccess.GetDisplay()
                        });
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /UserAccount/UserHome/Downloads
        [HttpGet]
        public ActionResult Downloads()
        {
            InitializeDownloadLinks();

            return PartialView("_DownloadsPartial");
        }

        // GET: /UserAccount/UserHome/ViewUsage
        [HttpGet]
        public ActionResult ViewUsage(int? page)
        {
            var usage = RetrieveUsageDetails();

            var pageableData = new PageableViewModel<UsageSummaryModel>
                {
                    PageSize = UsageTablePageSize,
                    CurrentPage = page ?? 1,
                    DataItems = usage,
                    TableBodyPartialView = "_UsageSummaryListPartial"
                };

            return PartialView("_UsageSummaryPartial", pageableData);
        }

        private async Task<UpdateUserResponse> UpdateUserAccountDetails(UserAccountModel userAccount)
        {
            var updateUserRequest = new UpdateUserRequest
                {
                    TokenAuth = new TokenAuth
                        {
                            Token = (string)Session["token"]
                        },
                    entity_id = _encrypticsUser.EntityId,
                    first_name = userAccount.FirstName,
                    last_name = userAccount.LastName,
                    user_id = _encrypticsUser.UserId
                };

            var updateUserResponse = await _portalService.UpdateUserAsync(updateUserRequest);

            return updateUserResponse;
        }

        private void InitializeDownloadLinks()
        {
            ViewBag.WindowsLink1 = WebConfigurationManager.AppSettings["WinDownloadLink"];
            ViewBag.WindowsLink2 = WebConfigurationManager.AppSettings["WinDownload2Link"];
            ViewBag.MacLink = WebConfigurationManager.AppSettings["MacDownloadLink"];
            ViewBag.AndroidLink = WebConfigurationManager.AppSettings["AndroidDownloadLink"];
            ViewBag.iOSLink = WebConfigurationManager.AppSettings["iOSDownloadLink"];
        }

        private async Task<UserAccountModel> RetrieveUserAccountDetails()
        {
            var accountDetailRequest =
                new GetAccountDetailsRequest(_tokenAuth, _encrypticsUser.EntityId, _encrypticsUser.UserId);
            var cli = new WebClient();
            string jsonstring = string.Empty;
            if (Session["auth"] == null)
                jsonstring = JsonConvert.SerializeObject(new LoginModel { UserName = User.Identity.Name });
            else
                jsonstring = JsonConvert.SerializeObject(new LoginModel { UserName = Session["UserName"].ToString() });
            string url = String.Format("http://idtp376/EncrypticsWebAPI/v2/accounts/getUserIdentifier");
            cli.Headers[HttpRequestHeader.ContentType] = "application/json";
            PortalService.UserAccount useracc=null;
            try
            {
               useracc = JsonConvert.DeserializeObject<PortalService.UserAccount>(cli.UploadString(url, jsonstring));
            }
            catch (Exception ex)
            {

            }
            GetAccountDetailsResponse response = await _portalService.GetAccountDetailsAsync(accountDetailRequest);
            response.TokenAuth.Status = TokenStatus.Succes;
            response.GetAccountDetailsResult = useracc;
            return response.TokenAuth.Status == TokenStatus.Succes
                       ? Mapper.Map<PortalService.UserAccount, UserAccountModel>(response.GetAccountDetailsResult)
                       : null;
        }

        private IEnumerable<UsageSummaryModel> RetrieveUsageDetails()
        {
            var getDeviceListRequest = new GetUserUsageSummaryByMonthRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = _encrypticsUser.EntityId,
                    user_id = _encrypticsUser.UserId
                };

            GetUserUsageSummaryByMonthResponse response =
                _portalService.GetUserUsageSummaryByMonth(getDeviceListRequest);

            IEnumerable<UsageSummaryModel> usage =
                Mapper.Map<UsageSummary[], IEnumerable<UsageSummaryModel>>(response.GetUserUsageSummaryByMonthResult)
                      .ToList().OrderByDescending(us => us.Year).ThenByDescending(us => us.Month);

            return usage;
        }

        private void CalculateReferencePath()
        {
            string removeString = Request.ApplicationPath ?? string.Empty;
            int index = Request.FilePath.IndexOf(removeString, StringComparison.CurrentCultureIgnoreCase);

            Debug.Print("Request.FilePath: {0}", Request.FilePath);
            Debug.Print("AppPath: {0}", Request.ApplicationPath);
            Debug.Print("removeString: {0}", removeString);
            Debug.Print("index: {0}", index);

            string requestPath = (index < 0)
                                     ? Request.FilePath
                                     : Request.FilePath.Remove(index, removeString.Length);

            Debug.Print("RequestPath: {0}", requestPath);

            ViewBag.RequestPath = requestPath;
        }

        private static string CalculateMd5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }
    }
}