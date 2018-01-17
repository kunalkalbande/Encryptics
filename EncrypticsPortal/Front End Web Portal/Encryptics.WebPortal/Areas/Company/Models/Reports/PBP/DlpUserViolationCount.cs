using AutoMapper;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.PortalService;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Encryptics.WebPortal.Areas.Company.Models.Reports.PBP
{
    public class DlpUserViolationCount
    {
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Display(Name = "Violations")]
        public long ViolationCount { get; set; }
    }

    public sealed class DlpUserViolationCountReportDefinition : PBPReportDefinition
    {
        public DlpUserViolationCountReportDefinition()
        {
            Id = GetHashCode();
            Name = "Top Users by Violations";
            HasTopCountParameter = true;
            DefaultSortColumn = "UserName";
            NumericColumn = "ViolationCount";
            ReportType = "TopUsersByViolations";
        }

        public override void FillData(TokenAuth token, ReportParametersModel reportParameters, PortalServiceSoap portalService, ReportParams requestParameters, bool generateChartData)
        {
            var reportRequest = new GetTopUsersByViolationsRequest(token, reportParameters.EntityId, null, requestParameters);
            var topUsersByViolationsReport = portalService.GetTopUsersByViolations(reportRequest);
            var violationCounts = Mapper.Map<DLPUserViolationCount[], IEnumerable<DlpUserViolationCount>>(
                    topUsersByViolationsReport.GetTopUsersByViolationsResult.UserViolations).ToArray();

            reportParameters.ReportData = violationCounts.ToDataTable(reportParameters.DefaultSortColumn);
            reportParameters.TotalRecords = topUsersByViolationsReport.GetTopUsersByViolationsResult.TotalRecords;

            if (!generateChartData) return;

            requestParameters.Page = 1;
            requestParameters.RecordsPerPage = int.MaxValue;

            topUsersByViolationsReport = portalService.GetTopUsersByViolations(reportRequest);
            var chartDataTable = Mapper.Map<DLPUserViolationCount[], IEnumerable<DlpUserViolationCount>>(
                    topUsersByViolationsReport.GetTopUsersByViolationsResult.UserViolations).ToArray().ToDataTable(reportParameters.DefaultSortColumn);

            GenerateJsonChartData(chartDataTable, reportParameters);
        }
    }
}