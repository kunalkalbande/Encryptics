using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using Encryptics.WebPortal.ActionResults;
using Encryptics.WebPortal.Areas.Company.Models;
using Encryptics.WebPortal.Areas.UserAdmin.Models;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.Filters;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using Newtonsoft.Json;

namespace Encryptics.WebPortal.Areas.Company.Controllers
{
    //[AuthorizeAction(true)]
    public class UsageRightsGroupController : PortalServiceAwareController
    {
        private const int _page_Size = 10;

        private static readonly string[] _groupPermissions = new[] { 
            "Company/UsageRightsGroup/Index",
            "Company/UsageRightsGroup/Create",
            "Company/UsageRightsGroup/Delete",
            "Company/UsageRightsGroup/Details"
        };

        private static readonly string[] _groupMemberPermissions = new[] { 
            "Company/UsageRightsGroup/Index",
            "Company/UsageRightsGroup/Details",
            "Company/UsageRightsGroup/AddGroupMember",
            "Company/UsageRightsGroup/RemoveGroupMember",
            "Company/UsageRightsGroup/AjaxSearchActiveAccounts",
            "Company/UsageRightsGroup/Edit"
        };

        public UsageRightsGroupController(PortalServiceSoap portalService)
            : base(portalService)
        {
        }

        #region Actions
        //
        // GET: /Company/UsageRightsGroup/
        public async Task<ActionResult> Index(long entityId, int? currentPage)
        {
            var usageRightsGroups = await GetUsageRightsGroupsAsync(entityId);

            var model = new PageableViewModel<UsageRightsGroupModel>
            {
                CurrentPage = currentPage ?? 1,
                PageSize = _page_Size,
                DataItems = usageRightsGroups
            };

            HandleMessages();
            await InitializeViewBagAsync(entityId);
            await SetViewPermissionsAsync(entityId, _groupPermissions);

            return View(model);
        }

        //
        // GET: /Company/UsageRightsGroup/Details/5

        public async Task<ActionResult> Details(long entityId, long groupId, int? currentPage)
        {
            var usageRightsGroup = await GetGroupDetails(groupId);

            var userAccounts = await GetGroupMembers(groupId);

            usageRightsGroup.GroupMempers = new PageableViewModel<UsageRightsGroupMemberModel>
            {
                CurrentPage = currentPage ?? 1,
                PageSize = _page_Size,
                DataItems = userAccounts
            };

            HandleMessages();
            await InitializeViewBagAsync(entityId);
            await SetViewPermissionsAsync(entityId, _groupMemberPermissions);

            return View(usageRightsGroup);
        }

        //
        // GET: /Company/UsageRightsGroup/Create

        public ActionResult Create()
        {
            return View(new EditableUsageRightsGroupModel());
        }

        //
        // POST: /Company/UsageRightsGroup/Create

        [HttpPost]
        public async Task<ActionResult> Create(long entityId, EditableUsageRightsGroupModel model, int? currentPage)
        {
            try
            {
                var usageRightsGroup = Mapper.Map<EditableUsageRightsGroupModel, UsageRightsGroup>(model);

                var request = new InsertUsageRightsGroupRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    usage_rights_group = usageRightsGroup
                };

                var response = await _portalService.InsertUsageRightsGroupAsync(request);

                if (response.TokenAuth.Status != TokenStatus.Succes ||
                    response.InsertUsageRightsGroupResult.Status != InsertUsageRightsGroupStatus.Success)
                {
                    switch (response.InsertUsageRightsGroupResult.Status)
                    {
                        case InsertUsageRightsGroupStatus.Access_Denied:
                            TempData["ErrorMessage"] = "Could not insert Usage Rights Group (Access Denied).";
                            break;
                        case InsertUsageRightsGroupStatus.Failed:
                            TempData["ErrorMessage"] = "Could not insert Usage Rights Group (Failed).";
                            break;
                        case InsertUsageRightsGroupStatus.Name_Not_Available:
                            TempData["ErrorMessage"] = "Could not insert Usage Rights Group (Name not available).";
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Trace.TraceError(exception.Message);
                Trace.TraceInformation(exception.StackTrace);

                TempData["ErrorMessage"] = "Could not insert Usage Rights Group (Exception).";
            }

            return RedirectToAction("Index", new { entityId, currentPage });
        }

        //
        // GET: /Company/UsageRightsGroup/Edit/5

        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //
        // POST: /Company/UsageRightsGroup/Edit/5

        //
        // POST: /Company/UsageRightsGroup/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long entityId, int groupId, EditableUsageRightsGroupModel model, int? currentPage)
        {
            try
            {
                var request = new UpdateUsageRightsGroupRequest
                {
                    TokenAuth = _tokenAuth,
                    group_id = model.GroupId,
                    usage_rights_group = Mapper.Map<EditableUsageRightsGroupModel, UsageRightsGroup>(model)
                };

                var response = await _portalService.UpdateUsageRightsGroupAsync(request);

                if (response.TokenAuth.Status != TokenStatus.Succes ||
                    response.UpdateUsageRightsGroupResult != UpdateUsageRightsGroupResult.Success)
                {
                    TempData["ErrorMessage"] = response.UpdateUsageRightsGroupResult == UpdateUsageRightsGroupResult.Access_Denied
                                                   ? "Could not update Usage Rights Group (Access Denied)."
                                                   : "Could not update Usage Rights Group (Failed).";
                }

            }
            catch (Exception exception)
            {
                Trace.TraceError(exception.Message);
                Trace.TraceInformation(exception.StackTrace);

                TempData["ErrorMessage"] = "Could not update Usage Rights Group (Exception).";
            }

            return RedirectToAction("Details", new { entityId, groupId, currentPage });
        }

        //
        // GET: /Company/UsageRightsGroup/Delete/5

        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //
        // POST: /Company/UsageRightsGroup/Delete/5

        //
        // POST: /Company/UsageRightsGroup/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(long entityId, ICollection<long> groupIds, int? currentPage)
        {
            try
            {
                var request = new UpdateUsageRightsGroupStatusRequest
                {
                    TokenAuth = _tokenAuth,
                    is_active = false
                };

                foreach (var id in groupIds)
                {
                    request.group_id = id;

                    var response = await _portalService.UpdateUsageRightsGroupStatusAsync(request);

                    if (response.TokenAuth.Status != TokenStatus.Succes ||
                        response.UpdateUsageRightsGroupStatusResult != UpdateUsageRightsGroupResult.Success)
                    {
                        TempData["ErrorMessage"] = response.UpdateUsageRightsGroupStatusResult == UpdateUsageRightsGroupResult.Access_Denied
                                                   ? "Could not update Usage Rights Group status (Access Denied)."
                                                   : "Could not update Usage Rights Group status (Failed).";
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                TempData["ErrorMessage"] = "Updating existing Usage Rights Group status failed (exception).";
            }

            return RedirectToAction("Index", new { entityId, currentPage });
        }

        //
        // GET: /Company/UsageRightsGroup/AddGroupMember
        public ActionResult AddGroupMember()
        {
            return View();
        }

        //
        // POST: /Company/UsageRightsGroup/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> AddGroupMember(long entityId, long groupId, long userId, int? currentPage)
        {
            try
            {
                var request = new InsertUsageRightsGroupUserRequest
                {
                    TokenAuth = _tokenAuth,
                    group_id = groupId,
                    user_id = userId
                };
                var response = await _portalService.InsertUsageRightsGroupUserAsync(request);

                if (response.TokenAuth.Status != TokenStatus.Succes ||
                    response.InsertUsageRightsGroupUserResult.Status != InsertUsageRightsGroupUserStatus.Success)
                {
                    switch (response.InsertUsageRightsGroupUserResult.Status)
                    {
                        case InsertUsageRightsGroupUserStatus.Access_Denied:
                            TempData["ErrorMessage"] = "Could not insert Usage Rights Group Member (Access Denied).";
                            break;
                        case InsertUsageRightsGroupUserStatus.Failed:
                            TempData["ErrorMessage"] = "Could not insert Usage Rights Group Member (Failed).";
                            break;
                        case InsertUsageRightsGroupUserStatus.Group_Not_Exists:
                            TempData["ErrorMessage"] = "Could not insert Usage Rights Group Member (Group does not exist).";
                            break;
                        case InsertUsageRightsGroupUserStatus.User_Exists:
                            TempData["ErrorMessage"] = "Could not insert Usage Rights Group Member (User already exists).";
                            break;
                        case InsertUsageRightsGroupUserStatus.User_In_Another_Group:
                            TempData["ErrorMessage"] = "Could not insert Usage Rights Group Member (User already exists in another group).";
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Trace.TraceError(exception.Message);
                Trace.TraceInformation(exception.StackTrace);

                TempData["ErrorMessage"] = "Could not update Usage Rights Group Member (Exception).";
            }

            return RedirectToAction("Details", new { entityId, groupId, currentPage });
        }

        // GET: /Company/CompanyAdminRoles/AjaxSearchActiveAccounts/5
        [HttpGet, ValidateHttpAntiForgeryToken]
        public async Task<JsonResult> AjaxSearchActiveAccounts(long entityId, string searchTerm)
        {
            var accountsResponse = await RetrieveActiveAccountsAsync(entityId, searchTerm);

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
        // POST: /Company/UsageRightsGroup/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveGroupMember(long entityId, long groupId, ICollection<long> groupMemberIds, int? currentPage)
        {
            try
            {
                var request = new RemoveUsageRightsGroupUserRequest
                {
                    TokenAuth = _tokenAuth
                };

                foreach (var id in groupMemberIds)
                {
                    request.usage_rights_group_user_id = id;

                    var response = await _portalService.RemoveUsageRightsGroupUserAsync(request);

                    if (response.TokenAuth.Status != TokenStatus.Succes ||
                        response.RemoveUsageRightsGroupUserResult != RemoveUsageRightsGroupUserResult.Success)
                    {
                        TempData["ErrorMessage"] = response.RemoveUsageRightsGroupUserResult == RemoveUsageRightsGroupUserResult.Access_Denied
                                                   ? "Could not remove Usage Rights Group Member (Access Denied)."
                                                   : "Could not remove Usage Rights Group Member (Failed).";
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                TempData["ErrorMessage"] = "Removing existing Usage Rights Group failed (exception).";
            }

            return RedirectToAction("Details", new { entityId, groupId, currentPage });
        }

        #endregion

        #region Helper Methods

        private async Task<IEnumerable<UsageRightsGroupModel>> GetUsageRightsGroupsAsync(long entityId)
        {
            IEnumerable<UsageRightsGroupModel> dataItems;

            var request = new GetUsageRightsGroupsRequest
            {
                TokenAuth = _tokenAuth,
                entity_id = entityId
            };

            var response = await _portalService.GetUsageRightsGroupsAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetUsageRightsGroupsResult != null)
            {
                dataItems =
                    Mapper.Map<UsageRightsGroup[], IEnumerable<UsageRightsGroupModel>>(response.GetUsageRightsGroupsResult);
            }
            else
            {
                dataItems = new List<UsageRightsGroupModel>();
                ViewData.Add("ErrorMessage", "Could not retrieve usage rights groups.");
            }

            return dataItems;
        }

        private async Task<EditableUsageRightsGroupModel> GetGroupDetails(long groupId)
        {
            var request = new GetUsageRightsGroupDetailsRequest
            {
                TokenAuth = _tokenAuth,
                group_id = groupId
            };

            var response = await _portalService.GetUsageRightsGroupDetailsAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetUsageRightsGroupDetailsResult != null)
            {
                return Mapper.Map<UsageRightsGroup, EditableUsageRightsGroupModel>(response.GetUsageRightsGroupDetailsResult);
            }

            return null;
        }

        private async Task<IEnumerable<UsageRightsGroupMemberModel>> GetGroupMembers(long groupId)
        {
            var request = new GetUsageRightsGroupUsersRequest
            {
                TokenAuth = _tokenAuth,
                group_id = groupId
            };

            var response = await _portalService.GetUsageRightsGroupUsersAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetUsageRightsGroupUsersResult != null)
            {
                return Mapper.Map<IEnumerable<UsageRightsGroupMemberModel>>(response.GetUsageRightsGroupUsersResult);
            }

            return null;
        }

        private async Task<GetAccountsListResponse> RetrieveActiveAccountsAsync(long entityId, string searchTerm)
        {
            var request = BuildActiveAccountsRequest(entityId, searchTerm);

            return await _portalService.GetAccountsListAsync(request);
        }

        private GetAccountsListRequest BuildActiveAccountsRequest(long entityId, string searchTerm)
        {
            return new GetAccountsListRequest
            {
                TokenAuth = _tokenAuth,
                entity_id = entityId,
                term = searchTerm,
                page = 1,
                page_size = int.MaxValue
            };
        }

        #endregion
    }
}
