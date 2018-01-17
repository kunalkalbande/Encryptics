using AutoMapper;
using Encryptics.WebPortal.Areas.Company.Models;
using Encryptics.WebPortal.Areas.Company.Models.Reports.Malware;
using Encryptics.WebPortal.Areas.Company.Models.Reports.PBP;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using PortalServiceCompany = Encryptics.WebPortal.PortalService.Company;
using GlobalResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Areas.Company.Controllers
{
    public class ReportsController : PortalServiceAwareController
    {
        public ReportsController(PortalServiceSoap portalService) : base(portalService) { }

        [HttpGet]
        public async Task<ViewResult> RetrieveReportResults(long entityId)
        {
            var model = await BuildModel(entityId);

            await InitializeViewBagAsync(entityId);

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> RetrieveReportResults(long entityId, ReportParametersModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await BuildReports(model.EntityId, model.Reports);

                    //model.FileTypes = await GetFileTypes();

                    await InitializeViewBagAsync(entityId);

                    try
                    {
                        FillReportData(model, true);
                    }
                    catch(Exception e)
                    {
                        Trace.TraceWarning("Error retrieving data for reports.");
                        Trace.TraceError(string.Format("Exception: {0}", e.Message));
                        Trace.TraceError(string.Format("Exception Stack Trace: {0}", e.StackTrace));

                        if (!ViewData.ContainsKey("ErrorMessage"))
                            ViewData.Add("ErrorMessage", "Could not retrieve report details.");
                        else
                            ViewData["ErrorMessage"] = "Could not retrieve report details.";
                    }
                }
                catch (Exception e)
                {
                    if (!ViewData.ContainsKey("ErrorMessage"))
                        ViewData.Add("ErrorMessage", "Could not retrieve company details.");
                    else
                        ViewData["ErrorMessage"] = "Could not retrieve company details.";
                }
            }

            return View(model);
        }

        [HttpGet]
        public FileStreamResult DownloadCsv(long entityId, ReportParametersModel model)
        {
            if (ModelState.IsValid)
            {
                var fileStream = new MemoryStream();
                var streamWriter = new StreamWriter(fileStream);
                var fileName = string.Format("{0}_{1}.csv", model.ReportType,
                                                DateTime.UtcNow.ToString("yyyymmddhhMMss"));

                model.PageSize = int.MaxValue;
                model.Page = 1;
                FillReportData(model, false);

                AppendCsvLine(model, streamWriter, null, (dataRow, column) => column.ColumnName);
                foreach (DataRow row in model.ReportData.Rows)
                {
                    AppendCsvLine(model, streamWriter, row, (dataRow, column) => string.Format("\"{0}\"", dataRow[column.ColumnName]));
                }

                streamWriter.Flush();
                fileStream.Seek(0, SeekOrigin.Begin);

                return File(fileStream, "text/csv", fileName); 
            }

            return null;
        }

        [HttpGet]
        public ViewResult PrintChartData(long entityId, ReportParametersModel model)
        {
            if (ModelState.IsValid)
            {
                FillReportData(model, true);
            }

            return View("PrintChartData", model);
        }

        private async Task<ReportParametersModel> BuildModel(long entityId)
        {
            var model = new ReportParametersModel
                {
                    EntityId = entityId,
                    //FileTypes = await GetFileTypes()
                };

            await BuildReports(entityId, model.Reports);

            return model;
        }

        private async Task BuildReports(long entityId, IEnumerable<ReportDefinitionModel> reports)
        {
            var companyRequest = new GetCompanyDetailsRequest(_tokenAuth, entityId);
            var response = await _portalService.GetCompanyDetailsAsync(companyRequest);

            if (response.GetCompanyDetailsResult == null || response.TokenAuth.Status != TokenStatus.Succes)
            {
                ModelState.AddModelError(string.Empty, GlobalResources.ErrorMessageCouldNotRetrieveCompanyDetails);
            }

            var companyDetails = Mapper.Map<PortalServiceCompany, CompanyDetailsModel>(response.GetCompanyDetailsResult);
            var reportDefinitionModels = reports as ReportDefinitionModel[] ?? reports.ToArray();

            if (!companyDetails.IsPBPEnabled)
            {
                reportDefinitionModels.Where(rd => rd.GetType().IsSubclassOf(typeof(PBPReportDefinition))).ToArray().Each(rd => rd.IsVisible = false);
            }

            if (!companyDetails.IsZDPEnabled)
            {
                reportDefinitionModels.Where(rd => rd.GetType().IsSubclassOf(typeof(GWReportDefinition))).Each(rd => rd.IsVisible = false);
            }
        }

        //private async Task<IEnumerable<SelectListItem>> GetFileTypes()
        //{
        //    var getMalwareFileTypesRequest = new GetMalwareFileTypesRequest(_tokenAuth);
        //    var results = await _portalService.GetMalwareFileTypesAsync(getMalwareFileTypesRequest);

        //    return results.GetMalwareFileTypesResult.Select(s => new SelectListItem
        //    {
        //        Text = s,
        //        Value = s
        //    }).ToList();
        //}

        private void FillReportData(ReportParametersModel model, bool generateChartData)
        {
            if (string.IsNullOrEmpty(model.OrderBy))
            {
                model.OrderBy = model.SelectedReport.DefaultSortColumn;
            }

            model.StartDate = model.StartDate.ToUTC(_encrypticsUser.UTCOffset, _encrypticsUser.UsesDST);
            model.EndDate = model.EndDate.ToUTC(_encrypticsUser.UTCOffset, _encrypticsUser.UsesDST).AddDays(1);

            var reportParameters = InitializeReportParameters(model);

            model.SelectedReport.FillData(_tokenAuth, model, _portalService, reportParameters, generateChartData);
        }

        private static ReportParams InitializeReportParameters(ReportParametersModel model)
        {
            return new ReportParams
                {
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Page = model.Page,
                    RecordsPerPage = model.PageSize,
                    SortColumn = model.OrderBy ?? model.DefaultSortColumn,
                    TopCount = model.TopCount ?? (model.ShowTopCount ? 5 : (int?)null),
                    OrderDirection = string.IsNullOrEmpty(model.OrderByDir)
                                         ? OrderByDirection.ASC
                                         : (OrderByDirection)Enum.Parse(typeof(OrderByDirection), model.OrderByDir)
                };
        }

        private static void AppendCsvLine(ReportParametersModel model, TextWriter writer, DataRow row,
                                          Func<DataRow, DataColumn, object> data)
        {
            var columnCounter = 0;
            var dataLine = new StringBuilder();

            foreach (DataColumn c in model.ReportData.Columns)
            {
                dataLine.Append(data(row, c));
                if (columnCounter < model.ReportData.Columns.Count)
                {
                    dataLine.Append(',');
                }
                columnCounter++;
            }

            writer.WriteLine(dataLine);
        }
    }
}