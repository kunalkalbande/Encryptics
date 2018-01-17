using System.Threading;
using AutoMapper;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.IdentityModel;
using Encryptics.WebPortal.PortalService;
using Google.DataTable.Net.Wrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using DataTable = System.Data.DataTable;

namespace Encryptics.WebPortal.Areas.Company.Models.Reports.PBP
{
    [DefaultSortColumn("CreatedDate")]
    public class DlpViolation
    {
        [DisplayColumnWidth("10%")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [DisplayColumnWidth("10%")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [DisplayColumnWidth("10%")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [DisplayColumnWidth("15%")]
        [Display(Name = "Date Created")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime CreatedDate { get; set; }
    }

    public sealed class DlpReportDefinition : PBPReportDefinition
    {
        public DlpReportDefinition()
        {
            Id = GetHashCode();
            Name = "All PBP Violations";
            HasTopCountParameter = false;
            DefaultSortColumn = "Description";
            ReportType = "Violations";
            AggregateData = true;
        }

        public override void FillData(TokenAuth token, ReportParametersModel reportParameters, PortalServiceSoap portalService, ReportParams requestParameters, bool generateChartData)
        {
            var reportRequest = new GetDLPViolationsRequest(token, reportParameters.EntityId, null, requestParameters);
            var violationsReport = portalService.GetDLPViolations(reportRequest);
            var violations = Mapper.Map<DLPViolation[], IEnumerable<DlpViolation>>(
                    violationsReport.GetDLPViolationsResult.Violations).ToArray();
            var encrpticsUser = Thread.CurrentPrincipal as EncrypticsPrincipal;

            if (encrpticsUser != null)
            {
                foreach (var violation in violations)
                {
                    violation.CreatedDate = violation.CreatedDate.FromUTC(encrpticsUser.UTCOffset, encrpticsUser.UsesDST);
                }
            }

            reportParameters.ReportData = violations.ToDataTable(reportParameters.DefaultSortColumn);
            reportParameters.TotalRecords = violationsReport.GetDLPViolationsResult.TotalRecords;

            if (!generateChartData) return;

            requestParameters.Page = 1;
            requestParameters.RecordsPerPage = int.MaxValue;

            violationsReport = portalService.GetDLPViolations(reportRequest);
            var chartDataTable = Mapper.Map<DLPViolation[], IEnumerable<DlpViolation>>(
                    violationsReport.GetDLPViolationsResult.Violations).ToArray().ToDataTable(reportParameters.DefaultSortColumn);

            GenerateJsonChartData(chartDataTable, reportParameters);
        }

        public override void GenerateJsonChartData(DataTable chartData, ReportParametersModel reportParameters)
        {
            string orderByColumnName = string.IsNullOrEmpty(reportParameters.OrderBy) ? reportParameters.DefaultSortColumn : reportParameters.OrderBy;

            var rows =
                (from row in chartData.AsEnumerable()
                 let pivotColumn =
                     chartData.Columns[orderByColumnName].ExtendedProperties.Contains("Format")
                         ? string.Format(
                             chartData.Columns[orderByColumnName].ExtendedProperties["Format"].ToString
                                 (),
                             row[orderByColumnName])
                         : row[orderByColumnName].ToString()
                 group row by pivotColumn
                     into g
                     select new { PivotColumn = g.Key.Replace("\"", @"\\"""), Violations = g.Count() }).ToArray();

            var newTable = rows.ToDataTable(string.Empty);

            reportParameters.ChartDataJson = SystemDataTableConverter.Convert(newTable).GetJson();
        }
    }
}
