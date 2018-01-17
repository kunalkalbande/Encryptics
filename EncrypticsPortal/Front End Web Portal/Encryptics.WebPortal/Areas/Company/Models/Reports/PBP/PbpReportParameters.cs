using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Encryptics.WebPortal.Areas.Company.Models;
using Encryptics.WebPortal.PortalService;

namespace WebPortal.Models.Reports
{
    public class PbpReportParameters : ReportParametersModel
    {
        private static readonly IEnumerable<PbpReportDefinition> _pbpReportDefinitions;
        private enum ReportTypes
        {
            Violations = 1,
            TopViolatedRules = 3,
            TopUsersByEncryptions = 4,
            TopUsersByViolations = 5,
            ViolationsRuleTypeOverview = 2
        }

        public PbpReportParameters()
        {
            _reports = _pbpReportDefinitions;
        }

        static PbpReportParameters()
        {
            _pbpReportDefinitions = new List<PbpReportDefinition>
                {
                new PbpReportDefinition
                    {
                        Id = (int) ReportTypes.Violations,
                        Name = "All PBP Violations",
                        HasTopCountParameter = false,
                        //DefaultSortColumn = "CreatedDate",
                        DefaultSortColumn = "Description",
                        ReportType = ReportTypes.Violations.ToString(),
                        AggregateData = true,
                        FillData = (model, proxy, reportParams) =>
                            {
                                var violationsReport = proxy.GetDLPViolations(model.EntityId, null, reportParams);
                                var violations =
                                    Mapper.Map<DLPViolation[], IEnumerable<DlpViolation>>(
                                        violationsReport.Violations).ToArray();
                                model.ReportData = ConvertToDataTable(violations, model.DefaultSortColumn);
                                model.TotalRecords = violationsReport.TotalRecords;
                            }
                    },
                new PbpReportDefinition
                    {
                        Id = (int) ReportTypes.TopViolatedRules,
                        Name = "Top Violated Rules",
                        HasTopCountParameter = true,
                        DefaultSortColumn = "Description",
                        ReportType = ReportTypes.TopViolatedRules.ToString(),
                        FillData = (model, proxy, reportParams) =>
                            {
                                var topViolatedRulesReport = proxy.GetTopViolatedDLPRules(model.EntityId, null,
                                                                                          reportParams);
                                var ruleViolations =
                                    Mapper.Map<DLPRuleViolationCount[], IEnumerable<DlpRuleViolationCount>>(
                                        topViolatedRulesReport.RuleViolations).ToArray();
                                model.ReportData = ConvertToDataTable(ruleViolations, model.DefaultSortColumn);
                                model.TotalRecords = topViolatedRulesReport.TotalRecords;
                            }
                    },
                new PbpReportDefinition
                    {
                        Id = (int) ReportTypes.TopUsersByEncryptions,
                        Name = "Top Users by Encryptions",
                        HasTopCountParameter = true,
                        DefaultSortColumn = "UserName",
                        NumericColumn = "EncryptionCount",
                        ReportType = ReportTypes.TopUsersByEncryptions.ToString(),
                        FillData = (model, proxy, reportParams) =>
                            {
                                var topUsersByEncryptionsReport = proxy.GetTopUsersByEncryptions(model.EntityId,
                                                                                                 reportParams);
                                var encrypts =
                                    Mapper.Map<UserEncryptionCount[], IEnumerable<DlpUserEncryptionCount>>(
                                        topUsersByEncryptionsReport.UserEncryptions).ToArray();

                                model.ReportData = ConvertToDataTable(encrypts, model.DefaultSortColumn);
                                model.TotalRecords = topUsersByEncryptionsReport.TotalRecords;
                            }
                    },
                new PbpReportDefinition
                    {
                        Id = (int) ReportTypes.TopUsersByViolations,
                        Name = "Top Users by Violations",
                        HasTopCountParameter = true,
                        DefaultSortColumn = "UserName",
                        NumericColumn = "ViolationCount",
                        ReportType = ReportTypes.TopUsersByViolations.ToString(),
                        FillData = (model, proxy, reportParams) =>
                            {
                                var topUsersByViolationsReport = proxy.GetTopUsersByViolations(model.EntityId, null,
                                                                                               reportParams);
                                var violationCounts =
                                    Mapper.Map<DLPUserViolationCount[], IEnumerable<DlpUserViolationCount>>(
                                        topUsersByViolationsReport.UserViolations).ToArray();

                                model.ReportData = ConvertToDataTable(violationCounts, model.DefaultSortColumn);
                                model.TotalRecords = topUsersByViolationsReport.TotalRecords;
                            }
                    },
                new PbpReportDefinition
                    {
                        Id = (int) ReportTypes.ViolationsRuleTypeOverview,
                        Name = "Violations Rule Type Overview",
                        HasTopCountParameter = false,
                        DefaultSortColumn = "Title",
                        ShowGraphOnly= true,
                        ReportType = ReportTypes.ViolationsRuleTypeOverview.ToString(),
                        FillData = (model, proxy, reportParams) =>
                            {
                                var ruleTypeOverviewReport = proxy.GetDLPViolationsRuleTypeOverview(model.EntityId, null,
                                                                                               reportParams);
                                var violationCounts =
                                    Mapper.Map<DLPViolationsRuleTypeOverviewItem[], IEnumerable<DlpRuleType>>(
                                        ruleTypeOverviewReport.RuleTypes).ToArray();

                                model.ReportData = ConvertToDataTable(violationCounts, model.DefaultSortColumn);
                                model.TotalRecords = Convert.ToInt64(violationCounts.Sum(x => x.Violations));
                            }
                    }
            };
        }

        public bool AggregateData
        {
            get
            {
                return SelectedReport != null && ((PbpReportDefinition)SelectedReport).AggregateData;
            }
        }

        public string NumericColumn
        {
            get
            {
                return SelectedReport != null ? ((PbpReportDefinition)SelectedReport).NumericColumn : string.Empty ;
            }
        }

        class PbpReportDefinition : ReportDefinition
        {
            public bool AggregateData { get; set; }
            public string NumericColumn { get; set; }
        }
    }
}