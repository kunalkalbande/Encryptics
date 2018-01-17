using AutoMapper;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using System.Collections.Generic;
using System.Linq;

namespace Encryptics.WebPortal.Areas.Company.Models.Reports.PBP
{
    public class PBPJustification : DlpViolation
    {
        public string Justification { get; set; }
    }

    public sealed class PBPUnencryptedJustification : PBPReportDefinition
    {
        public PBPUnencryptedJustification()
        {
            Id = GetHashCode();
            Name = "Sent Unencrypted Justification";
            HasTopCountParameter = false;
            DefaultSortColumn = "";
            ShowGraphOnly = false;
            NumericColumn = "";
            IsGraphAvailable = false;
        }

        public override void FillData(TokenAuth token, ReportParametersModel reportParameters, PortalServiceSoap portalService, ReportParams requestParameters, bool generateChartData)
        {
            var request = new GetDLPJustificationsRequest
                {
                    TokenAuth=token,
                    report_params = requestParameters,
                    entity_id=reportParameters.EntityId
                };

            var response = portalService.GetDLPJustifications(request);

            if (response.TokenAuth.Status != TokenStatus.Succes || response.GetDLPJustificationsResult == null) return;

            var reportData = Mapper.Map<DLPViolation[], IEnumerable<PBPJustification>> (response.GetDLPJustificationsResult.Violations).ToArray();

            reportParameters.ReportData = reportData.ToDataTable();
            reportParameters.TotalRecords = response.GetDLPJustificationsResult.TotalRecords;
        }
    }
}