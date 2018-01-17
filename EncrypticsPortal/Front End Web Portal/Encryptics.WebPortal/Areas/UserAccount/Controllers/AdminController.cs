using AutoMapper;
using Encryptics.WebPortal.Areas.UserAccount.Models;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using System;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using MyResources = Encryptics.WebPortal.Properties.Resources;
using ServiceContactInfo = Encryptics.WebPortal.PortalService.ContactInfo;
using ServiceUserAccount = Encryptics.WebPortal.PortalService.UserAccount;

namespace Encryptics.WebPortal.Areas.UserAccount.Controllers
{
    public class AdminController : PortalServiceAwareController
    {
        private static readonly IEnumerable<string> _dashboardPermissions = new[]
            {
                "UserAccount/Admin/Manage",
                "UserAccount/Admin/UnlockAccount", 
                "UserAccount/Admin/ActivateAccount", 
                "UserAccount/Admin/SuspendAccount",
                "UserAccount/Admin/TransferAccount",
                "UserAccount/Admin/RemoveAccount", 
                "UserAccount/Admin/AssignLicense", 
                "UserAccount/Admin/RemoveLicense",
                "UserAccount/Admin/ResetPassword", 
                "UserAccount/Admin/ChangeAccountRole"
            };

        public AdminController(PortalServiceSoap portalService)
            : base(portalService)
        {
        }

        public string MobileDeviceModel
        {
            get { return Request.Browser.MobileDeviceModel.ToLower(); }
        }

        public int UsageTablePageSize
        {
            get { return MobileDeviceModel == "unknown" ? 9 : 7; }
        }

        public int DeviceListPageSize
        {
            get { return MobileDeviceModel == "unknown" || MobileDeviceModel == "ipad" ? 4 : 1; }
        }

        public async Task<ActionResult> LocateActiveAccountByEmail(long entityId, string email)
        {
            var userId = await FindAccountIdByEmailAsync(entityId, email);

            if (userId != null)
            {
                return RedirectToAction("ActiveAccount", new { entityId, userId });
            }

            TempData["ErrorMessage"] = "Account not found.";

            return RedirectToAction("Dashboard", "CompanyHome", new { area = "Company", entityId });
        }

        // GET: /UserAccount/Admin/ActiveAccount
        [HttpGet]
        public async Task<ActionResult> ActiveAccount(long entityId, long userId, int? usagePage, int? devicePage)
        {
            var model = new AccountOverviewModel();

            ViewBag.CompanyId = entityId;

            HandleMessages();

            await SetModel(entityId, userId, usagePage, devicePage, model);

            var webPortalRole = model.Account.PrimaryRole;

            ViewBag.RoleDropdownList = await
                                       BuildRoleDropDownListAsync(model.Account.EntityId,
                                                                  webPortalRole == null ? 0 : webPortalRole.RoleId);

            await SetViewPermissionsAsync(entityId, _dashboardPermissions);

            return View(model);
        }

        // GET: /UserAccount/Admin/SuspendedAccount
        [HttpGet]//[AuthorizeAction(true)]
        public async Task<ActionResult> SuspendedAccount(long entityId, long userid, long userId, int? usagePage, int? devicePage)
        {
            var model = new AccountOverviewModel();

            ViewBag.CompanyId = entityId;

            HandleMessages();

            await SetModel(entityId, userId, usagePage, devicePage, model);

            var webPortalRole = model.Account.PrimaryRole;

            ViewBag.RoleDropdownList = await BuildRoleDropDownListAsync(model.Account.EntityId,
                                                                  webPortalRole == null ? 0 : webPortalRole.RoleId);

            await SetViewPermissionsAsync(entityId, _dashboardPermissions);

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> PendingAccount(long entityId, string requestEmail)
        {
            var request = new GetPendingAccountsRequest(_tokenAuth, entityId, requestEmail,
                                                        PendingAccountSearchType.EMAIL, PendingAccountItemListSort.Email,
                                                        OrderByDirection.ASC, int.MaxValue, 1);

            var response = await _portalService.GetPendingAccountsAsync(request);

            if (response.GetPendingAccountsResult.Accounts.Count() > 1)
            {
                Trace.TraceWarning(@"Pending accounts matching ""{0}"" is more than 1!", new object[] { requestEmail });
            }

            if (response.GetPendingAccountsResult.Accounts.Any())
            {
                var model = Mapper.Map<PendingUserAccountListItem, UserAccountModel>(
                        response.GetPendingAccountsResult.Accounts.First());

                model.EntityId = entityId;

                return View(model);
            }

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignLicense(long entityId, UserAccountModel account)
        {
            var request = new InsertUserLicenseRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    user_id = account.UserId
                };

            var response = await _portalService.InsertUserLicenseAsync(request);

            if (response.TokenAuth.Status != TokenStatus.Succes)
            {
                TempData["ErrorMessage"] = string.Format("Token error: {0}", response.TokenAuth.Status);
            }
            else
            {
                switch (response.InsertUserLicenseResult.Status)
                {
                    case InsertUserLicenseStatus.Success:
                        TempData["SuccessMessage"] = string.Format("Pro license added to {0}.", account.UserName);
                        break;
                    case InsertUserLicenseStatus.Insufficient_Amount:
                        TempData["ErrorMessage"] = string.Format("Pro license could not be added to {0}. Not enough licenses.",
                                                                 account.UserName);
                        break;
                    case InsertUserLicenseStatus.Access_Denied:
                        TempData["ErrorMessage"] = "Access denied.";
                        break;
                    case InsertUserLicenseStatus.Failed:
                        TempData["ErrorMessage"] = "Failed";
                        break;
                    case InsertUserLicenseStatus.License_Already_Exists:
                        TempData["ErrorMessage"] = string.Format("User {0} already has a license.",
                                                                 account.UserName);
                        break;
                    case InsertUserLicenseStatus.Domain_Denied:
                        TempData["ErrorMessage"] = string.Format("Pro license could not be added to {0}. Domain does not match license owner's domain.",
                                                                 account.UserName);
                        break;
                }
            }

            if (response.InsertUserLicenseResult.Status == InsertUserLicenseStatus.Success &&
                response.TokenAuth.Status == TokenStatus.Succes)
            {
                TempData["SuccessMessage"] = string.Format("Pro license added to {0}.", account.UserName);
            }
            else if (response.InsertUserLicenseResult.Status == InsertUserLicenseStatus.Insufficient_Amount &&
                     response.TokenAuth.Status == TokenStatus.Succes)
            {
                TempData["ErrorMessage"] = string.Format("Pro license could not be added to {0}. Not enough licenses.",
                                                         account.UserName);
            }
            else
            {
                TempData["ErrorMessage"] = string.Format("Could not assign license to {0}.", account.UserName);
            }

            return RedirectToAction("ActiveAccount", new { entityId, userId = account.UserId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLicense(long entityId, UserAccountModel account)
        {
            var request = new RemoveUserLicensesRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    user_id = account.UserId
                };

            var response = await _portalService.RemoveUserLicensesAsync(request);

            if (response.RemoveUserLicensesResult && response.TokenAuth.Status == TokenStatus.Succes)
            {
                TempData["SuccessMessage"] = string.Format("Pro license removed from {0}.", account.UserName);
            }
            else
            {
                TempData["ErrorMessage"] = string.Format("Could not remove Pro license from {0}.", account.UserName);
            }

            return RedirectToAction("ActiveAccount", new { entityId, userId = account.UserId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> SuspendAccount(long entityId, UserAccountModel account)
        {
            var response = await UpdateUserStatusAsync(entityId, account.UserId, UserStatus.Suspended);

            if (response.UpdateUserStatusResult && response.TokenAuth.Status == TokenStatus.Succes)
            {
                TempData["SuccessMessage"] = string.Format("{0} successfully suspended.", account.UserName);
            }
            else
            {
                TempData["ErrorMessage"] = string.Format("Could not suspend {0}.", account.UserName);
            }

            return RedirectToAction("SuspendedAccount", new { entityId, userId = account.UserId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> ActivateAccount(long entityId, UserAccountModel account)
        {
            var response = await UpdateUserStatusAsync(entityId, account.UserId, UserStatus.Active);

            if (response.UpdateUserStatusResult && response.TokenAuth.Status == TokenStatus.Succes)
            {
                TempData["SuccessMessage"] = string.Format("{0} successfully activated.", account.UserName);
            }
            else
            {
                TempData["ErrorMessage"] = string.Format("Could not activate {0}.", account.UserName);
            }

            return RedirectToAction("ActiveAccount", new { entityId, userId = account.UserId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveAccount(long entityId, UserAccountModel account)
        {
            var response = await _portalService.RemoveUserAsync(new RemoveUserRequest(_tokenAuth, entityId, account.UserId));

            if (response.RemoveUserResult && response.TokenAuth.Status == TokenStatus.Succes)
            {
                return RedirectToAction(string.Empty, "CompanyHome", new { entityId, area = "Company" });
            }

            TempData["ErrorMessage"] = "Could not remove user.";

            return RedirectToAction("ActiveAccount", new { entityId, userId = account.UserId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(long entityId, UserAccountModel account)
        {
            var request = new UpdateUserResetPasswordRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    user_id = account.UserId,
                    random_password = true,
                    new_password = string.Empty
                };

            var response = await _portalService.UpdateUserResetPasswordAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.UpdateUserResetPasswordResult)
            {
                TempData["SuccessMessage"] = string.Format("Password reset link successuflly sent to {0}.",
                                                           account.UserName);
            }
            else
            {
                TempData["ErrorMessage"] = "Could not reset password.";
            }

            return RedirectToAction("ActiveAccount", new { entityId, userId = account.UserId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> TransferAccount(long currentEntityId, UserAccountModel account, bool? transferLicenses)
        {
            var request = new UpdateUserCompanyRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = currentEntityId,
                    user_id = account.UserId,
                    new_entity_id = account.EntityId,
                    transfer_licenses = transferLicenses ?? false
                };

            var response = await _portalService.UpdateUserCompanyAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.UpdateUserCompanyResult)
            {
                var accountLink = HtmlHelper.GenerateLink(ControllerContext.RequestContext, RouteTable.Routes,
                                                             account.Name, string.Empty, "ActiveAccount", "Admin",
                                                             new RouteValueDictionary
                                                                 {
                                                                     {"area", "UserAccount"},
                                                                     {"entityId", account.EntityId},
                                                                     {"userId", account.UserId}
                                                                 }, null);
                var companyLink = HtmlHelper.GenerateLink(ControllerContext.RequestContext, RouteTable.Routes,
                                                             account.CompanyName, string.Empty, "Dashboard",
                                                             "CompanyHome",
                                                             new RouteValueDictionary
                                                                 {
                                                                     {"area", "Company"},
                                                                     {"entityId", account.EntityId}
                                                                 }, null);

                TempData["SuccessMessage"] = string.Format("User {0} successfully transfered to {1}.", accountLink,
                                                           companyLink);
            }
            else
            {
                TempData["ErrorMessage"] = "Could not transfer account.";
            }

            return RedirectToAction("Dashboard", "CompanyHome", new { entityId = currentEntityId, area = "Company" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeAccountRole(long entityId, UserAccountModel account)
        {
            var request = new InsertEntityRoleUserRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    user_id = account.UserId,
                    role_id = account.AdminPortalRoleId
                };

            var response = await _portalService.InsertEntityRoleUserAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes &&
                response.InsertEntityRoleUserResult.Status == InsertEntityRoleUserStatus.Success)
            {
                TempData["SuccessMessage"] = string.Format("User {0}'s role was successfully updated.", account.UserName);
            }
            else
            {
                TempData["ErrorMessage"] = string.Format("Could not update role for {0}.", account.UserName);
            }

            return RedirectToAction("ActiveAccount", new { entityId, userId = account.UserId });
        }

        [HttpGet]
        public async Task<ActionResult> Manage(long entityId, long userId)
        {
            ViewBag.CompanyId = entityId;
            ViewData["ErrorMessage"] = TempData["ErrorMessage"];

            var request = new GetAccountDetailsRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    user_id = userId
                };

            var response = await _portalService.GetAccountDetailsAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetAccountDetailsResult != null)
            {
                var model = Mapper.Map<ServiceUserAccount, EditableUserAccountModel>(response.GetAccountDetailsResult);

                return View(model);
            }

            TempData["ErrorMessage"] = "Could not retrieve account details";

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(long entityId, EditableUserAccountModel model)
        {
            ViewBag.CompanyId = entityId;

            if (ModelState.IsValid)
            {
                var request = new UpdateUserContactInfoRequest
                    {
                        TokenAuth = _tokenAuth,
                        first_name = model.FirstName,
                        last_name = model.LastName,
                        user_id = model.UserId,
                        user_contact_info = Mapper.Map<ContactInfoModel, ServiceContactInfo>(model.ContactInfo)
                    };

                var response = await _portalService.UpdateUserContactInfoAsync(request);

                if (response.TokenAuth.Status == TokenStatus.Succes && response.UpdateUserContactInfoResult)
                {
                    return RedirectToAction("ActiveAccount", new { entityId, userId = model.UserId });
                }

                Trace.TraceWarning("Updating account {0} failed.", new object[] { model.UserName });
                Trace.TraceInformation("Token status: {0}.",
                                       new object[] { TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status] });
                Trace.TraceInformation("Response: {0}.", new object[] { response.UpdateUserContactInfoResult });

                ViewData["ErrorMessage"] = "Could not update user account contact details";
            }
            else
            {
                ViewData["ErrorMessage"] = AggregateModelStateErrorsToHtmlString();
            }

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> AjaxDevices(long entityId, long userId, int? page)
        {
            try
            {
                var devices = await GetDevicesAsync(entityId, userId);

                var pageableData = new PageableViewModel<DeviceModel>
                    {
                        PageSize = DeviceListPageSize,
                        CurrentPage = page ?? 1,
                        DataItems = devices
                    };

                var partialViewHtml = ViewRenderer.RenderViewToString("_UserDevicesPartial", ControllerContext, pageableData, true);
                Debug.Print("Returning HTML: {0}", partialViewHtml);

                return Json(new { success = true, Data = partialViewHtml }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format("Error: {0}", e.Message));
                return Json(new
                    {
                        success = false,
                        errors = new[] { MyResources.CouldNotRetrieveDevicesErrorMessage }
                    },
                            JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> AjaxUsage(long entityId, long userId, int? page)
        {
            var model = await RetrieveUsageDetailsAsync(entityId, userId, page);

            Debug.Print("EntityID: {0}", new object[] { entityId });
            Debug.Print("AccountID: {0}", new object[] { userId });
            Debug.Print("Page: {0}", new object[] { page });

            if (model != null)
            {
                var partialViewHtml =
                    ViewRenderer.RenderViewToString("_UsageSummaryPartial", ControllerContext, model, true);
                Debug.Print("Returning HTML: {0}", partialViewHtml);

                return Json(new { success = true, Data = partialViewHtml }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, errors = ModelState.ToList() });
        }

        [HttpPost /*, ValidateAntiForgeryToken*/] // TODO: add token validation
        public async Task<ActionResult> AjaxResendActivation(long entityId, string email)
        {
            var response = await _portalService.ResendActivationAsync(email);

            return response
                       ? Json(new { success = true })
                       : Json(
                           new
                               {
                                   success = false,
                                   errors = new[] { string.Format("Could not resend activation link to {0}", email) }
                               });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePendingAccount(long entityId, long userId)
        {
            var request = new RemovePendingAccountRequest(_tokenAuth, entityId, userId);
            var response = await _portalService.RemovePendingAccountAsync(request);

            if (!response.RemovePendingAccountResult || response.TokenAuth.Status != TokenStatus.Succes)
            {
                TempData["ErrorMessage"] = "Could not remove user.";
            }

            return RedirectToAction("ManageAccounts", "UserAdminHome", new { entityId, area = "UserAdmin" });
        }

        //[HttpGet, AuthorizeAction(true)]
        //public async Task<ActionResult> Emails(long entityid, long userid)
        //{
        //    var getUserEmailsSentRequest = new GetUserEmailsSentRequest(_tokenAuth, entityid, userid);

        //    var response = await _portalService.GetUserEmailsSentAsync(getUserEmailsSentRequest);

        //    if (_tokenAuth.Status == TokenStatus.Succes && response.GetUserEmailsSentResult != null)
        //    {
        //        var emails = Mapper.Map<UserEmail[], IEnumerable<UserEmailModel>>(response.GetUserEmailsSentResult).ToList();

        //        var dataModel = new PageableViewModel<UserEmailModel>
        //            {
        //                DataItems = emails,
        //                PageSize = 10,
        //                CurrentPage = 1
        //            };

        //        return View(dataModel);
        //    }

        //    return View();
        //}

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> UnlockAccount(long entityId, UserAccountModel account)
        {
            var updateUserUnlockRequest = new UpdateUserUnlockRequest(_tokenAuth, entityId, account.UserId);

            var response = await _portalService.UpdateUserUnlockAsync(updateUserUnlockRequest);

            if (response.UpdateUserUnlockResult && response.TokenAuth.Status == TokenStatus.Succes)
            {
                TempData["SuccessMessage"] = string.Format("{0} successfully unlocked.", account.UserName);
            }
            else
            {
                TempData["ErrorMessage"] = string.Format("Could not unlock {0}.", account.UserName);
            }

            return RedirectToAction("ActiveAccount", new { entityId, userId = account.UserId });
        }

        private async Task SetModel(long entityId, long userId, int? usagePage, int? devicePage,
                                    AccountOverviewModel model)
        {
            model.Account = await RetrieveAccountDetailsAsync(entityId, userId);
            model.Usage = await RetrieveUsageDetailsAsync(entityId, userId, usagePage);
            model.Devices = await RetrieveDeviceDetailsAsync(entityId, userId, devicePage);
        }

        private async Task<UserAccountModel> RetrieveAccountDetailsAsync(long entityId, long accountId)
        {
            var request = new GetAccountDetailsRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    user_id = accountId
                };

            GetAccountDetailsResponse response = await _portalService.GetAccountDetailsAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetAccountDetailsResult != null)
            {
                return Mapper.Map<ServiceUserAccount, UserAccountModel>(response.GetAccountDetailsResult);
            }

            ViewBag.ErrorMessage = "Could not retrieve account details.";

            return null;
        }

        private async Task<PageableViewModel<UsageSummaryModel>> RetrieveUsageDetailsAsync(long entityId, long accountId,
                                                                                           int? page)
        {
            var getDeviceListRequest = new GetUserUsageSummaryByMonthRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    user_id = accountId
                };

            var response = await _portalService.GetUserUsageSummaryByMonthAsync(getDeviceListRequest);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetUserUsageSummaryByMonthResult != null)
            {
                var devices = Mapper.Map<UsageSummary[], IEnumerable<UsageSummaryModel>>(response.GetUserUsageSummaryByMonthResult)
                          .ToList().OrderByDescending(us => us.Year).ThenByDescending(us => us.Month);

                var pageableData = new PageableViewModel<UsageSummaryModel>
                    {
                        PageSize = UsageTablePageSize,
                        CurrentPage = page ?? 1,
                        DataItems = devices,
                        TableBodyPartialView = "_UsageSummaryListPartial"
                    };

                return pageableData;
            }

            ViewBag.ErrorMessage = "Could not retrieve usage details.";

            return null;
        }

        private async Task<PageableViewModel<DeviceModel>> RetrieveDeviceDetailsAsync(long entityId, long accountId,
                                                                                      int? page)
        {
            var getDeviceListRequest = new GetDeviceListRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    user_id = accountId
                };

            var response = await _portalService.GetDeviceListAsync(getDeviceListRequest);

            IEnumerable<DeviceModel> devices = Mapper.Map<Device[], IEnumerable<DeviceModel>>(
                response.GetDeviceListResult)
                                                     .OrderByDescending(x => x.HasActiveSession)
                                                     .ThenByDescending(x => x.DateDeployed);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetDeviceListResult != null)
            {
                return new PageableViewModel<DeviceModel>
                    {
                        PageSize = DeviceListPageSize,
                        CurrentPage = page ?? 1,
                        DataItems = devices,
                    };
            }

            ViewBag.ErrorMessage = "Could not retrieve device details.";

            return null;
        }

        private async Task<UpdateUserStatusResponse> UpdateUserStatusAsync(long id, long userId, UserStatus newStatus)
        {
            var request = new UpdateUserStatusRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = id,
                    user_id = userId,
                    new_status = newStatus
                };

            var response = await _portalService.UpdateUserStatusAsync(request);

            return response;
        }

        private async Task<IEnumerable<DeviceModel>> GetDevicesAsync(long entityId, long userId)
        {
            var getDeviceListRequest = new GetDeviceListRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    user_id = userId
                };

            var response = await _portalService.GetDeviceListAsync(getDeviceListRequest);
            IEnumerable<DeviceModel> devices = Mapper.Map<Device[], IEnumerable<DeviceModel>>(
                response.GetDeviceListResult)
                                                     .OrderByDescending(x => x.HasActiveSession)
                                                     .ThenByDescending(x => x.DateDeployed);

            return devices;
        }

        private async Task<long?> FindAccountIdByEmailAsync(long entityId, string email)
        {
            var accountsRequest = new GetAccountsListRequest(_tokenAuth, entityId, email, AccountSearchType.EMAIL,
                                                         AccountItemListSort.ID, OrderByDirection.ASC, 1, 1, UserStatus.Active);

            var response = await _portalService.GetAccountsListAsync(accountsRequest);

            if (response.TokenAuth.Status != TokenStatus.Succes || response.GetAccountsListResult.Accounts == null ||
                response.GetAccountsListResult.Accounts.Length == 0 || response.GetAccountsListResult.TotalRecords == 0)
            {
                return null;
            }

            return response.GetAccountsListResult.Accounts.First().Id;
        }

        private async Task<IEnumerable<SelectListItem>> BuildRoleDropDownListAsync(long entityId, long? selectedId)
        {
            IEnumerable<SelectListItem> roleDropDownList = new EntityCollection<SelectListItem>
                {
                    new SelectListItem
                        {
                            Selected = 0 == selectedId,
                            Value = "0",
                            Text = MyResources.RoleDropDown_User
                        }
                };

            var request = new GetGrantableRolesRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId
                };

            var response = await _portalService.GetGrantableRolesAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetGrantableRolesResult.Any())
            {
                roleDropDownList =
                    roleDropDownList.Concat(response.GetGrantableRolesResult.Select(x => new SelectListItem
                        {
                            Selected = x.Id == selectedId,
                            Value = x.Id.ToString(CultureInfo.InvariantCulture),
                            Text = x.Title
                        }));
            }

            return roleDropDownList;
        }
    }
}