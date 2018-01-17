using AutoMapper;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.PortalService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Encryptics.WebPortal.Areas.Company.Models.Reports.PBP
{
    public class DlpRuleType
    {
        //public long Id { get; set; }

        public string Title { get; set; }

        //public string Description { get; set; }

        public double Violations { get; set; }
    }

    public sealed class DlpRuleTypeReportDefinition : PBPReportDefinition
    {
        public DlpRuleTypeReportDefinition()
        {
            Id = GetHashCode();
            Name = "Violations Rule Type Overview";
            HasTopCountParameter = false;
            DefaultSortColumn = "Title";
            ShowGraphOnly = true;
            NumericColumn = "Violations";
            ReportType = "ViolationsRuleTypeOverview";
        }

        public override void FillData(TokenAuth token, ReportParametersModel reportParameters, PortalServiceSoap portalService, ReportParams requestParameters, bool generateChartData)
        {
            var reportRequest = new GetDLPViolationsRuleTypeOverviewRequest(token, reportParameters.EntityId, null, requestParameters);
            var ruleTypeOverviewReport = portalService.GetDLPViolationsRuleTypeOverview(reportRequest);
            var violationCounts = Mapper.Map<DLPViolationsRuleTypeOverviewItem[], IEnumerable<DlpRuleType>>(
                    ruleTypeOverviewReport.GetDLPViolationsRuleTypeOverviewResult.RuleTypes).ToArray();
            reportParameters.ReportData = violationCounts.ToDataTable(reportParameters.DefaultSortColumn);
            reportParameters.TotalRecords = Convert.ToInt64(violationCounts.Sum(x => x.Violations));

            if (!generateChartData) return;

            requestParameters.Page = 1;
            requestParameters.RecordsPerPage = int.MaxValue;

            ruleTypeOverviewReport = portalService.GetDLPViolationsRuleTypeOverview(reportRequest);
            var chartDataTable = Mapper.Map<DLPViolationsRuleTypeOverviewItem[], IEnumerable<DlpRuleType>>(
                    ruleTypeOverviewReport.GetDLPViolationsRuleTypeOverviewResult.RuleTypes).ToArray().ToDataTable(reportParameters.DefaultSortColumn);

            GenerateJsonChartData(chartDataTable, reportParameters);
        }
    }
}