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
    [AuthorizeAction(true)]
    public class DistributionGroupMembersController : PortalServiceAwareController
    {
        private const int PAGE_SIZE = 10;
        //
        // GET: /Company/DistributionGroupMembers/

        public DistributionGroupMembersController(PortalServiceSoap portalService)
            : base(portalService)
        {
        }

        public async Task<ActionResult> Index(long entityId, long groupId, int? currentPage)
        {
            var distrubutionGroup = await GetGroupDetails(groupId);

            var userAccounts = await GetGroupMembers(groupId);

            distrubutionGroup.GroupMempers = new PageableViewModel<DistributionGroupMemberModel>
                {
                    CurrentPage = currentPage ?? 0,
                    PageSize = PAGE_SIZE,
                    DataItems = userAccounts
                };

            HandleMessages();
            await InitializeViewBagAsync(entityId);

            return View(distrubutionGroup);
        }

        //
        // GET: /Company/DistributionGroupMembers/Details/5

        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        //
        // GET: /Company/DistributionGroupMembers/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Company/DistributionGroupMembers/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(long entityId, long groupId, string userName, int? currentPage)
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

            return RedirectToAction("Index", new { entityId, groupId, currentPage });
        }

        //
        // GET: /Company/DistributionGroupMembers/Edit/5

        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //
        // POST: /Company/DistributionGroupMembers/Edit/5

        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //
        // GET: /Company/DistributionGroupMembers/Delete/5

        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //
        // POST: /Company/DistributionGroupMembers/Delete/5

        [HttpPost]
        public async Task<ActionResult> Delete(long entityId, long groupId, ICollection<long> groupMemberIds, int? currentPage)
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
                                                    : "Could not update Distribution Group Member (Failed).";
                     }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                TempData["ErrorMessage"] = "Deleting existing Distribution Group failed (exception).";
            }

            var userAccounts = await GetGroupMembers(groupId);

            var model = new PageableViewModel<DistributionGroupMemberModel>
            {
                CurrentPage = currentPage ?? 0,
                PageSize = PAGE_SIZE,
                DataItems = userAccounts
            };

            return PartialView("_groupMemberTable", model);
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
                    accountsResponse.GetAccountsResult.Accounts);

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

        #region Helper Methods
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

        private async Task<GetAccountsResponse> RetrieveActiveAccountsAsync(string searchTerm)
        {
            var request = BuildActiveAccountsRequest(searchTerm);

            return await _portalService.GetAccountsAsync(request);
        }

        private GetAccountsRequest BuildActiveAccountsRequest(string searchTerm)
        {
            return new GetAccountsRequest
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
