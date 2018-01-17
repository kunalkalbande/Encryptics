using Google.DataTable.Net.Wrapper;
using DataTable = System.Data.DataTable;

namespace Encryptics.WebPortal.Areas.Company.Models.Reports.PBP
{
    public class PBPReportDefinition : ReportDefinitionModel
    {
        public override void GenerateJsonChartData(DataTable chartData, ReportParametersModel reportParameters)
        {
            RemoveColumns(chartData);

            reportParameters.ChartDataJson = SystemDataTableConverter.Convert(chartData).GetJson();
        }
    }
}