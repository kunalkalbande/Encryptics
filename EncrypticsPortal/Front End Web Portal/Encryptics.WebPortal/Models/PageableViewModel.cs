using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Models
{
    public class PageableViewModel<T>
    {
        private IEnumerable<T> _dataItems;

        public IEnumerable<T> DataItems
        {
            get
            {
                return _dataItems == null ? new List<T>() : _dataItems.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
            }

            set { _dataItems = value; }
        }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public virtual int PageCount
        {
            get
            {
                int numPages;

                if (_dataItems == null)
                {
                    numPages = 0;
                }
                else
                {
                    numPages = _dataItems.Count() / PageSize;

                    if (numPages == 0) return 1;

                    if ((_dataItems.Count() % PageSize) > 0)
                    {
                        numPages++;
                    }
                }

                return numPages;
            }
        }

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

        public IEnumerable<SelectListItem> PageSizes
        {
            get { return _pageSizes; }
        }

        private readonly IEnumerable<SelectListItem> _pageSizes = new List<SelectListItem>
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

        public string TableBodyPartialView { get; set; }
    }
}