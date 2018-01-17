using Encryptics.WebPortal.PortalService;
using Google.DataTable.Net.Wrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using DataTable = System.Data.DataTable;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public abstract class ReportDefinitionModel
    {
        public int Id { get; protected set; }
        public string Name { get; protected set; }
        public bool HasTopCountParameter { get; protected set; }
        public string DefaultSortColumn { get; protected set; }
        public Type ObjectType { get; protected set; }
        public string ReportType { get; protected set; }
        public bool ShowGraphOnly { get; protected set; }
        public bool AggregateData { get; protected set; }
        public string NumericColumn { get; protected set; }
        public bool ShowFileTypes { get; protected set; }
        public bool IsVisible { get; set; }
        public bool IsGraphAvailable { get; set; }

        protected ReportDefinitionModel()
        {
            IsVisible = false;
            IsGraphAvailable = true;
        }

        public virtual void FillData(TokenAuth token, ReportParametersModel reportParameters, PortalServiceSoap portalService, ReportParams requestParameters, bool generateChartData)
        {
        }

        public virtual void GenerateJsonChartData(DataTable chartData, ReportParametersModel reportParameters)
        {
            reportParameters.ChartDataJson = SystemDataTableConverter.Convert(chartData).GetJson();
        }

        protected void RemoveColumns(DataTable chartDataTable)
        {
            string[] columnsToRemove = (from dc in chartDataTable.Columns.Cast<DataColumn>()
                                  where dc.ColumnName != DefaultSortColumn && dc.ColumnName != NumericColumn
                                  select dc.ColumnName).ToArray();

            foreach (var columnName in columnsToRemove)
            {
                chartDataTable.Columns.Remove(columnName);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DisplayColumnWidthAttribute : Attribute
    {
        public string ColumnWidth { get; private set; }

        public DisplayColumnWidthAttribute(string displayWidth)
        {
            ColumnWidth = displayWidth;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultSortColumnAttribute : Attribute
    {
        public string ColumnName { get; set; }

        public DefaultSortColumnAttribute(string defaultColumnName)
        {
            ColumnName = defaultColumnName;
        }
    }

    public class CustomModelMetadataProvider : DataAnnotationsModelMetadataProvider
    {
        protected override ModelMetadata CreateMetadata(
            IEnumerable<Attribute> attributes,
            Type containerType,
            Func<object> modelAccessor,
            Type modelType,
            string propertyName)
        {
            var enumerable = attributes as Attribute[] ?? attributes.ToArray();

            var data = base.CreateMetadata(
                enumerable,
                containerType,
                modelAccessor,
                modelType,
                propertyName);

            var tooltip = enumerable
                .SingleOrDefault(a => typeof(DisplayColumnWidthAttribute) == a.GetType());

            if (tooltip != null)
                data.AdditionalValues
                    .Add("DisplayColumnWidth", ((DisplayColumnWidthAttribute)tooltip).ColumnWidth);

            return data;
        }
    }
}
