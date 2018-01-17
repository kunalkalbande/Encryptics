using AutoMapper;
using Encryptics.WebPortal.ActionResults;
using Encryptics.WebPortal.Areas.Company.Models;
using Encryptics.WebPortal.Areas.UserAccount.Models;
using Encryptics.WebPortal.Areas.UserAdmin.Models;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.Filters;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using AllResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Areas.UserAdmin.Controllers
{
    public class UserAdminHomeController : PortalServiceAwareController
    {
        private readonly string[] _manageAccountPermissions = new[]
            {
                "UserAdmin/UserAdminHome/AjaxSearchActiveAccounts",
                "UserAdmin/UserAdminHome/AjaxSearchSuspendedAccounts",
                "UserAdmin/UserAdminHome/AjaxSearchPendingAccounts",
                //"UserAdmin/UserAdminHome/AjaxRemoveActiveAccounts",
                "UserAdmin/UserAdminHome/AjaxDisableActiveAccounts",
                "UserAdmin/UserAdminHome/AjaxRemovePendingAccounts",
                "UserAdmin/UserAdminHome/AjaxReinstateUsers",
                "UserAdmin/UserAdminHome/AjaxAssignProLicenses",
                "UserAdmin/UserAdminHome/AjaxResendActivationLinks"
            };

        public UserAdminHomeController(PortalServiceSoap portalService)
            : base(portalService)
        {
        }

        [HttpGet]
        public async Task<ActionResult> ManageAccounts(long entityId)
        {
            var model = await BuildCompanyAccountsModel(entityId);

            ConvertModelStateToErrorMessages();

            await InitializeViewBagAsync(entityId);

            await SetViewPermissionsAsync(entityId, _manageAccountPermissions);

            return View(model);
        }

        [HttpGet]
        public JsonpResult AjaxSearchActiveAccounts()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxSearchActiveAccounts(long entityId,
                                                                 [Bind(Prefix = "ActiveAccountSearchParameters")] ActiveAccountsSearchModel
                                                                     activeAccountSearchParameters)
        {
            if (TempData["ErrorMessage"] != null)
            {
                return Json(new { errors = new[] { TempData["ErrorMessage"].ToString() } });
            }

            var accountsResponse = await GetActiveAccountsAsync(entityId, activeAccountSearchParameters);

            ViewData.TemplateInfo.HtmlFieldPrefix = "ActiveAccountSearchParameters";

            if (accountsResponse.TokenAuth.Status == TokenStatus.Succes)
            {
                FillAccountsResult(activeAccountSearchParameters, accountsResponse);
            }
            else
            {
                return Json(new { errors = new[] { AllResources.ErrorMessageCouldNotRetrieveActiveAccounts } });
            }

            ViewBag.EntityId = entityId;

            await SetViewPermissionsAsync(entityId, _manageAccountPermissions);

            return PartialView("_ActiveAccounts", activeAccountSearchParameters);
        }

        [HttpGet/*, AuthorizeAction(true)*/]
        public JsonpResult AjaxSearchSuspendedAccounts()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, ValidateHttpAntiForgeryToken/*, AuthorizeAction(true)*/]
        public async Task<ActionResult> AjaxSearchSuspendedAccounts(long entityId,
                                                                    [Bind(Prefix = "SuspendedAccountSearchParameters")] ActiveAccountsSearchModel
                                                                        suspendedAccountSearchParameters)
        {
            if (TempData["ErrorMessage"] != null)
            {
                return Json(new { errors = new[] { TempData["ErrorMessage"].ToString() } });
            }

            var accountsResponse = await GetSuspendedAccountsAsync(entityId, suspendedAccountSearchParameters);

            ViewData.TemplateInfo.HtmlFieldPrefix = "SuspendedAccountSearchParameters";

            if (accountsResponse.TokenAuth.Status == TokenStatus.Succes)
            {
                FillAccountsResult(suspendedAccountSearchParameters, accountsResponse);
            }
            else
            {
                return Json(new { errors = new[] { AllResources.ErrorMessageCouldNotRetrieveActiveAccounts } });
            }

            ViewBag.EntityId = entityId;

            await SetViewPermissionsAsync(entityId, _manageAccountPermissions);

            return PartialView("_SuspendedAccounts", suspendedAccountSearchParameters);
        }

        [HttpGet]
        public JsonpResult AjaxSearchPendingAccounts()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxSearchPendingAccounts(long entityId,
                                                                  [Bind(Prefix = "PendingAccountSearchParameters")] PendingAccountsSearchModel
                                                                      pendingAccountsSearchParameters)
        {
            if (TempData["ErrorMessage"] != null)
            {
                return Json(new { errors = new[] { TempData["ErrorMessage"].ToString() } });
            }
            var accountsResponse = await GetPendingAccountsAsync(entityId, pendingAccountsSearchParameters);

            ViewData.TemplateInfo.HtmlFieldPrefix = "PendingAccountSearchParameters";

            if (accountsResponse.TokenAuth.Status == TokenStatus.Succes)
            {
                FillPendingAccounts(pendingAccountsSearchParameters, accountsResponse, entityId);
            }
            else
            {
                return Json(new { errors = new[] { AllResources.ErrorMessageCouldNotRetrieveActiveAccounts } });
            }

            ViewBag.EntityId = entityId;

            await SetViewPermissionsAsync(entityId, _manageAccountPermissions);

            return PartialView("_PendingAccounts", pendingAccountsSearchParameters);
        }

        [HttpGet/*, AuthorizeAction(true)*/]
        public JsonpResult AjaxDisableActiveAccounts()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, ValidateHttpAntiForgeryToken/*, AuthorizeAction(true)*/]
        public async Task<ActionResult> AjaxDisableActiveAccounts(long entityId, IEnumerable<long> userIds)
        {
            var allDisableSuccessful = true;
            var allLicenseRemoveSuccessful = true;
            var userStatusRequest = new UpdateUserStatusRequest
            {
                TokenAuth = _tokenAuth,
                entity_id = entityId,
                new_status = UserStatus.Suspended
            };
            var licensesRequest = new RemoveUserLicensesRequest
                {

                    TokenAuth = _tokenAuth,
                    entity_id = entityId
                };

            foreach (var userId in userIds)
            {
                userStatusRequest.TokenAuth.Nonce = this.GetAntiForgeryToken();
                userStatusRequest.user_id = userId;

                var updateUserStatusResponse = await _portalService.UpdateUserStatusAsync(userStatusRequest);

                if (updateUserStatusResponse.TokenAuth.Status != TokenStatus.Succes ||
                    !updateUserStatusResponse.UpdateUserStatusResult)
                    allDisableSuccessful = false;
                else
                {
                    licensesRequest.TokenAuth.Nonce = this.GetAntiForgeryToken();
                    licensesRequest.user_id = userId;
                    var licensesResponse = await _portalService.RemoveUserLicensesAsync(licensesRequest);
                    if (licensesResponse.TokenAuth.Status != TokenStatus.Succes ||
                        !licensesResponse.RemoveUserLicensesResult)
                        allLicenseRemoveSuccessful = false;
                }
            }

            if (!allDisableSuccessful || !allLicenseRemoveSuccessful)
            {
                var errors = new List<string>();

                if (!allDisableSuccessful) errors.Add("Could not disable all user(s) selected.");
                if (!allLicenseRemoveSuccessful) errors.Add("Could not remove all licenses for user(s) selected.");

                return Json(new { success = false, errors = errors.ToArray() });
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public JsonpResult AjaxRemoveActiveAccounts()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxRemoveActiveAccounts(long entityId, List<long> userIds)
        {
            var allSuccessful = true;
            var request = new RemoveUserRequest { TokenAuth = _tokenAuth, entity_id = entityId };

            foreach (var userId in userIds)
            {
                request.user_id = userId;
                request.TokenAuth.Nonce = AntiForgeryTokenHelper.GetAntiForgeryToken();

                var response = await _portalService.RemoveUserAsync(request);

                if (response.TokenAuth.Status != TokenStatus.Succes || !response.RemoveUserResult)
                {
                    allSuccessful = false;
                }
            }

            return !allSuccessful ? Json(new { success = false, errors = new[] { "Could not remove all user(s) selected." } }) : Json(new { success = true });
        }

        [HttpGet]
        public JsonpResult AjaxAssignProLicenses()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxAssignProLicenses(long entityId, List<long> userIds)
        {
            var request = new InsertUserLicenseRequest(_tokenAuth, entityId, 0);
            var errors = new List<string>();

            foreach (var userId in userIds)
            {
                request.user_id = userId;
                request.TokenAuth.Nonce = AntiForgeryTokenHelper.GetAntiForgeryToken();

                var response = await _portalService.InsertUserLicenseAsync(request);

                if (response.InsertUserLicenseResult.Status == InsertUserLicenseStatus.Success) continue;

                var userName = string.Empty;
                var userResponse = await
                                   _portalService.GetAccountDetailsAsync(new GetAccountDetailsRequest(_tokenAuth, entityId, userId));

                if (userResponse.TokenAuth.Status == TokenStatus.Succes)
                {
                    userName = userResponse.GetAccountDetailsResult.UserName;
                }

                switch (response.InsertUserLicenseResult.Status)
                {
                    case InsertUserLicenseStatus.Insufficient_Amount:
                        var companyName = string.Empty;
                        var companyResponse = await
                                              _portalService.GetCompanyDetailsAsync(new GetCompanyDetailsRequest(_tokenAuth, entityId));
                        if (companyResponse.TokenAuth.Status == TokenStatus.Succes)
                        {
                            companyName = companyResponse.GetCompanyDetailsResult.Name;
                        }

                        errors.Add(
                            string.Format("User {0} could not be assigned a license. Company {1} does not have enough licenses.<br/>",
                                          userName, companyName));

                        break;

                    case InsertUserLicenseStatus.License_Already_Exists:
                        errors.Add(string.Format(
                            "User {0} could not be assigned a license. A PRO license has already been issued.<br/>",
                            userName));

                        break;

                    case InsertUserLicenseStatus.Domain_Denied:
                        errors.Add(string.Format("Pro license could not be added to {0}. Domain does not match license owner's domain.",
                                                                 userName));
                        break;

                    default:
                        errors.Add(string.Format("User {0} could not be assigned license. Error Code: {1}", userName,
                                                        (int)response.InsertUserLicenseResult.Status));

                        break;
                }
            }

            if (errors.Count > 0)
            {
                Json(
                    new { success = false, errors });
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<ActionResult> AjaxLicenseInfoUpdate(long entityId)
        {
            var request = new GetCompanyDetailsRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId
                };

            var response = await _portalService.GetCompanyDetailsAsync(request);

            if (response.TokenAuth.Status != TokenStatus.Succes || response.GetCompanyDetailsResult == null)
            {
                return Json(new { errors = new[] { "Could not retrieve updated company license details." } },
                            JsonRequestBehavior.AllowGet);
            }

            var licenseInfo = Mapper.Map<LicensingInfo, LicensingModel>(response.GetCompanyDetailsResult.LicensingInfo);

            return Json(new
                {
                    active = licenseInfo.ActiveLicenses,
                    used = licenseInfo.UsedLicenses,
                    available = licenseInfo.AvailableLicenses
                }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonpResult AjaxResendActivationLinks()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxResendActivationLinks(List<string> emails)
        {
            var allSuccessful = true;

            foreach (var email in emails)
            {
                var response = await _portalService.ResendActivationAsync(email);

                if (!response)
                {
                    allSuccessful = false;

                }
            }

            if (!allSuccessful)
            {
                return Json(new
                    {
                        success = false,
                        errors = new[] { "Could not resend activation email to all user(s) selected." }
                    });
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public JsonpResult AjaxRemovePendingAccounts()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxRemovePendingAccounts(long entityId, List<long> userIds)
        {
            var allSuccessful = true;

            foreach (var userId in userIds)
            {
                _tokenAuth.Nonce = AntiForgeryTokenHelper.GetAntiForgeryToken();
                var response = await _portalService
                    .RemovePendingAccountAsync(new RemovePendingAccountRequest(_tokenAuth, entityId, userId));

                if (!response.RemovePendingAccountResult || response.TokenAuth.Status != TokenStatus.Succes)
                {
                    allSuccessful = false;
                }
            }

            if (!allSuccessful)
            {
                return Json(new
                {
                    success = false,
                    errors = new[] { "Could not remove all user(s) selected." }
                });
                //TempData["ErrorMessage"] = "Could not remove all user(s) selected.";
            }

            return Json(new { success = true });
            //return RedirectToSearchPendingAccounts(entityId, pendingAccountsSearchParameters);
        }

        [HttpGet]
        public async Task<ActionResult> AddNewUser(long entityId)
        {
            var companyDetails = await GetCompanyDetailsAsync(entityId);
            var editableUserAccountModel = new InsertableUserAccountModel
                {
                    EntityId = entityId,
                    CompanyName = companyDetails.Name
                };

            return View(editableUserAccountModel);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> AddNewUser(long entityId, InsertableUserAccountModel newUserAccount,
            int? month, int? year)
        {
            if (ModelState.IsValid)
            {
                var response = await _portalService.InsertUserAsync(new InsertUserRequest
                    {
                        TokenAuth = _tokenAuth,
                        first_name = newUserAccount.FirstName,
                        last_name = newUserAccount.LastName,
                        assign_license = newUserAccount.AssignLicense,
                        software_installed_by_admin = newUserAccount.DownloadSoftware,
                        entity_id = entityId,
                        email_address = newUserAccount.UserName
                    });

                if (!(response.TokenAuth.Status == TokenStatus.Succes &&
                      response.InsertUserResult.Status == InsertUserStatus.Success))
                {
                    Trace.TraceError(string.Format("InsertUserAsync failed: {0}", response.InsertUserResult.Status));

                    Trace.TraceWarning(string.Format("Token status: {0}", response.TokenAuth.Status));
                    Trace.WriteIf(string.IsNullOrEmpty(response.TokenAuth.Token), "Token is unknown.");
                    Trace.TraceWarning(string.Format("Adding account {0} failed: {1}.", newUserAccount.UserName,
                                                     response.InsertUserResult.Status));

                    if (response.InsertUserResult.Status == InsertUserStatus.User_Already_Exists)
                    {
                        ViewData["ErrorMessage"] = "User already exists.";
                    }
                    else
                    {
                        Trace.TraceWarning("Addding new user {0} failed.", new object[] { newUserAccount.UserName });
                        Trace.TraceInformation("Token status: {0}.",
                                               new object[] { TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status] });
                        Trace.TraceInformation("Response: {0}.",
                                               new object[]
                                                   {
                                                       Enum.GetName(typeof (InsertUserStatus),
                                                                    response.InsertUserResult.Status)
                                                   });
                        ViewData["ErrorMessage"] = "New user could not added due to an internal server error.";
                    }
                }
                else
                {
                    var newUserLink = HtmlHelper.GenerateLink(ControllerContext.RequestContext, RouteTable.Routes,
                                                              newUserAccount.UserName, string.Empty,
                                                              "PendingAccount", "Admin", new RouteValueDictionary
                                                                  {
                                                                      {"area", "UserAccount"},
                                                                      {"entityId", newUserAccount.EntityId},
                                                                      {"userId", newUserAccount.UserId}
                                                                  }, null);

                    TempData["SuccessMessage"] = string.Format("New user {0} successfuly added.", newUserLink);

                    return RedirectToAction(string.Empty, "CompanyHome", new { area = "Company", entityId });
                }
            }
            else
            {
                ConvertModelStateToErrorMessages();
            }

            return View(newUserAccount);
        }

        [HttpGet/*, AuthorizeAction(true)*/]
        public JsonpResult AjaxReinstateUsers()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, ValidateHttpAntiForgeryToken/*, AuthorizeAction(true)*/]
        public async Task<ActionResult> AjaxReinstateUsers(long entityId, IEnumerable<long> userIds)
        {
            if (!await UpdateUserStatus(entityId, userIds, UserStatus.Active))
            {
                return Json(new { success = false, errors = new[] { "Could not reinstate user(s)." } });
            }

            return Json(new { success = true });
        }

        private async Task<bool> UpdateUserStatus(long entityId, IEnumerable<long> userIds, UserStatus newState)
        {
            var allSuccessful = true;
            var request = new UpdateUserStatusRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    new_status = newState
                };

            foreach (var id in userIds)
            {
                request.user_id = id;
                request.TokenAuth.Nonce = AntiForgeryTokenHelper.GetAntiForgeryToken();
                var response = await _portalService.UpdateUserStatusAsync(request);
                if (response.TokenAuth.Status != TokenStatus.Succes || !response.UpdateUserStatusResult)
                {
                    allSuccessful = false;
                }
            }

            return allSuccessful;
        }

        private async Task<ManageCompanyAccountsModel> BuildCompanyAccountsModel(long entityId)
        {
            var companyDetails = await GetCompanyDetailsAsync(entityId);
            var activeAccountsSearchModel = new ActiveAccountsSearchModel
                {
                    ItemCount = companyDetails.ActiveUserAccountTotal
                };
            var pendingAccountsSearchModel = new PendingAccountsSearchModel
                {
                    ItemCount = companyDetails.PendingUserAccountTotal
                };
            var suspendedAccountsSearchModel = new ActiveAccountsSearchModel
                {
                    ItemCount = companyDetails.SuspendedUserAccountTotal
                };
            var model = new ManageCompanyAccountsModel
                {
                    EntityId = entityId,
                    CompanyName = companyDetails.Name,
                    ActiveAccounts = activeAccountsSearchModel,
                    PendingAccounts = pendingAccountsSearchModel,
                    SuspendedAccounts = suspendedAccountsSearchModel
                };

            await BuildActiveAccounts(entityId, model);
            await BuildSuspendedAccounts(entityId, model);
            await BuildPendingAccounts(entityId, model);

            return model;
        }

        private async Task BuildPendingAccounts(long entityId, ManageCompanyAccountsModel model)
        {
            var getPendingAccountsResponse = await GetPendingAccountsAsync(entityId, model.PendingAccounts);

            if (getPendingAccountsResponse.TokenAuth.Status == TokenStatus.Succes)
            {
                FillPendingAccounts(model.PendingAccounts, getPendingAccountsResponse, entityId);
            }
            else
            {
                Trace.TraceError("Token error: {0}",
                                 (object)
                                 TokenStatus.ErrorMessageDictionary[getPendingAccountsResponse.TokenAuth.Status]);

                ModelState.AddModelError(string.Empty, AllResources.ErrorMessageCouldNotRetreivePendingAccounts);
            }
        }

        private async Task BuildActiveAccounts(long id, ManageCompanyAccountsModel model)
        {
            var getAccountsResponse = await GetActiveAccountsAsync(id, model.ActiveAccounts);

            if (getAccountsResponse.TokenAuth.Status == TokenStatus.Succes)
            {
                FillAccountsResult(model.ActiveAccounts, getAccountsResponse);
            }
            else
            {
                Trace.TraceError("Token error: {0}",
                                 (object)TokenStatus.ErrorMessageDictionary[getAccountsResponse.TokenAuth.Status]);

                ModelState.AddModelError(string.Empty, AllResources.ErrorMessageCouldNotRetrieveActiveAccounts);
            }
        }

        private async Task BuildSuspendedAccounts(long id, ManageCompanyAccountsModel model)
        {
            var getAccountsResponse = await GetSuspendedAccountsAsync(id, model.SuspendedAccounts);

            if (getAccountsResponse.TokenAuth.Status == TokenStatus.Succes)
            {
                FillAccountsResult(model.SuspendedAccounts, getAccountsResponse);
            }
            else
            {
                Trace.TraceError("Token error: {0}",
                                 (object)TokenStatus.ErrorMessageDictionary[getAccountsResponse.TokenAuth.Status]);

                ModelState.AddModelError(string.Empty, AllResources.ErrorMessageCouldNotRetrieveActiveAccounts);
            }
        }

        private async Task<CompanySummaryModel> GetCompanyDetailsAsync(long entityId)
        {
            var response =
                await _portalService.GetCompanyDetailsAsync(new GetCompanyDetailsRequest
                    {
                        entity_id = entityId,
                        TokenAuth = _tokenAuth
                    });

            if (response.TokenAuth.Status == TokenStatus.Succes)
            {
                return Mapper.Map<PortalService.Company, CompanySummaryModel>(response.GetCompanyDetailsResult);
            }

            Trace.TraceError("Token error: {0}",
                             (object)TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]);

            ModelState.AddModelError(string.Empty, AllResources.ErrorMessageCouldNotRetrieveEntityDetails);

            return null;
        }

        private async Task<GetAccountsListResponse> GetActiveAccountsAsync(long? entityId,
                                                                           UserSearchParametersModel parametersModel)
        {
            var accountsRequest = BuildAccountsRequest(entityId, parametersModel, UserStatus.Active);

            return await _portalService.GetAccountsListAsync(accountsRequest);
        }

        private async Task<GetAccountsListResponse> GetSuspendedAccountsAsync(long? entityId,
                                                                              UserSearchParametersModel parametersModel)
        {
            var accountsRequest = BuildAccountsRequest(entityId, parametersModel, UserStatus.Suspended);

            return await _portalService.GetAccountsListAsync(accountsRequest);
        }

        private GetAccountsListRequest BuildAccountsRequest(long? entityId, UserSearchParametersModel searchParameters,
                                                            UserStatus userStatus)
        {
            return new GetAccountsListRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    term = searchParameters.SearchTerm,
                    user_status = userStatus,
                    order_dir = (OrderByDirection)Enum.Parse(typeof(OrderByDirection), searchParameters.SortOrder),
                    page = searchParameters.CurrentPage,
                    page_size = searchParameters.PageSize,
                    search_type = (AccountSearchType)searchParameters.SearchType,
                    sort_field =
                        (AccountItemListSort)Enum.Parse(typeof(AccountItemListSort), searchParameters.SortField)
                };
        }

        private static void FillAccountsResult(ActiveAccountsSearchModel activeAccountSearchParameters,
                                               GetAccountsListResponse accountsResponse)
        {
            activeAccountSearchParameters.ActiveAccounts =
                Mapper.Map<UserAccountListItem[], IEnumerable<ActiveAccountsListItemModel>>(
                    accountsResponse.GetAccountsListResult.Accounts);
            activeAccountSearchParameters.ItemCount = accountsResponse.GetAccountsListResult.TotalRecords;
        }

        private async Task<GetPendingAccountsResponse> GetPendingAccountsAsync(long entityId,
                                                                               UserSearchParametersModel
                                                                                   pendingAccountsSearchModel)
        {
            var accountsRequest = BuildPendingAccountsRequest(entityId, pendingAccountsSearchModel);

            return await _portalService.GetPendingAccountsAsync(accountsRequest);
        }

        private GetPendingAccountsRequest BuildPendingAccountsRequest(long entityId,
                                                                      UserSearchParametersModel searchParameters)
        {
            return new GetPendingAccountsRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    term = searchParameters.SearchTerm,
                    order_dir = (OrderByDirection)Enum.Parse(typeof(OrderByDirection), searchParameters.SortOrder),
                    page = searchParameters.CurrentPage,
                    page_size = searchParameters.PageSize,
                    search_type = (PendingAccountSearchType)searchParameters.SearchType,
                    sort_field =
                        (PendingAccountItemListSort)
                        Enum.Parse(typeof(PendingAccountItemListSort), searchParameters.SortField)
                };
        }

        private static void FillPendingAccounts(PendingAccountsSearchModel pendingAccountsSearchParameters,
                                                GetPendingAccountsResponse accountsResponse, long entityId)
        {
            pendingAccountsSearchParameters.PendingAccounts =
                Mapper.Map<PendingUserAccountListItem[], IEnumerable<PendingAccountsListItemModel>>(
                    accountsResponse.GetPendingAccountsResult.Accounts);

            pendingAccountsSearchParameters.PendingAccounts.Each(a => a.EntityId = entityId);

            pendingAccountsSearchParameters.ItemCount = accountsResponse.GetPendingAccountsResult.TotalRecords;
        }
    }
}
