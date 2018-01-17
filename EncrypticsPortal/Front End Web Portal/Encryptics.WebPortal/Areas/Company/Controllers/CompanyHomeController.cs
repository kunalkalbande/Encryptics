using AutoMapper;
using Encryptics.WebPortal.ActionResults;
using Encryptics.WebPortal.Areas.Company.Models;
using Encryptics.WebPortal.Areas.Company.Models.Reports.PBP;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.Filters;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using Google.DataTable.Net.Wrapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using LicensePoolType = Encryptics.WebPortal.PortalService.LicensePoolType;
using PortalServiceCompany = Encryptics.WebPortal.PortalService.Company;
using PortalServiceCompanyDomain = Encryptics.WebPortal.PortalService.CompanyDomain;
using UserEncryptionCount = Encryptics.WebPortal.PortalService.UserEncryptionCount;

namespace Encryptics.WebPortal.Areas.Company.Controllers
{
    public class CompanyHomeController : PortalServiceAwareController
    {
        private long _entityId;
        private CompanySummaryModel _companyDetails;
        private GetEntityUsageSummariesRequest _usageSummariesRequest;

        private static readonly string[] _dashboardPermissions = new[] { 
            "CompanyAdmin/CompanyAdminHome/Dashboard",
            "Company/CompanyHome/EditCompanyDetails",
            "Company/CompanyHome/AddLicenses",
            "Company/CompanyDomain/Index",
            "UserAdmin/UserAdminHome/ManageAccounts",
            "UserAdmin/UserAdminHome/AddNewUser",
            "UserAdmin/UploadAccounts/Index",
            "Company/Reports/RetrieveReportResults",
            "Company/CompanyHome/EditCompanySettings",
            "Company/PolicyBasedProtection/ConfigurePolicies",
            "Company/PolicyBasedProtection/AssignRules",
            "Company/ZeroDayProtection/ConfigureSettings",
            "Company/CompanyProductVersion/Index",
            "Company/DistributionGroup/Index",
            "Company/UsageRightsGroup/Index"
        };

        private static readonly string[] _settingsPermssions = new[]
            {
                "Company/CompanyHome/AjaxUpdateEmailTemplate", 
                "Company/CompanyHome/AjaxGetEmailTemplate"
            };

        public CompanyHomeController(PortalServiceSoap portalService) : base(portalService) { }

        [HttpGet]
        public async Task<ActionResult> Dashboard(long entityId, int? month, int? year)
        {
            ViewData["CompanyId"] = _entityId = entityId;

            ViewBag.Month = month;
            ViewBag.Year = year;

            await BuildCompanySummaryModel(month, year);

            ViewBag.ShowActiveTab = "ActiveAccounts";

            if (TempData.ContainsKey("PendingAcctsParams") || TempData.ContainsKey("ShowPendingAccounts"))
                ViewBag.ShowActiveTab = "PendingAccounts";

            HandleMessages();

            await SetViewPermissionsAsync(_entityId, _dashboardPermissions);

            return View(_companyDetails);
        }

        [HttpGet]
        public async Task<JsonResult> AjaxGetDailyUsageSummary(long entityId, int month, int? year)
        {
            var startDate = new DateTime(year ?? DateTime.Today.Year, month + 1, 1).ToUTC(_encrypticsUser.UTCOffset, _encrypticsUser.UsesDST);
            var endDate = startDate.AddMonths(1);

            _entityId = entityId;

            var usageSummary = await GetUsageSummary(startDate, endDate, SummaryType.Daily);

            return new JsonResult
            {
                Data = SystemDataTableConverter.Convert(usageSummary.ToDataTable()).GetJson(),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        public async Task<JsonResult> AjaxGetMonthlyUsageSummary(long entityId)
        {
            var startDate = GetUTCOffsetToday().AddMonths(-8);
            var endDate = GetUTCOffsetToday();

            _entityId = entityId;

            var usageSummary = await GetUsageSummary(startDate, endDate, SummaryType.Monthly);

            return new JsonResult
            {
                Data = SystemDataTableConverter.Convert(usageSummary.ToDataTable()).GetJson(),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        public JsonpResult AjaxGetEmailTemplate()
        {
            return GetJsonpAntiforgeryToken();
        }

        [HttpGet]
        public async Task<ActionResult> AjaxGetEmailTemplate(long entityId, int templateType)
        {
            var request = new GetCompanyEmailTemplateRequest(_tokenAuth, entityId, templateType);

            var response = await _portalService.GetCompanyEmailTemplateAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes)
            {
                if (response.GetCompanyEmailTemplateResult != null)
                {
                    var model =
                        Mapper.Map<CompanyEmailTemplate, EmailTemplateModel>(response.GetCompanyEmailTemplateResult);

                    return PartialView("_EmailTemplatePartial", model);
                }
                
                return PartialView("_EmailTemplatePartial");
            }

            return new JsonResult
            {
                Data = new { success = false, errors = new[] { "Could not retrieve email template." } },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost, ValidateHttpAntiForgeryToken]
        public async Task<JsonResult> AjaxUpdateEmailTemplate(long entityId, EmailTemplateModel model)
        {
            var companyEmailTemplate = Mapper.Map<EmailTemplateModel, CompanyEmailTemplate>(model);

            var request = new UpdateCompanyEmailTemplateRequest(_tokenAuth, entityId, companyEmailTemplate);

            var response = await _portalService.UpdateCompanyEmailTemplateAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes && response.UpdateCompanyEmailTemplateResult)
            {
                return new JsonResult
                {
                    Data = new { success = true },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            return new JsonResult
            {
                Data = new { success = false, errors = new[] { "Could not update email template." } },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        public async Task<ActionResult> EditCompanyDetails(long entityId)
        {
            var response = await _portalService.GetCompanyDetailsAsync(new GetCompanyDetailsRequest(_tokenAuth, entityId));

            if (response.TokenAuth.Status == TokenStatus.Succes)
            {
                var companyDetails = Mapper.Map<PortalServiceCompany, CompanyDetailsModel>(response.GetCompanyDetailsResult);

                InitializeViewBag(entityId, companyDetails.Name);

                return View(companyDetails);
            }

            TempData["ErrorMessage"] = "Could not retrieve details for entity.";

            return RedirectToAction("Dashboard", new { entityId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCompanyDetails(CompanyDetailsModel model)
        {
            var updatedCompany = Mapper.Map<CompanyDetailsModel, PortalServiceCompany>(model);
            var utcHelper = new UTCHelper(_encrypticsUser.UTCOffset, _encrypticsUser.UsesDST);

            if (model.GlobalExpirationDate.HasValue)
            {
                model.GlobalExpirationDate = utcHelper.GetUTCDateTime(model.GlobalExpirationDate.Value);
            }

            var response = await _portalService.UpdateCompanyAsync(new UpdateCompanyRequest(_tokenAuth, updatedCompany));

            if (response.TokenAuth.Status != TokenStatus.Succes)
            {
                TempData["ErrorMessage"] = "Could not update entity.";
            }

            return RedirectToAction("Dashboard", new { entityId = model.Id });
        }

        [HttpGet]
        public async Task<ActionResult> EditCompanySettings(long entityId)
        {
            var response = await _portalService.GetCompanyDetailsAsync(new GetCompanyDetailsRequest(_tokenAuth, entityId));

            if (response.TokenAuth.Status == TokenStatus.Succes)
            {
                var companyDetails = Mapper.Map<PortalServiceCompany, CompanyDetailsModel>(response.GetCompanyDetailsResult);

                InitializeViewBag(entityId, companyDetails.Name);

                await SetViewPermissionsAsync(entityId, _settingsPermssions);

                return View(companyDetails);
            }

            TempData["ErrorMessage"] = "Could not retrieve details for entity.";

            return RedirectToAction("Dashboard", new { entityId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCompanySettings(CompanyDetailsModel model)
        {
            var updatedCompany = Mapper.Map<CompanyDetailsModel, PortalServiceCompany>(model);
            var utcHelper = new UTCHelper(_encrypticsUser.UTCOffset, _encrypticsUser.UsesDST);

            if (model.GlobalExpirationDate.HasValue)
            {
                model.GlobalExpirationDate = utcHelper.GetUTCDateTime(model.GlobalExpirationDate.Value);
            }

            var response = await _portalService.UpdateCompanyAsync(new UpdateCompanyRequest(_tokenAuth, updatedCompany));

            if (response.TokenAuth.Status != TokenStatus.Succes)
            {
                TempData["ErrorMessage"] = "Could not update entity.";
            }

            return RedirectToAction("Dashboard", new { entityId = model.Id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> AddLicenses(long entityId, int? month, int? year, int transferAmount)
        {
            long fromEntityId = _encrypticsUser.EntityId;

            var response = await _portalService.InsertLicenseTransferAsync(new InsertLicenseTransferRequest
            {
                TokenAuth = _tokenAuth,
                amount = transferAmount,
                from_entity_id = fromEntityId,
                from_pool = LicensePoolType.AvailablePool,
                to_entity_id = entityId,
                to_pool = LicensePoolType.ActivePool
            });

            if (response.InsertLicenseTransferResult.Status != InsertLicenseTransferStatus.Success || response.TokenAuth.Status != TokenStatus.Succes)
            {
                Trace.TraceError(string.Format("InsertLicenseTransfer failed: {0}", response.InsertLicenseTransferResult.Status));
                if (response.TokenAuth.Status != TokenStatus.Succes)
                {
                    Trace.TraceWarning(string.Format("Token status: {0}", response.TokenAuth.Status));
                    Trace.WriteIf(string.IsNullOrEmpty(response.TokenAuth.Token), "Token is unknown.");
                }

                TempData["ErrorMessage"] = "Could not transfer licenses.";
            }

            return RedirectToAction("Dashboard", new { entityId, month, year });
        }

        [HttpGet]
        public async Task<ActionResult> AjaxGetUpdatedActiveCount(long entityId)
        {
            await GetCompanyDetails(entityId);

            var returnData = new
                {
                    success = true,
                    count = _companyDetails.ActiveUserAccountTotal
                };

            return Json(returnData, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> AjaxGetUpdatedPendingCount(long entityId)
        {
            await GetCompanyDetails(entityId);

            var returnData = new
                {
                    success = true,
                    count = _companyDetails.PendingUserAccountTotal
                };

            return Json(returnData, JsonRequestBehavior.AllowGet);
        }

        private async Task BuildCompanySummaryModel(int? month, int? year)
        {
            await GetCompanyDetails();
            await GetCompanyDomains();
            await GetPBPViolations();
            await GetPBPExceptions();
            await GetZDPViolations();
            await GetTopUsersReportData();

            if (month != null)
                await GetDailyUsageSummary((int)month, year ?? DateTime.Today.Year);
            else
                await GetMonthlyUsageSummary();
        }

        private async Task GetMonthlyUsageSummary()
        {
            if (_companyDetails == null) return;

            var today = GetUTCOffsetToday();
            var startDate = today.AddMonths(-8);
            var endDate = today.AddDays(1);
            var usageSummary = await GetUsageSummary(startDate, endDate, SummaryType.Monthly);

            ViewBag.UsageSummary = "Monthly";

            _companyDetails.EncrypticsUsage = usageSummary.ToDataTable();
        }

        private async Task GetDailyUsageSummary(int month, int year)
        {
            if (_companyDetails == null) return;

            var startDate = new DateTime(year, month + 1, 1).ToUTC(_encrypticsUser.UTCOffset, _encrypticsUser.UsesDST);
            var endDate = startDate.AddMonths(1);

            ViewBag.UsageSummary = "Daily";

            var usageSummary = await GetUsageSummary(startDate, endDate, SummaryType.Daily);

            _companyDetails.EncrypticsUsage = usageSummary.ToDataTable();
        }

        private async Task<ICollection<CompanyUsageSummaryModel>> GetUsageSummary(DateTime startDate, DateTime endDate, SummaryType summaryType)
        {
            _usageSummariesRequest = new GetEntityUsageSummariesRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = _entityId,
                    start_date = startDate,
                    end_date = endDate,
                    sumType = summaryType
                };

            var response = await _portalService.GetEntityUsageSummariesAsync(_usageSummariesRequest);

            if (response.TokenAuth.Status == TokenStatus.Succes)
            {
                return Mapper.Map<UsageSummary[], ICollection<CompanyUsageSummaryModel>>(
                        response.GetEntityUsageSummariesResult);
            }

            Trace.TraceError("Token error: {0}",
                             (object)TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]);

            ModelState.AddModelError(string.Empty, "Could not retrieve Encryptics usage summary.");

            return new Collection<CompanyUsageSummaryModel>();
        }

        private async Task GetCompanyDetails(long entityId)
        {
            _entityId = entityId;

            await GetCompanyDetails();
        }

        private async Task GetCompanyDetails()
        {
            var response =
                await _portalService.GetCompanyDetailsAsync(new GetCompanyDetailsRequest
                    {
                        entity_id = _entityId,
                        TokenAuth = _tokenAuth
                    });

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetCompanyDetailsResult != null)
            {
                _companyDetails = Mapper.Map<PortalServiceCompany, CompanySummaryModel>(response.GetCompanyDetailsResult);
            }
            else
            {
                Trace.TraceError("Token error: {0}",
                                 (object)TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]);

                ModelState.AddModelError(string.Empty, "Could not retrieve entity details.");
            }
        }

        private async Task GetPBPViolations()
        {
            if (_companyDetails == null) return;

            DateTime endDate;
            DateTime startDate;
            GetWeekTimeSpan(out endDate, out startDate);
            var reportParams = new ReportParams
            {
                EndDate = endDate,
                StartDate = startDate,
                RecordsPerPage = int.MaxValue
            };
            var reportRequest = new GetTopViolatedDLPRulesRequest(_tokenAuth, _entityId, null, reportParams);
            var topViolatedRulesReport = await _portalService.GetTopViolatedDLPRulesAsync(reportRequest);
            var ruleViolations = Mapper.Map<DLPRuleViolationCount[], IEnumerable<DlpRuleViolationCount>>(
                    topViolatedRulesReport.GetTopViolatedDLPRulesResult.RuleViolations).ToArray();

            _companyDetails.PBPViolations = ruleViolations.Sum(x => x.ViolationCount);
        }

        private void GetWeekTimeSpan(out DateTime endDate, out DateTime startDate)
        {
            var today = GetUTCOffsetToday();
            endDate = today.Date.AddDays(1);
            startDate = today.Date.AddDays(-6);
        }

        private async Task GetPBPExceptions()
        {
            if (_companyDetails == null) return;

            DateTime endDate;
            DateTime startDate;
            GetWeekTimeSpan(out endDate, out startDate);
            var reportParams = new ReportParams
            {
                EndDate = startDate,
                StartDate = endDate,
                RecordsPerPage = int.MaxValue
            };

            var request = new GetDLPJustificationsRequest
            {
                TokenAuth = _tokenAuth,
                report_params = reportParams,
                entity_id = _entityId
            };

            var response = await _portalService.GetDLPJustificationsAsync(request);

            _companyDetails.PBPExceptions = response.GetDLPJustificationsResult.TotalRecords;
        }

        private async Task GetZDPViolations()
        {
            if (_companyDetails == null) return;

            DateTime endDate;
            DateTime startDate;
            GetWeekTimeSpan(out endDate, out startDate);
            var reportParams = new ReportParams
            {
                EndDate = endDate,
                StartDate = startDate,
                RecordsPerPage = int.MaxValue
            };

            var reportRequest = new GetMalwareFilesRequest(_tokenAuth, _entityId, null, reportParams);
            var filesReport = await _portalService.GetMalwareFilesAsync(reportRequest);

            _companyDetails.ZDPViolations = filesReport.GetMalwareFilesResult.TotalFiles;
            _companyDetails.ZDPViolatingUsers = filesReport.GetMalwareFilesResult.Files.GroupBy(f => f.UserName).Count();
        }

        private async Task GetCompanyDomains()
        {
            if (_companyDetails == null) return;

            var response = await _portalService.GetCompanyDomainsAsync(new GetCompanyDomainsRequest(_tokenAuth, _entityId));

            if (response.TokenAuth.Status == TokenStatus.Succes)
            {
                _companyDetails.Domains =
                    Mapper.Map<PortalServiceCompanyDomain[], IEnumerable<CompanyDomainModel>>(
                        response.GetCompanyDomainsResult);
            }
            else
            {
                Trace.TraceError("Token error: {0}",
                                 (object)TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]);

                ModelState.AddModelError(string.Empty, "Could not retrieve entity domain assignments.");
            }
        }

        private async Task GetTopUsersReportData()
        {
            if (_companyDetails == null) return;

            var startDate = _companyDetails.CreatedDate;

            if (_companyDetails.CreatedDate.HasValue)
            {
                startDate = new DateTimeOffset(_companyDetails.CreatedDate.Value, TimeSpan.Zero).UtcDateTime;
            }

            var today = GetUTCOffsetToday();
            var endDate = today.Date.AddDays(1);

            var getTopUsersByEncryptionsRequest = new GetTopUsersByEncryptionsRequest(_tokenAuth, _entityId, new ReportParams
            {
                StartDate = startDate,
                EndDate = endDate,
                OrderDirection = OrderByDirection.DESC,
                Page = 1,
                RecordsPerPage = 5,
                TopCount = 5,
                SortColumn = "EncryptionCount"
            });

            var response = await _portalService.GetTopUsersByEncryptionsAsync(getTopUsersByEncryptionsRequest);

            _companyDetails.TopFiveUsers =
                Mapper.Map<UserEncryptionCount[], ICollection<UserEncryptionModel>>(
                    response.GetTopUsersByEncryptionsResult.UserEncryptions).ToDataTable();

            _companyDetails.TopFiveUsers.Columns.Remove("FirstName");
            _companyDetails.TopFiveUsers.Columns.Remove("LastName");
            _companyDetails.TopFiveUsers.Columns.Remove("UserId");
        }

        [HttpGet/*, AuthorizeAction(true)*/]
        public async Task<ActionResult> AjaxGetUpdatedSuspendedCount(long entityid)
        {
            await GetCompanyDetails(entityid);

            var returnData = new
            {
                success = true,
                count = _companyDetails.SuspendedUserAccountTotal
            };

            return Json(returnData, JsonRequestBehavior.AllowGet);
        }
    }
}