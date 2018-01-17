using AutoMapper;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.PortalService;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Encryptics.WebPortal.Areas.Company.Models.Reports
{
    public class UserEncryptionCount
    {
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Display(Name = "Encrypts")]
        public long EncryptionCount { get; set; }
    }

    public sealed class DlpUserEncryptionCountReportDefinition : ReportDefinitionModel
    {
        public DlpUserEncryptionCountReportDefinition()
        {
            Id = GetHashCode();
            Name = "Top Users by Encryptions";
            HasTopCountParameter = true;
            DefaultSortColumn = "UserName";
            NumericColumn = "EncryptionCount";
            ReportType = "TopUsersByEncryptions";
        }

        public override void FillData(TokenAuth token, ReportParametersModel reportParameters, PortalServiceSoap portalService, ReportParams requestParameters, bool generateChartData)
        {
            var reportRequest = new GetTopUsersByEncryptionsRequest(token, reportParameters.EntityId, requestParameters);
            var topUsersByEncryptionsReport = portalService.GetTopUsersByEncryptions(reportRequest);
            var encrypts = Mapper.Map<PortalService.UserEncryptionCount[], IEnumerable<UserEncryptionCount>>(
                    topUsersByEncryptionsReport.GetTopUsersByEncryptionsResult.UserEncryptions).ToArray();

            reportParameters.ReportData = encrypts.ToDataTable(reportParameters.DefaultSortColumn);
            reportParameters.TotalRecords = topUsersByEncryptionsReport.GetTopUsersByEncryptionsResult.TotalRecords;

            if (!generateChartData) return;

            requestParameters.Page = 1;
            requestParameters.RecordsPerPage = int.MaxValue;

            topUsersByEncryptionsReport = portalService.GetTopUsersByEncryptions(reportRequest);

            var chartDataTable = Mapper.Map<PortalService.UserEncryptionCount[], IEnumerable<UserEncryptionCount>>(
                    topUsersByEncryptionsReport.GetTopUsersByEncryptionsResult.UserEncryptions).ToArray().ToDataTable(reportParameters.DefaultSortColumn);

            RemoveColumns(chartDataTable);

            GenerateJsonChartData(chartDataTable, reportParameters);
        }
    }
}