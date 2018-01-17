using System.Data;
using AutoMapper;
using Encryptics.WebPortal.ActionResults;
using Encryptics.WebPortal.Areas.Company.Models;
using Encryptics.WebPortal.Areas.CompanyAdmin.Models;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.Filters;
using Encryptics.WebPortal.IdentityModel;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using LicensePoolType = Encryptics.WebPortal.PortalService.LicensePoolType;

namespace Encryptics.WebPortal.Areas.CompanyAdmin.Controllers
{
    public class CompanyAdminHomeController : PortalServiceAwareController
    {
        private readonly IEnumerable<string> _dashboardPermissions = new[]
            {
                "CompanyAdmin/CompanyAdminHome/AddNewCompany", 
                "CompanyAdmin/CompanyAdminHome/TransferLicenses", 
                "CompanyAdmin/SoftwareReleases/Index"
            };
        private const int _pageSize = 25;

        public CompanyAdminHomeController(PortalServiceSoap portalService)
            : base(portalService)
        {
        }

        #region Actions
        [HttpGet]
        public async Task<ActionResult> Dashboard(string searchTerm, int? currentPage, int? pageSize)
        {
            HandleMessages();

            await SetViewPermissionsAsync(EncrypticsPrincipal.CurrentEncrypticsUser.EntityId, _dashboardPermissions);

            var model = new CompanyAdminModel
                {
                    EncryptsData = await GetEncryptsAsync(), 
                    DecryptsData = await GetDecryptsAsync(),
                    LicenseExpirationData = await GetExpiringLicensesAsync()
                };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult FilterCompanies(string searchTerm, int? currentPage, int? pageSize)
        {
            return RedirectToAction("Dashboard", new { searchTerm, currentPage, pageSize });
        }

        //[HttpGet]
        //public async Task<ActionResult> DeleteEntity(long entityId, string entityName, int? page, string searchTerm)
        //{
        //    var response = await _portalService.RemoveCompanyAsync(new RemoveCompanyRequest
        //        {
        //            TokenAuth = _tokenAuth,
        //            entity_id = entityId,
        //            company_name = entityName
        //        });

        //    if (response.RemoveCompanyResult != RemoveCompanyResult.Success)
        //    {
        //        TempData.Add("ErrorMessage", "Could not remove company: " + response.RemoveCompanyResult.ToString());
        //    }

        //    return RedirectToAction("Dashboard", new { page, searchName = searchTerm });
        //}

        [HttpGet]
        public JsonpResult GetCompanySearchResults()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpPost, ValidateHttpAntiForgeryToken]
        public async Task<JsonResult> GetCompanySearchResults(string searchTerm)
        {
            var companies = await RetrieveCompanies();

            var filteredCompanies = from c in companies
                                    where CompareNames(searchTerm, c.Name)
                                    select new { label = c.Name, value = c.Id };

            var jsonpResult = new JsonpResult
                {
                    Data = filteredCompanies,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

            Debug.Print("Result: {0}", JsonConvert.SerializeObject(jsonpResult));

            return jsonpResult;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> AddNewCompany(CompanyDetailsModel model)
        {
            if (ModelState.IsValid)
            {
                var request = new InsertCompanyRequest
                    {
                        TokenAuth = _tokenAuth,
                        name = model.Name
                    };

                var response = await _portalService.InsertCompanyAsync(request);

                if (response.TokenAuth.Status == TokenStatus.Succes &&
                    response.InsertCompanyResult.Status == InsertCompanyStatus.Success)
                {
                    return RedirectToAction(string.Empty, "CompanyHome",
                                            new { area = "Company", entityId = response.InsertCompanyResult.Id });
                }
                if (response.TokenAuth.Status == TokenStatus.Succes &&
                    response.InsertCompanyResult.Status == InsertCompanyStatus.Name_Not_Available)
                {
                    TempData["ErrorMessage"] = "Could not add new company. That company name is not available.";

                }
                else
                {
                    TempData["ErrorMessage"] = "Could not add new company.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = AggregateModelStateErrorsToHtmlString();
            }

            return RedirectToAction("Dashboard");
        }

        //[HttpPost, ValidateAntiForgeryToken]
        //public async Task<ActionResult> AddNewReleaseVersion(SoftwareReleaseModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var softwareRelease = Mapper.Map<SoftwareReleaseModel, SoftwareRelease>(model);
        //        var request = new InsertSoftwareReleaseRequest(_tokenAuth, softwareRelease);
        //        var response = await _portalService.InsertSoftwareReleaseAsync(request);

        //        if (response.TokenAuth.Status == TokenStatus.Succes)
        //        {
        //            if(response.InsertSoftwareReleaseResult.Status != InsertSoftwareReleaseStatus.Success)
        //            {
        //                TempData["ErrorMessage"] = "Could not add new software release version.";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        TempData["ErrorMessage"] = AggregateModelStateErrorsToHtmlString();
        //    }

        //    return RedirectToAction("Dashboard");
        //}

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> TransferLicenses(TransferLicenseModel model)
        {
            if (ModelState.IsValid)
            {
                var request = new InsertLicenseTransferRequest
                    {
                        TokenAuth = _tokenAuth,
                        amount = model.TransferAmount,
                        from_entity_id = model.FromEntityId,
                        from_pool = (LicensePoolType)model.FromLicensePool,
                        to_pool = (LicensePoolType)model.ToLicensePool,
                        to_entity_id = model.ToEntityId
                    };

                var response = await _portalService.InsertLicenseTransferAsync(request);

                if (response.InsertLicenseTransferResult.Status == InsertLicenseTransferStatus.Success)
                {
                    ModelState.Clear();

                    var fromEntityLink = HtmlHelper.GenerateLink(ControllerContext.RequestContext, RouteTable.Routes, model.FromEntityName, string.Empty,
                        "Dashboard", "CompanyHome", new RouteValueDictionary { { "area", "Company" }, { "entityId", model.FromEntityId } }, null);

                    var toEntityLink = HtmlHelper.GenerateLink(ControllerContext.RequestContext, RouteTable.Routes, model.ToEntityName, string.Empty,
                        "Dashboard", "CompanyHome", new RouteValueDictionary { { "area", "Company" }, { "entityId", model.ToEntityId } }, null);

                    TempData["SuccessMessage"] = string.Format("Successfully transferred {0} licenses from {1} to {2}.", model.TransferAmount, fromEntityLink, toEntityLink);
                }
                else if (response.InsertLicenseTransferResult.Status == InsertLicenseTransferStatus.Access_Denied)
                {
                    TempData["ErrorMessage"] = "You do not have permission to transfer licenses.";
                }
                else
                {
                    Trace.TraceWarning("Transferring {0} {1} licenses from {2}({3}) to {4}({5}) as {6} failed.",
                        new object[]
                            {
                                model.TransferAmount, 
                                model.FromLicensePool, 
                                model.FromEntityName,
                                model.FromEntityId,
                                model.ToEntityName,
                                model.ToEntityId,
                                model.ToLicensePool
                            });
                    Trace.TraceInformation("Token status: {0}.", new object[] { TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status] });
                    Trace.TraceInformation("Response: {0}.", new object[] { Enum.GetName(typeof(InsertLicenseTransferStatus), response.InsertLicenseTransferResult.Status) });

                    TempData["ErrorMessage"] = "Licenses NOT transferred successfully due to a server error.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = AggregateModelStateErrorsToHtmlString();
            }

            return RedirectToAction(string.Empty);
        }

        public ActionResult ActiveCompanies([Bind(Prefix = "Active")]CompanySearchModel model)
        {
            int? currentPage = null;
            int? pageSize = null;
            string searchTerm = null;

            if (model != null)
            {
                currentPage = model.CurrentPage;
                pageSize = model.PageSize;
                searchTerm = model.SearchTerm;
            }

            Debug.Print("Page: " + (currentPage ?? 1));
            var companies = RetrieveCompanies(CompanyStatus.ACTIVE);

            ViewBag.Filtered = !string.IsNullOrEmpty(searchTerm);
            var searchResults = companies = string.IsNullOrEmpty(searchTerm)
                                               ? companies
                                               : companies.Where(c => CompareNames(searchTerm, c.Name));

            Debug.Print("Companies: {0}", companies.Count());

            var pageableData = new CompanySearchModel
            {
                CurrentPage = currentPage ?? 1,
                DataItems = searchResults,
                PageSize = pageSize ?? _pageSize,
                TableBodyPartialView = "_CompanyListPartial",
                SearchTerm = searchTerm
            };

            ViewData.TemplateInfo.HtmlFieldPrefix = "Active";

            return PartialView(pageableData);
        }

        public ActionResult ExpiredCompanies([Bind(Prefix = "Expired")]CompanySearchModel model)
        {
            int? currentPage = null;
            int? pageSize = null;
            string searchTerm = null;

            if (model != null)
            {
                currentPage = model.CurrentPage;
                pageSize = model.PageSize;
                searchTerm = model.SearchTerm;
            }

            Debug.Print("Page: " + (currentPage ?? 1));
            var companies = RetrieveCompanies(CompanyStatus.INACTIVE);

            ViewBag.Filtered = !string.IsNullOrEmpty(searchTerm);
            var searchResults = companies = string.IsNullOrEmpty(searchTerm)
                                               ? companies
                                               : companies.Where(c => CompareNames(searchTerm, c.Name));

            Debug.Print("Companies: {0}", companies.Count());

            var pageableData = new CompanySearchModel
            {
                CurrentPage = currentPage ?? 1,
                DataItems = searchResults,
                PageSize = pageSize ?? _pageSize,
                TableBodyPartialView = "_CompanyListPartial",
                SearchTerm = searchTerm
            };

            ViewData.TemplateInfo.HtmlFieldPrefix = "Expired";

            return PartialView(pageableData);
        }

        #endregion

        #region Helper Methods
        private static bool CompareNames(string searchName, string companyName)
        {
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(companyName, searchName, CompareOptions.IgnoreCase) >= 0;
        }

        private async Task<IEnumerable<CompanyListItemModel>> RetrieveCompanies()
        {
            var encrypticsUser = EncrypticsPrincipal.CurrentEncrypticsUser;
            var getUserCompaniesRequest = new GetUserCompaniesRequest(_tokenAuth, encrypticsUser.EntityId, encrypticsUser.UserId, encrypticsUser.UserId, CompanyStatus.ANY);

            Debug.Print("userId: " + encrypticsUser.UserId);
            Debug.Print("entityId: " + encrypticsUser.EntityId);
            Debug.Print("token: " + _tokenAuth.Token);

            var response = await _portalService.GetUserCompaniesAsync(getUserCompaniesRequest);

            return Mapper.Map<CompanyListItem[], IEnumerable<CompanyListItemModel>>(response.GetUserCompaniesResult).ToList();
        }

        private IEnumerable<CompanyListItemModel> RetrieveCompanies(CompanyStatus status)
        {
            var encrypticsUser = EncrypticsPrincipal.CurrentEncrypticsUser;
            var getUserCompaniesRequest = new GetUserCompaniesRequest(_tokenAuth, encrypticsUser.EntityId, encrypticsUser.UserId, encrypticsUser.UserId, status);

            Debug.Print("userId: " + encrypticsUser.UserId);
            Debug.Print("entityId: " + encrypticsUser.EntityId);
            Debug.Print("token: " + _tokenAuth.Token);

            var response = _portalService.GetUserCompanies(getUserCompaniesRequest);

            return Mapper.Map<CompanyListItem[], IEnumerable<CompanyListItemModel>>(response.GetUserCompaniesResult).ToList();
        }
        
        private async Task<DataTable> GetEncryptsAsync()
        {
            var request = new GetTopCompaniesUsageSummariesByEncryptionsRequest
            {
                TokenAuth = _tokenAuth,
                start_date = DateTime.Today.AddMonths(-6),
                end_date = DateTime.Now,
                top_count = 5
            };

            var response = await _portalService.GetTopCompaniesUsageSummariesByEncryptionsAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetTopCompaniesUsageSummariesByEncryptionsResult != null)
            {
                var companyUsage = Mapper.Map<CompanyUsageCount[], IEnumerable<CompanyEncrypticsUsageModel>>(response.GetTopCompaniesUsageSummariesByEncryptionsResult).ToList();

                return UsageDataTable(companyUsage);
            }

            return null;
        }


        private async Task<DataTable> GetDecryptsAsync()
        {
            var request = new GetTopCompaniesUsageSummariesByDecryptionsRequest
            {
                TokenAuth = _tokenAuth,
                start_date = DateTime.Today.AddMonths(-6),
                end_date = DateTime.Now,
                top_count = 5
            };

            var response = await _portalService.GetTopCompaniesUsageSummariesByDecryptionsAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetTopCompaniesUsageSummariesByDecryptionsResult != null)
            {
                var companyUsage = Mapper.Map<CompanyUsageCount[], IEnumerable<CompanyEncrypticsUsageModel>>(response.GetTopCompaniesUsageSummariesByDecryptionsResult).ToList();
                
                return UsageDataTable(companyUsage);
            }

            return null;
        }

        private static DataTable UsageDataTable(IList<CompanyEncrypticsUsageModel> companyUsage)
        {
            var usageDataTable = new DataTable();
            var companyUsageSummary = from c in companyUsage
                                      group c by c.CompanyName
                                      into g select new {Name = g.First().CompanyName, Usage = g.Sum(x => x.Usage)};
            var companyColumns =
                (from c in companyUsageSummary orderby c.Usage descending select new DataColumn(c.Name, typeof (int))).ToArray();
            var months = (from m in companyUsage select m.MonthUsed).Distinct();

            usageDataTable.Columns.Add("Month", typeof (string));
            usageDataTable.Columns.AddRange(companyColumns);

            foreach (var m in months)
            {
                var m1 = m;
                var x = from c in companyUsage where c.MonthUsed == m1 select new {cn = c.CompanyName, u = c.Usage};
                var r = usageDataTable.NewRow();
                r["Month"] = m.ToString("MMM yyyy");

                foreach (var t in x)
                {
                    r[t.cn] = t.u;
                }

                usageDataTable.Rows.Add(r);
            }

            return usageDataTable;
        }

        private async Task<DataTable> GetExpiringLicensesAsync()
        {
            var request = new GetTopCompaniesByExpirationRequest
            {
                TokenAuth = _tokenAuth,
                top_count = 10
            };

            var response = await _portalService.GetTopCompaniesByExpirationAsync(request);

            var dataTable = new DataTable();

            dataTable.Columns.AddRange(new DataColumn[] { new DataColumn("Company"), new DataColumn("Expires", typeof(DateTime)), new DataColumn("Global", typeof(bool)) });

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetTopCompaniesByExpirationResult.CompanyExpirations != null)
            {
                if (response.GetTopCompaniesByExpirationResult.TotalRecords != response.GetTopCompaniesByExpirationResult.CompanyExpirations.Length) Trace.TraceWarning("TotalRecords field does not match number of recoreds returned.");

                foreach (var r in response.GetTopCompaniesByExpirationResult.CompanyExpirations)
                {
                    dataTable.Rows.Add(r.Name, r.ExpirationDate, r.UseGlobalExpirationDate);
                }
            }

            return dataTable;
        }

        #endregion
    }
}