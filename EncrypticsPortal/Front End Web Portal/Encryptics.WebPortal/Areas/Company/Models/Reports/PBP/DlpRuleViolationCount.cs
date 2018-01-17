using AutoMapper;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.PortalService;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Encryptics.WebPortal.Areas.Company.Models.Reports.PBP
{
    public class DlpRuleViolationCount
    {
        public string Description { get; set; }

        [Display(Name = "Violations")]
        public long ViolationCount { get; set; }
    }

    public sealed class DlpRuleViolationCountDefinition : PBPReportDefinition
    {
        public DlpRuleViolationCountDefinition()
        {
            Id = GetHashCode();
            Name = "Top Violated Rules";
            HasTopCountParameter = true;
            DefaultSortColumn = "Description";
            ReportType = "TopViolatedRules";
            NumericColumn = "ViolationCount";
        }

        public override void FillData(TokenAuth token, ReportParametersModel reportParameters, PortalServiceSoap portalService, ReportParams requestParameters, bool generateChartData)
        {
            var reportRequest = new GetTopViolatedDLPRulesRequest(token, reportParameters.EntityId, null, requestParameters);
            var topViolatedRulesReport = portalService.GetTopViolatedDLPRules(reportRequest);
            var ruleViolations = Mapper.Map<DLPRuleViolationCount[], IEnumerable<DlpRuleViolationCount>>(
                    topViolatedRulesReport.GetTopViolatedDLPRulesResult.RuleViolations).ToArray();
            reportParameters.ReportData = ruleViolations.ToDataTable(reportParameters.DefaultSortColumn);
            reportParameters.TotalRecords = topViolatedRulesReport.GetTopViolatedDLPRulesResult.TotalRecords;

            if (!generateChartData) return;

            requestParameters.Page = 1;
            requestParameters.RecordsPerPage = int.MaxValue;

            topViolatedRulesReport = portalService.GetTopViolatedDLPRules(reportRequest);
            var chartDataTable = Mapper.Map<DLPRuleViolationCount[], IEnumerable<DlpRuleViolationCount>>(
                    topViolatedRulesReport.GetTopViolatedDLPRulesResult.RuleViolations).ToArray().ToDataTable(reportParameters.DefaultSortColumn);

            GenerateJsonChartData(chartDataTable, reportParameters);
        }
    }
}