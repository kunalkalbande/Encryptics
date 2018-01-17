using AutoMapper;
using Encryptics.WebPortal.ActionResults;
using Encryptics.WebPortal.Areas.Company.Models;
using Encryptics.WebPortal.Areas.UserAdmin.Models;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.Filters;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Areas.Company.Controllers
{
    public class DistributionGroupController : PortalServiceAwareController
    {
        private const int _page_Size = 10;

        private static readonly string[] _groupPermissions = new[] { 
            "Company/DistributionGroup/Index",
            "Company/DistributionGroup/Create",
            "Company/DistributionGroup/Delete",
            "Company/DistributionGroup/Details"
        };

        private static readonly string[] _groupMemberPermissions = new[] { 
            "Company/DistributionGroup/Index",
            "Company/DistributionGroup/Details",
            "Company/DistributionGroup/AddGroupMember",
            "Company/DistributionGroup/RemoveGroupMember",
            "Company/DistributionGroup/AjaxSearchActiveAccounts",
            "Company/DistributionGroup/Edit"
        };

        //
        // GET: /Company/DistributionGroup/
        public DistributionGroupController(PortalServiceSoap portalService)
            : base(portalService)
        {
        }

        public async Task<ActionResult> Index(long entityId, int? currentPage)
        {
            var distributionGroups = await GetDistributionGroupsAsync(entityId);

            var model = new PageableViewModel<DistributionGroupModel>
                {
                    CurrentPage = currentPage ?? 1,
                    PageSize = _page_Size,
                    DataItems = distributionGroups
                };

            HandleMessages();
            await InitializeViewBagAsync(entityId);
            await SetViewPermissionsAsync(entityId, _groupPermissions);

            return View(model);
        }

        //
        // GET: /Company/DistributionGroup/Details/5
        public async Task<ActionResult> Details(long entityId, long groupId, int? currentPage)
        {
            var distrubutionGroup = await GetGroupDetails(groupId);

            var userAccounts = await GetGroupMembers(groupId);

            distrubutionGroup.GroupMempers = new PageableViewModel<DistributionGroupMemberModel>
            {
                CurrentPage = currentPage ?? 1,
                PageSize = _page_Size,
                DataItems = userAccounts
            };

            HandleMessages();
            await InitializeViewBagAsync(entityId);
            await SetViewPermissionsAsync(entityId, _groupMemberPermissions);

            return View(distrubutionGroup);
        }

        //
        // GET: /Company/DistributionGroup/Create
        public ActionResult Create()
        {
            var distributionGroupModel = new DistributionGroupModel();

            return View(distributionGroupModel);
        }

        //
        // POST: /Company/DistributionGroup/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(long entityId, DistributionGroupModel model, int? currentPage)
        {
            try
            {
                var request = new InsertAccessGroupRequest
                    {
                        TokenAuth = _tokenAuth,
                        entity_id = entityId,
                        is_active = true,
                        name = model.GroupName
                    };

                var response = await _portalService.InsertAccessGroupAsync(request);

                if (response.TokenAuth.Status != TokenStatus.Succes ||
                    response.InsertAccessGroupResult.Status != InsertAccessGroupStatus.Success)
                {
                    switch (response.InsertAccessGroupResult.Status)
                    {
                        case InsertAccessGroupStatus.Access_Denied:
                            TempData["ErrorMessage"] = "Could not insert Distribution Group (Access Denied).";
                            break;
                        case InsertAccessGroupStatus.Failed:
                            TempData["ErrorMessage"] = "Could not insert Distribution Group (Failed).";
                            break;
                        case InsertAccessGroupStatus.Name_Not_Available:
                            TempData["ErrorMessage"] = "Could not insert Distribution Group (Name not available).";
                            break;
                        case InsertAccessGroupStatus.Parameters_Missing:
                            TempData["ErrorMessage"] = "Could not insert Distribution Group (Parameter missing).";
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Trace.TraceError(exception.Message);
                Trace.TraceInformation(exception.StackTrace);

                TempData["ErrorMessage"] = "Could not insert Distribution Group (Exception).";
            }

            return RedirectToAction("Index", new { entityId, currentPage });
        }

        //
        // GET: /Company/DistributionGroup/Edit/5
        public async Task<ActionResult> Edit(long entityId, int groupId, int? currentPage)
        {
            var distributionGroup = await GetGroupDetails(groupId);

            return PartialView(distributionGroup);
        }

        //
        // POST: /Company/DistributionGroup/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long entityId, int groupId, DistributionGroupModel model, int? currentPage)
        {
            try
            {
                var request = new UpdateAccessGroupRequest
                    {
                        TokenAuth = _tokenAuth,
                        group_id = model.GroupId,
                        name = model.GroupName,
                        is_active = model.IsActive
                    };

                var response = await _portalService.UpdateAccessGroupAsync(request);

                if (response.TokenAuth.Status != TokenStatus.Succes ||
                    response.UpdateAccessGroupResult != UpdateAccessGroupResult.Success)
                {
                    TempData["ErrorMessage"] = response.UpdateAccessGroupResult == UpdateAccessGroupResult.Access_Denied
                                                   ? "Could not update Distribution Group (Access Denied)."
                                                   : "Could not update Distribution Group (Failed).";
                }

            }
            catch (Exception exception)
            {
                Trace.TraceError(exception.Message);
                Trace.TraceInformation(exception.StackTrace);

                TempData["ErrorMessage"] = "Could not update Distribution Group (Exception).";
            }

            return RedirectToAction("Details", new { entityId, groupId, currentPage });
        }

        //
        // POST: /Company/DistributionGroup/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(long entityId, ICollection<long> groupIds, int? currentPage)
        {
            try
            {
                var request = new UpdateAccessGroupRequest
                    {
                        TokenAuth = _tokenAuth,
                        is_active = false
                    };

                foreach (var id in groupIds)
                {
                    request.group_id = id;

                    var response = await _portalService.UpdateAccessGroupAsync(request);

                    if (response.TokenAuth.Status != TokenStatus.Succes ||
                        response.UpdateAccessGroupResult != UpdateAccessGroupResult.Success)
                    {
                        TempData["ErrorMessage"] = response.UpdateAccessGroupResult == UpdateAccessGroupResult.Access_Denied
                                                   ? "Could not update Distribution Group (Access Denied)."
                                                   : "Could not update Distribution Group (Failed).";
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                TempData["ErrorMessage"] = "Deleting existing Distribution Group failed (exception).";
            }

            //var distributionGroups = await GetDistributionGroupsAsync(entityId);
            //var model = new PageableViewModel<DistributionGroupModel>
            //{
            //    CurrentPage = currentPage ?? 1,
            //    PageSize = PAGE_SIZE,
            //    DataItems = distributionGroups
            //};

            return RedirectToAction("Index", new { entityId, currentPage });
            //return PartialView("_distributionGroupTable", model);
        }

        //
        // GET: /Company/DistributionGroup/AddGroupMember
        public ActionResult AddGroupMember()
        {
            return View();
        }

        //
        // POST: /Company/DistributionGroup/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> AddGroupMember(long entityId, long groupId, string userName, int? currentPage)
        {
            try
            {
                var request = new InsertAccessGroupUserRequest
                {
                    TokenAuth = _tokenAuth,
                    group_id = groupId,
                    user_name = userName
                };
                var response = await _portalService.InsertAccessGroupUserAsync(request);

                if (response.TokenAuth.Status != TokenStatus.Succes ||
                    response.InsertAccessGroupUserResult.Status != InsertAccessGroupUserStatus.Success)
                {
                    switch (response.InsertAccessGroupUserResult.Status)
                    {
                        case InsertAccessGroupUserStatus.Access_Denied:
                            TempData["ErrorMessage"] = "Could not insert Distribution Group Member (Access Denied).";
                            break;
                        case InsertAccessGroupUserStatus.Failed:
                            TempData["ErrorMessage"] = "Could not insert Distribution Group Member (Failed).";
                            break;
                        case InsertAccessGroupUserStatus.Group_Not_Exists:
                            TempData["ErrorMessage"] = "Could not insert Distribution Group Member (Group does not exist).";
                            break;
                        case InsertAccessGroupUserStatus.User_Exists:
                            TempData["ErrorMessage"] = "Could not insert Distribution Group Member (User already exists).";
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Trace.TraceError(exception.Message);
                Trace.TraceInformation(exception.StackTrace);

                TempData["ErrorMessage"] = "Could not update Distribution Group Member (Exception).";
            }

            return RedirectToAction("Details", new { entityId, groupId, currentPage });
        }

        // GET: /Company/CompanyAdminRoles/AjaxSearchActiveAccounts/5
        [HttpGet, ValidateHttpAntiForgeryToken]
        public async Task<JsonResult> AjaxSearchActiveAccounts(string searchTerm)
        {
            var accountsResponse = await RetrieveActiveAccountsAsync(searchTerm);

            if (accountsResponse.TokenAuth.Status != TokenStatus.Succes)
            {
                return new JsonResult
                {
                    Data = new { errors = new[] { "Could not retrieve active accounts." } },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            var searchResults =
                Mapper.Map<UserAccountListItem[], IEnumerable<ActiveAccountsListItemModel>>(
                    accountsResponse.GetAccountsListResult.Accounts);

            var companies = from a in searchResults
                            select
                                new
                                {
                                    label = a.UserName,
                                    value = a.Id
                                };

            var jsonpResult = new JsonpResult
            {
                Data = companies,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            Debug.Print("Result: {0}", JsonConvert.SerializeObject(jsonpResult));

            return jsonpResult;
        }

        //
        // POST: /Company/DistributionGroup/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveGroupMember(long entityId, long groupId, ICollection<long> groupMemberIds, int? currentPage)
        {
            try
            {
                var request = new RemoveAccessGroupUserRequest
                {
                    TokenAuth = _tokenAuth
                };

                foreach (var id in groupMemberIds)
                {
                    request.group_user_id = id;

                    var response = await _portalService.RemoveAccessGroupUserAsync(request);

                    if (response.TokenAuth.Status != TokenStatus.Succes ||
                        response.RemoveAccessGroupUserResult != RemoveAccessGroupUserResult.Success)
                    {
                        TempData["ErrorMessage"] = response.RemoveAccessGroupUserResult == RemoveAccessGroupUserResult.Access_Denied
                                                   ? "Could not remove Distribution Group Member (Access Denied)."
                                                   : "Could not remove Distribution Group Member (Failed).";
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                TempData["ErrorMessage"] = "Removing existing Distribution Group failed (exception).";
            }

            //var userAccounts = await GetGroupMembers(groupId);

            //var model = new PageableViewModel<DistributionGroupMemberModel>
            //{
            //    CurrentPage = currentPage ?? 0,
            //    PageSize = PAGE_SIZE,
            //    DataItems = userAccounts
            //};

            return RedirectToAction("Details", new { entityId, groupId, currentPage });
            //return PartialView("_groupMemberTable", model);
        }

        #region Helper Methods
        private async Task<IEnumerable<DistributionGroupModel>> GetDistributionGroupsAsync(long entityId)
        {
            IEnumerable<DistributionGroupModel> dataItems;

            var request = new GetAccessGroupsRequest
            {
                TokenAuth = _tokenAuth,
                entity_id = entityId
            };

            var response = await _portalService.GetAccessGroupsAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetAccessGroupsResult != null)
            {
                dataItems =
                    Mapper.Map<AccessGroup[], IEnumerable<DistributionGroupModel>>(response.GetAccessGroupsResult);
            }
            else
            {
                dataItems = new List<DistributionGroupModel>();
                ViewData.Add("ErrorMessage", "Could not retrieve distribution groups.");
            }

            return dataItems;
        }

        private async Task<DistributionGroupModel> GetGroupDetails(long groupId)
        {
            var request = new GetAccessGroupDetailsRequest
            {
                TokenAuth = _tokenAuth,
                group_id = groupId
            };

            var response = await _portalService.GetAccessGroupDetailsAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetAccessGroupDetailsResult != null)
            {
                return Mapper.Map<AccessGroup, DistributionGroupModel>(response.GetAccessGroupDetailsResult);
            }

            return null;
        }

        private async Task<IEnumerable<DistributionGroupMemberModel>> GetGroupMembers(long groupId)
        {
            var request = new GetAccessGroupUsersRequest
            {
                TokenAuth = _tokenAuth,
                group_id = groupId
            };

            var response = await _portalService.GetAccessGroupUsersAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetAccessGroupUsersResult != null)
            {
                return Mapper.Map<IEnumerable<DistributionGroupMemberModel>>(response.GetAccessGroupUsersResult);
            }

            return null;
        }

        private async Task<GetAccountsListResponse> RetrieveActiveAccountsAsync(string searchTerm)
        {
            var request = BuildActiveAccountsRequest(searchTerm);

            return await _portalService.GetAccountsListAsync(request);
        }

        private GetAccountsListRequest BuildActiveAccountsRequest(string searchTerm)
        {
            return new GetAccountsListRequest
            {
                TokenAuth = _tokenAuth,
                term = searchTerm,
                page = 1,
                page_size = int.MaxValue,
            };
        }

        #endregion
    }
}
