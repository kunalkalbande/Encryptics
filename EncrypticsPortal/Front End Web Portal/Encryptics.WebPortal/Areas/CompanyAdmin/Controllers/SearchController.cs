using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using Encryptics.WebPortal.ActionResults;
using Encryptics.WebPortal.Areas.CompanyAdmin.Models;
using Encryptics.WebPortal.Areas.UserAdmin.Models;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.Filters;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.IdentityModel;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using Newtonsoft.Json;
using AllResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Areas.CompanyAdmin.Controllers
{
    public class SearchController : PortalServiceAwareController
    {
        private readonly string[] _searchPermissions = new[]
            {
                "CompanyAdmin/Search/AjaxGetAllSearchResults",
                "CompanyAdmin/Search/AjaxSearchCompanies",
                "CompanyAdmin/Search/AjaxSearchActiveAccounts",
                "CompanyAdmin/Search/AjaxSearchSuspendedAccounts",
                "CompanyAdmin/Search/AjaxSearchPendingAccounts",
                //"CompanyAdmin/Search/AjaxRemoveActiveAccounts",`
                "CompanyAdmin/Search/AjaxDisableActiveAccounts",
                "CompanyAdmin/Search/AjaxEnableSuspendedAccounts",
                "CompanyAdmin/Search/AjaxAssignProLicenses",
                "CompanyAdmin/Search/AjaxRemovePendingAccounts",
                "UserAdmin/UserAdminHome/AjaxResendActivationLinks"
            };

        public SearchController(PortalServiceSoap portalService)
            : base(portalService)
        {
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> SearchResults(string siteSearchTerm)
        {
            var encrypticsUser = EncrypticsPrincipal.CurrentEncrypticsUser;

            if (string.IsNullOrEmpty(siteSearchTerm))
                return View("SearchResults");

            ModelState.Clear();

            var viewModel = InitializeSearchResultsModel(siteSearchTerm);

            await FillActiveAccounts(viewModel);
            await FillSuspendedAccounts(viewModel);
            await FillPendingAccounts(viewModel);
            await FillCompanies(viewModel, encrypticsUser.EntityId, encrypticsUser.UserId, siteSearchTerm);

            await SetViewPermissionsAsync(encrypticsUser.EntityId, _searchPermissions);

            return View("SearchResults", viewModel);
        }

        [HttpGet]
        public JsonpResult AjaxGetAllSearchResults()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxGetAllSearchResults(string searchTerm)
        {
            var encrypticsUser = EncrypticsPrincipal.CurrentEncrypticsUser;
            var searchResults = new List<object>();
            var companies = await RetrieveCompanies(encrypticsUser.EntityId,
                encrypticsUser.UserId, searchTerm);
            var filteredCompanies = from c in companies
                                    select new
                                        {
                                            label = c.Name,
                                            value = Url.Action("Dashboard", "CompanyHome",
                                                               new { area = "Company", entityId = c.Id }),
                                            category = "Company"
                                        };
            var searchUsers = InitializeDefaultSearchModel(searchTerm);

            await SearchActiveAccounts(searchUsers);
            await SearchSuspendedAccounts(searchUsers);
            await SearchPendingAccounts(searchUsers);

            var activeAccounts = searchUsers.ActiveAccounts.DataItems.Select(a => new
                {
                    label = string.IsNullOrEmpty(a.FullName.Trim()) ? a.UserName : string.Format("{0} ({1})", a.FullName, a.UserName),
                    value = Url.Action("ActiveAccount", "Admin", new
                        {
                            area = "UserAccount",
                            entityId = a.EntityId,
                            userId = a.Id
                        }),
                    category = "Active Users"
                });

            var suspendedAccounts = searchUsers.SuspendedAccounts.DataItems.Select(a => new
            {
                label = string.IsNullOrEmpty(a.FullName.Trim()) ? a.UserName : string.Format("{0} ({1})", a.FullName, a.UserName),
                value = Url.Action("SuspendedAccount", "Admin", new
                {
                    area = "UserAccount",
                    entityId = a.EntityId,
                    userId = a.Id
                }),
                category = "Suspended Users"
            });

            var pendingAccounts = searchUsers.PendingAccounts.DataItems.Select(p => new
                {
                    label = !string.IsNullOrEmpty(p.FullName.Trim()) ? p.FullName.Trim() : p.UserName,
                    value = Url.Action("PendingAccount", "Admin", new
                        {
                            area = "UserAccount",
                            entityId = p.EntityId,
                            requestEmail = p.UserName
                        }),
                    category = "Pending Users"
                });

            var jsonpResult = new JsonpResult
                {
                    Data = searchResults
                        .Concat(filteredCompanies)
                        .Concat(activeAccounts)
                        .Concat(suspendedAccounts)
                        .Concat(pendingAccounts),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

            Debug.Print("Result: {0}", JsonConvert.SerializeObject(jsonpResult));

            return jsonpResult;
        }

        //[HttpGet]
        //public JsonpResult AjaxSearchCompanies()
        //{
        //    return GetJsonpAntiforgeryToken();
        //}

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> AjaxSearchCompanies(string searchTerm, int? currentPage, int? pageSize)
        {
            var encrypticsUser = EncrypticsPrincipal.CurrentEncrypticsUser;
            var companyListItemModels = await RetrieveCompanies(encrypticsUser.EntityId, encrypticsUser.UserId, searchTerm);
            var listItemModels = companyListItemModels as IList<CompanyListItemModel> ?? companyListItemModels.ToList();
            var model = new CompanySearchModel
                {
                    ItemCount = listItemModels.LongCount(),
                    DataItems = listItemModels,
                    CurrentPage = currentPage ?? 1,
                    PageSize = pageSize ?? 15,
                    TableBodyPartialView = "_CompanyListPartial",
                    SearchTerm = searchTerm
                };

            return PartialView("_CompaniesPartial", model);
        }

        [HttpGet]
        public ActionResult AjaxSearchActiveAccounts()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, /*ValidateAntiForgeryToken*/ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxSearchActiveAccounts(ActiveAccountsSearchModel activeAccountSearchParameters)
        {
            if (TempData["ErrorMessage"] != null)
            {
                return new JsonResult
                    {
                        Data = new { errors = new[] { TempData["ErrorMessage"].ToString() } },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
            }

            var accountsResponse = await RetrieveActiveAccounts(activeAccountSearchParameters);

            if (accountsResponse.TokenAuth.Status == TokenStatus.Succes)
            {
                FillActiveAccountsResult(activeAccountSearchParameters, accountsResponse);
            }
            else
            {
                return new JsonResult
                    {
                        Data = new { errors = new[] { "Could not retrieve active accounts." } },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
            }

            ModelState.Clear();

            await SetViewPermissionsAsync(EncrypticsPrincipal.CurrentEncrypticsUser.EntityId, _searchPermissions);

            return PartialView("_ActiveAccountsPartial", activeAccountSearchParameters);
        }

        [HttpGet]
        public ActionResult AjaxSearchSuspendedAccounts()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, /*ValidateAntiForgeryToken*/ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxSearchSuspendedAccounts(ActiveAccountsSearchModel suspendedAccountSearchParameters)
        {
            if (TempData["ErrorMessage"] != null)
            {
                return new JsonResult
                {
                    Data = new { errors = new[] { TempData["ErrorMessage"].ToString() } },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            var accountsResponse = await RetrieveSuspendedAccounts(suspendedAccountSearchParameters);

            if (accountsResponse.TokenAuth.Status == TokenStatus.Succes)
            {
                FillActiveAccountsResult(suspendedAccountSearchParameters, accountsResponse);
            }
            else
            {
                return new JsonResult
                {
                    Data = new { errors = new[] { "Could not retrieve active accounts." } },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            ModelState.Clear();

            await SetViewPermissionsAsync(EncrypticsPrincipal.CurrentEncrypticsUser.EntityId, _searchPermissions);

            return PartialView("_SuspendedAccountsPartial", suspendedAccountSearchParameters);
        }

        [HttpGet]
        public ActionResult AjaxSearchPendingAccounts()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, /*ValidateAntiForgeryToken*/ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxSearchPendingAccounts(
            PendingAccountsSearchModel pendingAccountSearchParameters)
        {
            if (TempData["ErrorMessage"] != null)
            {
                return Json(new { errors = new[] { TempData["ErrorMessage"].ToString() } });
            }

            var accountsResponse = await RetrievePendingAccounts(pendingAccountSearchParameters);

            if (accountsResponse.TokenAuth.Status == TokenStatus.Succes)
            {
                FillPendingAccountsResult(pendingAccountSearchParameters, accountsResponse);
            }
            else
            {
                return Json(new { errors = new[] { "Could not retrieve pending accounts." } });
            }

            ModelState.Clear();

            await SetViewPermissionsAsync(EncrypticsPrincipal.CurrentEncrypticsUser.EntityId, _searchPermissions);

            return PartialView("_PendingAccountsPartial", pendingAccountSearchParameters);
        }

        [HttpPost, /*ValidateAntiForgeryToken,*/ ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxRemoveActiveAccounts(
            ActiveAccountsSearchModel activeAccountSearchParameters, List<AccountId> userIds)
        {
            var allSuccessful = true;

            foreach (var userId in userIds)
            {
                var response =
                    await _portalService.RemoveUserAsync(new RemoveUserRequest(_tokenAuth, userId.eid, userId.id));

                if (!response.RemoveUserResult || response.TokenAuth.Status != TokenStatus.Succes)
                {
                    allSuccessful = false;
                }
            }

            if (!allSuccessful)
            {
                TempData["ErrorMessage"] = "Could not remove all user(s) selected.";
            }

            return RedirectToSearchActiveAccounts(activeAccountSearchParameters);
        }

        [HttpGet]
        public ActionResult AjaxDisableActiveAccounts()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, /*ValidateAntiForgeryToken,*/ ValidateHttpAntiForgeryToken/*, AuthorizeAction(true)*/]
        public async Task<ActionResult> AjaxDisableActiveAccounts(
            ActiveAccountsSearchModel activeAccountSearchParameters, List<AccountId> userIds)
        {
            //var allSuccessful = true;
            var allDisableSuccessful = true;
            var allLicenseRemoveSuccessful = true;

            foreach (var userId in userIds)
            {
                _tokenAuth.Nonce = this.GetAntiForgeryToken();
                var userStatusRequest = new UpdateUserStatusRequest(_tokenAuth, userId.eid, userId.id, UserStatus.Suspended);
                var userStatusResponse =
                    await _portalService.UpdateUserStatusAsync(userStatusRequest);

                if (!userStatusResponse.UpdateUserStatusResult || userStatusResponse.TokenAuth.Status != TokenStatus.Succes)
                {
                    //allSuccessful = false;
                    allDisableSuccessful = false;
                }
                else
                {
                    //licensesRequest.TokenAuth.Nonce = this.GetAntiForgeryToken();
                    _tokenAuth.Nonce = this.GetAntiForgeryToken();
                    var licensesResponse = await _portalService
                        .RemoveUserLicensesAsync(new RemoveUserLicensesRequest(_tokenAuth, userId.eid, userId.id));
                    if (licensesResponse.TokenAuth.Status != TokenStatus.Succes ||
                        !licensesResponse.RemoveUserLicensesResult)
                        allLicenseRemoveSuccessful = false;
                }
            }

            //if (!allSuccessful)
            //{
            //    TempData["ErrorMessage"] = "Could not remove all user(s) selected.";
            //}

            if (!allDisableSuccessful || !allLicenseRemoveSuccessful)
            {
                var errors = new List<string>();
                //var errors = new List<string> { "Could not disable all user(s) selected." };

                if (!allDisableSuccessful) errors.Add("Could not disable all user(s) selected.");
                if (!allLicenseRemoveSuccessful) errors.Add("Could not remove all licenses for user(s) selected.");

                return Json(new { success = false, errors = errors.ToArray() });
            }

            return Json(new { success = true });


            //return RedirectToSearchActiveAccounts(activeAccountSearchParameters);
        }

        [HttpGet]
        public ActionResult AjaxEnableSuspendedAccounts()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, /*ValidateAntiForgeryToken,*/ ValidateHttpAntiForgeryToken/*, AuthorizeAction(true)*/]
        public async Task<ActionResult> AjaxEnableSuspendedAccounts(
            ActiveAccountsSearchModel suspendedAccountSearchParameters, List<AccountId> userIds)
        {
            var allSuccessful = true;

            foreach (var userId in userIds)
            {
                _tokenAuth.Nonce = this.GetAntiForgeryToken();
                var userStatusRequest = new UpdateUserStatusRequest(_tokenAuth, userId.eid, userId.id, UserStatus.Active);
                var userStatusResponse =
                    await _portalService.UpdateUserStatusAsync(userStatusRequest);

                if (!userStatusResponse.UpdateUserStatusResult || userStatusResponse.TokenAuth.Status != TokenStatus.Succes)
                {
                    allSuccessful = false;
                }
            }

            if (!allSuccessful)
            {
                TempData["ErrorMessage"] = "Could not enable one or more user(s) selected.";
            }

            return RedirectToSearchPendingAccounts(suspendedAccountSearchParameters);
        }

        [HttpGet]
        public ActionResult AjaxAssignProLicenses()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost/*, ValidateAntiForgeryToken*/, ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxAssignProLicenses(ActiveAccountsSearchModel activeAccountSearchParameters,
                                                              List<AccountId> userIds)
        {
            var errors = new List<string>();

            foreach (var userId in userIds)
            {
                _tokenAuth.Nonce = this.GetAntiForgeryToken();
                var response = await
                    _portalService.InsertUserLicenseAsync(new InsertUserLicenseRequest(_tokenAuth, userId.eid, userId.id));

                if (response.InsertUserLicenseResult.Status == InsertUserLicenseStatus.Success) continue;

                var userName = string.Empty;
                var userResponse = await
                                   _portalService.GetAccountDetailsAsync(new GetAccountDetailsRequest(_tokenAuth, userId.eid, userId.id));

                if (userResponse.TokenAuth.Status == TokenStatus.Succes)
                {
                    userName = userResponse.GetAccountDetailsResult.UserName;
                }

                switch (response.InsertUserLicenseResult.Status)
                {
                    case InsertUserLicenseStatus.Insufficient_Amount:
                        var companyName = string.Empty;
                        var companyResponse = await
                                              _portalService.GetCompanyDetailsAsync(new GetCompanyDetailsRequest(_tokenAuth, userId.eid));
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

            return errors.Count > 0 ? Json(new { success = false, errors = errors.ToArray() }) : Json(new { success = true });
        }

        [HttpGet]
        public ActionResult AjaxRemovePendingAccounts()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, /*ValidateAntiForgeryToken,*/ ValidateHttpAntiForgeryToken]
        public async Task<ActionResult> AjaxRemovePendingAccounts(
            PendingAccountsSearchModel pendingAccountsSearchParameters, List<AccountId> userIds)
        {
            //var allSuccessful = true;
            var errors = new List<string>();

            foreach (var userId in userIds)
            {
                _tokenAuth.Nonce = this.GetAntiForgeryToken();
                var removeAccountRequest = new RemovePendingAccountRequest(_tokenAuth, userId.eid, userId.id);
                var removeAccountResponse = await
                    _portalService.RemovePendingAccountAsync(removeAccountRequest);

                if (removeAccountResponse.RemovePendingAccountResult && removeAccountResponse.TokenAuth.Status == TokenStatus.Succes) continue;

                errors.Add(string.Format("Pending account ({0}) could not be removed.", userId.email));

                // if it failed try and get the user details to report which user could not be removed.
                //_tokenAuth.Nonce = this.GetAntiForgeryToken();
                //var userDetailRequest = new GetAccountDetailsRequest(_tokenAuth, userId.eid, userId.id);
                //var userDetailResponse = _portalService.GetAccountDetails(userDetailRequest);

                //if (userDetailResponse.TokenAuth.Status == TokenStatus.Succes &&
                //    userDetailResponse.GetAccountDetailsResult != null)
                //{
                //    var userName = userDetailResponse.GetAccountDetailsResult.UserName;
                //    errors.Add(string.Format("Pending account ({0}) could not be removed.", userName));
                //}
                //else
                //{
                //    // if getting user details fails just add a message to hint which user id couldnt' be removed.
                //    errors.Add(string.Format("Could not be remove pending account ({0}-{1}).", userId.eid, userId.id));
                //}
                //allSuccessful = false;
            }

            //if (!allSuccessful)
            //{
            //    //TempData["ErrorMessage"] = 
            //    errors.Add("Could not remove all user(s) selected.");
            //}

            //return RedirectToSearchPendingAccounts(pendingAccountsSearchParameters);

            return errors.Count > 0 ? Json(new { success = false, errors = errors.ToArray() }) : Json(new { success = true });
        }

        /* Private Helpers */
        #region Private Helpers
        private RedirectToRouteResult RedirectToSearchActiveAccounts(
            UserSearchParametersModel activeAccountSearchParameters)
        {
            return RedirectToAction("AjaxSearchActiveAccounts", new RouteValueDictionary
                {
                    {"SearchTerm", activeAccountSearchParameters.SearchTerm},
                    {"SortOrder", activeAccountSearchParameters.SortOrder},
                    {"CurrentPage", activeAccountSearchParameters.CurrentPage},
                    {"PageSize", activeAccountSearchParameters.PageSize},
                    {"SearchType", activeAccountSearchParameters.SearchType},
                    {"SortField", activeAccountSearchParameters.SortField},
                    {"ItemCount", activeAccountSearchParameters.ItemCount}
                });
        }

        //private RedirectToRouteResult RedirectSuspendedAccounts(ActiveAccountsSearchModel suspendedAccountSearchParameters)
        //{
        //    return RedirectToAction("AjaxSearchSuspendedAccounts", new RouteValueDictionary
        //        {
        //            {"SearchTerm", suspendedAccountSearchParameters.SearchTerm},
        //            {"SortOrder", suspendedAccountSearchParameters.SortOrder},
        //            {"CurrentPage", suspendedAccountSearchParameters.CurrentPage},
        //            {"PageSize", suspendedAccountSearchParameters.PageSize},
        //            {"SearchType", suspendedAccountSearchParameters.SearchType},
        //            {"SortField", suspendedAccountSearchParameters.SortField},
        //            {"ItemCount", suspendedAccountSearchParameters.ItemCount}
        //        });
        //}

        private RedirectToRouteResult RedirectToSearchPendingAccounts(
            UserSearchParametersModel pendingAccountSearchParameters)
        {
            return RedirectToAction("AjaxSearchPendingAccounts", new RouteValueDictionary
                {
                    {"SearchTerm", pendingAccountSearchParameters.SearchTerm},
                    {"SortOrder", pendingAccountSearchParameters.SortOrder},
                    {"CurrentPage", pendingAccountSearchParameters.CurrentPage},
                    {"PageSize", pendingAccountSearchParameters.PageSize},
                    {"SearchType", pendingAccountSearchParameters.SearchType},
                    {"SortField", pendingAccountSearchParameters.SortField},
                    {"ItemCount", pendingAccountSearchParameters.ItemCount}
                });
        }

        private static SearchResultsModel InitializeSearchResultsModel(string searchTerm)
        {
            return new SearchResultsModel
                {
                    SearchTerm = searchTerm,
                    ActiveAccountSearchParameters = new ActiveAccountsSearchModel(),
                    SuspendedAccountSearchParameters = new ActiveAccountsSearchModel(),
                    PendingAccountSearchParameters = new PendingAccountsSearchModel(),
                    CompanySearchResults = new CompanySearchModel
                        {
                            CurrentPage = 1,
                            PageSize = 15,
                            TableBodyPartialView = "_CompanyListPartial"
                        }
                };
        }

        private async Task FillCompanies(SearchResultsModel viewModel, long entityId, long userId, string searchTerm)
        {
            viewModel.CompanySearchResults.SearchTerm = viewModel.SearchTerm;

            var companyListItemModels = await RetrieveCompanies(entityId, userId, searchTerm);

            var listItemModels = companyListItemModels as IList<CompanyListItemModel> ?? companyListItemModels.ToList();
            viewModel.CompanySearchResults.ItemCount = listItemModels.LongCount();

            viewModel.CompanySearchResults.DataItems = listItemModels;
        }

        private async Task FillActiveAccounts(SearchResultsModel viewModel)
        {
            viewModel.ActiveAccountSearchParameters.SearchTerm = viewModel.SearchTerm;

            var accountsResponse = await RetrieveActiveAccounts(viewModel.ActiveAccountSearchParameters);

            if (accountsResponse.TokenAuth.Status == TokenStatus.Succes)
            {
                FillActiveAccountsResult(viewModel.ActiveAccountSearchParameters, accountsResponse);
            }
            else
            {
                Trace.TraceError("Token error: {0}",
                                 (object)TokenStatus.ErrorMessageDictionary[accountsResponse.TokenAuth.Status]);

                ModelState.AddModelError(string.Empty, AllResources.ErrorMessageCouldNotRetrieveActiveAccounts);
            }
        }

        private async Task FillSuspendedAccounts(SearchResultsModel viewModel)
        {
            viewModel.SuspendedAccountSearchParameters.SearchTerm = viewModel.SearchTerm;

            var accountsResponse = await RetrieveSuspendedAccounts(viewModel.ActiveAccountSearchParameters);

            if (accountsResponse.TokenAuth.Status == TokenStatus.Succes)
            {
                FillSuspendedAccountsResult(viewModel.SuspendedAccountSearchParameters, accountsResponse);
            }
            else
            {
                Trace.TraceError("Token error: {0}",
                                 (object)TokenStatus.ErrorMessageDictionary[accountsResponse.TokenAuth.Status]);

                ModelState.AddModelError(string.Empty, AllResources.ErrorMessageCouldNotRetrieveSuspendedAccounts);
            }
        }

        private async Task FillPendingAccounts(SearchResultsModel viewModel)
        {
            viewModel.PendingAccountSearchParameters.SearchTerm = viewModel.SearchTerm;

            var accountsResponse =
                await RetrievePendingAccounts(viewModel.PendingAccountSearchParameters);

            if (accountsResponse.TokenAuth.Status == TokenStatus.Succes)
            {
                FillPendingAccountsResult(viewModel.PendingAccountSearchParameters, accountsResponse);
            }
            else
            {
                Trace.TraceError("Token error: {0}",
                                 (object)TokenStatus.ErrorMessageDictionary[accountsResponse.TokenAuth.Status]);

                ModelState.AddModelError(string.Empty, AllResources.ErrorMessageCouldNotRetreivePendingAccounts);
            }
        }

        private async Task<GetAccountsListResponse> RetrieveActiveAccounts(UserSearchParametersModel searchParameters)
        {
            var request = BuildActiveAccountsRequest(searchParameters, UserStatus.Active);

            return await _portalService.GetAccountsListAsync(request);
        }

        private async Task<GetAccountsListResponse> RetrieveSuspendedAccounts(UserSearchParametersModel searchParameters)
        {
            var request = BuildActiveAccountsRequest(searchParameters, UserStatus.Suspended);

            return await _portalService.GetAccountsListAsync(request);
        }

        private async Task<GetPendingAccountsResponse> RetrievePendingAccounts(
            UserSearchParametersModel searchParameters)
        {
            var request = BuildPendingAccountsRequest(searchParameters);

            return await _portalService.GetPendingAccountsAsync(request);
        }

        private static void FillActiveAccountsResult(ActiveAccountsSearchModel activeAccountSearchParameters,
                                                     GetAccountsListResponse accountsResponse)
        {
            activeAccountSearchParameters.ActiveAccounts =
                Mapper.Map<UserAccountListItem[], IEnumerable<ActiveAccountsListItemModel>>(
                    accountsResponse.GetAccountsListResult.Accounts);

            activeAccountSearchParameters.ItemCount = accountsResponse.GetAccountsListResult.TotalRecords;
        }

        private static void FillSuspendedAccountsResult(ActiveAccountsSearchModel activeAccountSearchParameters,
                                                     GetAccountsListResponse accountsResponse)
        {
            activeAccountSearchParameters.ActiveAccounts =
                Mapper.Map<UserAccountListItem[], IEnumerable<ActiveAccountsListItemModel>>(
                    accountsResponse.GetAccountsListResult.Accounts);

            activeAccountSearchParameters.ItemCount = accountsResponse.GetAccountsListResult.TotalRecords;
        }

        private static void FillPendingAccountsResult(PendingAccountsSearchModel pendingAccountSearchParameters,
                                                      GetPendingAccountsResponse accountsResponse)
        {
            pendingAccountSearchParameters.PendingAccounts =
                Mapper.Map<PendingUserAccountListItem[], IEnumerable<PendingAccountsListItemModel>>(
                    accountsResponse.GetPendingAccountsResult.Accounts);

            pendingAccountSearchParameters.ItemCount = accountsResponse.GetPendingAccountsResult.TotalRecords;
        }

        private static UserSearchModel InitializeDefaultSearchModel(string searchTerm)
        {
            return new UserSearchModel
                {
                    SearchLocation = UserSearchParametersModel.UserSearchLocation.Both,
                    SearchTerm = searchTerm,
                    SearchParameters = new UserSearchParametersModel
                        {
                            CurrentPage = 1,
                            SearchTerm = searchTerm,
                            SearchType = UserSearchParametersModel.UserSearchType.Any,
                            PageSize = int.MaxValue
                        }
                };
        }

        private async Task<IEnumerable<CompanyListItemModel>> RetrieveCompanies(long entityId, long userId, string filter = null)
        {
            var getUserCompaniesRequest = new GetUserCompaniesRequest(_tokenAuth, entityId, userId, userId, CompanyStatus.ANY);

            Debug.Print("userId: " + userId);
            Debug.Print("entityId: " + entityId);
            Debug.Print("token: " + _tokenAuth.Token);

            var response = await _portalService.GetUserCompaniesAsync(getUserCompaniesRequest);

            var companyList =
                Mapper.Map<CompanyListItem[], IEnumerable<CompanyListItemModel>>(response.GetUserCompaniesResult);

            if (!string.IsNullOrEmpty(filter))
            {
                companyList = from c in companyList
                              where CompareNames(filter, c.Name)
                              select c;
            }

            return companyList;
        }

        private static bool CompareNames(string searchName, string companyName)
        {
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(companyName, searchName, CompareOptions.IgnoreCase) >= 0;
        }

        private async Task SearchActiveAccounts(UserSearchModel model)
        {
            var request = BuildActiveAccountsRequest(model.SearchParameters, UserStatus.Active);

            var response = await _portalService.GetAccountsListAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetAccountsListResult != null)
            {
                var accounts = Mapper.Map<UserAccountListItem[],
                    IEnumerable<ActiveAccountsListItemModel>>(response.GetAccountsListResult.Accounts);

                model.ActiveAccounts = new PageableViewModel<ActiveAccountsListItemModel>
                    {
                        DataItems = accounts,
                        CurrentPage = model.SearchParameters.CurrentPage - 1,
                        PageSize = model.SearchParameters.PageSize
                    };

                model.SearchParameters.ItemCount = model.ActiveAccounts.DataItems.Count();
            }
            else
            {
                Trace.TraceError("Token error: {0}",
                                 (object)TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]);

                ModelState.AddModelError("ActiveAccounts", AllResources.ErrorMessageCouldNotRetrieveActiveAccounts);
            }
        }

        private async Task SearchSuspendedAccounts(UserSearchModel model)
        {
            var request = BuildActiveAccountsRequest(model.SearchParameters, UserStatus.Suspended);

            var response = await _portalService.GetAccountsListAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetAccountsListResult != null)
            {
                var accounts = Mapper.Map<UserAccountListItem[],
                    IEnumerable<ActiveAccountsListItemModel>>(response.GetAccountsListResult.Accounts);

                model.SuspendedAccounts = new PageableViewModel<ActiveAccountsListItemModel>
                {
                    DataItems = accounts,
                    CurrentPage = model.SearchParameters.CurrentPage - 1,
                    PageSize = model.SearchParameters.PageSize
                };

                model.SearchParameters.ItemCount = model.ActiveAccounts.DataItems.Count();
            }
            else
            {
                Trace.TraceError("Token error: {0}",
                                 (object)TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]);

                ModelState.AddModelError("SuspendedAccounts", AllResources.ErrorMessageCouldNotRetrieveActiveAccounts);
            }
        }

        private async Task SearchPendingAccounts(UserSearchModel model)
        {
            var request = BuildPendingAccountsRequest(model.SearchParameters);

            var response = await _portalService.GetPendingAccountsAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetPendingAccountsResult != null)
            {
                var accounts =
                    Mapper.Map<PendingUserAccountListItem[], IEnumerable<PendingAccountsListItemModel>>(
                        response.GetPendingAccountsResult.Accounts);

                model.PendingAccounts = new PageableViewModel<PendingAccountsListItemModel>
                    {
                        DataItems = accounts,
                        CurrentPage = model.SearchParameters.CurrentPage - 1,
                        PageSize = model.SearchParameters.PageSize
                    };

                model.SearchParameters.ItemCount = model.PendingAccounts.DataItems.Count();
            }
            else
            {
                Trace.TraceError("Token error: {0}",
                                 (object)TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]);

                ModelState.AddModelError("PendingAccounts", AllResources.ErrorMessageCouldNotRetreivePendingAccounts);
            }
        }

        private GetAccountsListRequest BuildActiveAccountsRequest(UserSearchParametersModel searchParameters, UserStatus userStatus)
        {
            _tokenAuth.Nonce = this.GetAntiForgeryToken();
            return new GetAccountsListRequest
                {
                    TokenAuth = _tokenAuth,
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

        private GetPendingAccountsRequest BuildPendingAccountsRequest(UserSearchParametersModel searchParameters)
        {
            return new GetPendingAccountsRequest
                {
                    TokenAuth = _tokenAuth,
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

        #endregion

        public class AccountId
        {
            // ReSharper disable InconsistentNaming
            public long id { get; set; }
            public long eid { get; set; }
            public string email { get; set; }
            // ReSharper restore InconsistentNaming
        }
    }
}