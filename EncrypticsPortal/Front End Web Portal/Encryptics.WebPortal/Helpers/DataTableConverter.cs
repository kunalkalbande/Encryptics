using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using Encryptics.WebPortal.Areas.Company.Models;

namespace Encryptics.WebPortal.Helpers
{
    public static class DataTableConverter
    {
        public static DataTable ToDataTable<T>(this ICollection<T> objectCollection, string defaultColumnName = null)
        {
            var properties = typeof(T).GetProperties();

            var newDataTable = new DataTable();
            if (defaultColumnName != null) newDataTable.ExtendedProperties.Add("DefaultSortColumn", defaultColumnName);

            var columns = from prop in properties
                          select AddDataColumn(prop);

            newDataTable.Columns.AddRange(columns.ToArray());

            if (objectCollection.Count != 0)
            {
                foreach (object o in objectCollection)
                    FillData(properties, newDataTable, o);
            }

            return newDataTable;
        }

        private static DataColumn AddDataColumn(PropertyInfo prop)
        {
            var newColumn = new DataColumn
                {
                    ColumnName = prop.Name,
                    Caption = GetColumnName(prop),
                    DataType = prop.PropertyType
                };

            AddColumnProperty<DisplayColumnWidthAttribute>(prop, newColumn, "ColumnWidth",
                                                               "Width");

            AddColumnProperty<DisplayFormatAttribute>(prop, newColumn, "DataFormatString",
                                                      "Format");

            return newColumn;
        }

        private static string GetColumnName(PropertyInfo pi)
        {
            return pi.GetCustomAttributes(typeof(DisplayAttribute), false).Any()
                       ? pi.GetCustomAttributes(typeof(DisplayAttribute), false).Cast<DisplayAttribute>().Single().Name
                       : pi.Name;
        }

        private static void FillData(IEnumerable<PropertyInfo> properties, DataTable dt, Object o)
        {
            var dr = dt.NewRow();

            foreach (var pi in properties)
            {
                dr[pi.Name] = pi.GetValue(o, null);
            }

            dt.Rows.Add(dr);
        }

        private static void AddColumnProperty<TProp>(PropertyInfo dataColumnPropertyInfo, DataColumn dataColumn,
                                                     string customAttributePropertyName,
                                                     string columnExtendedPropertyName)
        {
            if (!dataColumnPropertyInfo.GetCustomAttributes(typeof(TProp), false).Any()) return;

            var customAttribute =
                dataColumnPropertyInfo.GetCustomAttributes(typeof(TProp), false).Cast<TProp>().SingleOrDefault();

            var attributePropertyInfo = typeof(TProp).GetProperty(customAttributePropertyName);

            dataColumn.ExtendedProperties.Add(columnExtendedPropertyName,
                                              attributePropertyInfo.GetValue(customAttribute, null));
        }
    }
}