using Encryptics.WebPortal.PortalService;
using StructureMap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class ReportParametersModel
    {
        private string _orderByDir;

        public ReportParametersModel()
        {
            Page = 1;
            PageSize = 10;
            SelectedReportId = -1;
            StartDate = DateTime.Today.AddDays(-14);
            EndDate = DateTime.Today;
            ResetForm = true;
            Reports = ObjectFactory.GetAllInstances<ReportDefinitionModel>();
            Reports.ToList().ForEach(r => r.IsVisible = true);
            TopCount = 5;
        }

        public ReportParametersModel(IEnumerable<ReportDefinitionModel> reportDefinitions)
            : this()
        {
            Reports = reportDefinitions;
        }

        public long EntityId { get; set; }

        [Display(Name = "Select a Report")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid report.")]
        [Required]
        public int SelectedReportId { get; set; }

        public IEnumerable<SelectListItem> ReportNames
        {
            get
            {
                return from report in Reports
                       where report.IsVisible
                       select new SelectListItem
                           {
                               Value = report.Id.ToString(CultureInfo.InvariantCulture),
                               Text = report.Name
                           };
            }
        }

        [Required]
        public ReportDefinitionModel SelectedReport
        {
            get
            {
                return Reports.SingleOrDefault(report => report.Id == SelectedReportId);
            }
        }

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        [Required]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        [Required]
        public DateTime EndDate { get; set; }

        [Display(Name = "Top Count")]
        public long? TopCount { get; set; }

        public static IEnumerable<SelectListItem> TopCounts
        {
            get { return _topCountsItems; }
        }

        public string ReportName
        {
            get { return SelectedReport != null ? SelectedReport.Name : String.Empty; }
        }

        public string DefaultSortColumn
        {
            get { return SelectedReport != null ? SelectedReport.DefaultSortColumn : null; }
        }

        public string OrderBy { get; set; }

        public string OrderByDir
        {
            get { return !String.IsNullOrEmpty(OrderBy) ? String.IsNullOrEmpty(_orderByDir) ? "ASC" : _orderByDir : null; }
            set { _orderByDir = value; }
        }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public IEnumerable<SelectListItem> PageList
        {
            get
            {
                for (int i = 1; i <= PageCount; i++)
                {
                    yield return new SelectListItem
                        {
                            Text = i.ToString(CultureInfo.InvariantCulture),
                            Value = i.ToString(CultureInfo.InvariantCulture)
                        };
                }
            }
        }

        public DataTable ReportData { get; set; }

        public string ReportType { get { return SelectedReport.ReportType; } }

        public long TotalRecords { get; set; }

        public long PageCount
        {
            get { return (TotalRecords - 1) / PageSize + 1; }
        }

        public bool ShowTopCount
        {
            get { return SelectedReport != null && SelectedReport.HasTopCountParameter; }
        }

        public IEnumerable<int> ShowTopCountFor { get { return Reports.Where(r => r.HasTopCountParameter).Select(r => r.Id); } }

        public IEnumerable<int> ShowFileListFor { get { return Reports.Where(r => r.ShowFileTypes).Select(r => r.Id); } }

        public bool ResetForm { get; set; }

        public string ChartDataJson { get; set; }

        public bool ShowGraphOnly { get { return SelectedReport != null && SelectedReport.ShowGraphOnly; } }

        public bool IsGraphyAvailable { get { return SelectedReport != null && SelectedReport.IsGraphAvailable; } }

        public static IEnumerable<SelectListItem> PageSizes
        {
            get { return _pageSizes; }
        }

        public bool AggregateData
        {
            get
            {
                return SelectedReport != null && SelectedReport.AggregateData;
            }
        }

        public string NumericColumn
        {
            get
            {
                return SelectedReport != null ? SelectedReport.NumericColumn : string.Empty;
            }
        }

        [Display(Name = "Selected File Type")]
        public string SelectedFileType { get; set; }

        public bool ShowFileTypes
        {
            get { return SelectedReport != null && SelectedReport.ShowFileTypes; }
        }

        //public IEnumerable<SelectListItem> FileTypes { get; set; }

        public IEnumerable<ReportDefinitionModel> Reports { get; set; }

        public void FillData(TokenAuth token, ReportParametersModel model, PortalServiceSoap proxy, ReportParams reportParams)
        {
            SelectedReport.FillData(token, model, proxy, reportParams, true);
        }

        private static readonly IEnumerable<SelectListItem> _pageSizes = new List<SelectListItem>
            {
                new SelectListItem
                    {
                        Text = "10",
                        Value = "10"
                    },
                new SelectListItem
                    {
                        Text = "15",
                        Value = "15"
                    },
                new SelectListItem
                    {
                        Text = "20",
                        Value = "20"
                    },
                new SelectListItem
                    {
                        Text = "25",
                        Value = "25"
                    },
                new SelectListItem
                    {
                        Text = "50",
                        Value = "50"
                    },
                new SelectListItem
                    {
                        Text = "100",
                        Value = "100"
                    },
                new SelectListItem
                    {
                        Text = "All",
                        Value = Int32.MaxValue.ToString(CultureInfo.InvariantCulture)
                    },
            };

        private static readonly List<SelectListItem> _topCountsItems = new List<SelectListItem>
            {
                new SelectListItem {Text = "1", Value = "1"},
                new SelectListItem {Text = "5", Value = "5"},
                new SelectListItem {Text = "10", Value = "10"},
                new SelectListItem {Text = "15", Value = "15"},
                new SelectListItem {Text = "20", Value = "20"},
                new SelectListItem {Text = "25", Value = "25"},
                new SelectListItem {Text = "50", Value = "50"}
            };
    }
}