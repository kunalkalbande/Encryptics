using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Encryptics.WebPortal.Models;
using MyResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Areas.UserAccount.Models
{
    public class UsageSummaryModel
    {
        //[Display(Name = "ID")]
        public string Id
        {
            get
            {
                return Year.ToString(CultureInfo.CurrentUICulture) + ((Month == null) ? "" : Month.ToString()) +
                       ((Day == null) ? "" : Day.ToString());
            }
        }

        [Display(ResourceType = typeof (MyResources), Name = "UsageSummaryYearDisplay")]
        public int Year { get; set; }

        [Display(ResourceType = typeof (MyResources), Name = "UsageSummaryMonthDisplay")]
        public int? Month { get; set; }

        [Display(ResourceType = typeof (MyResources), Name = "UsageSummaryDayDisplay")]
        public int? Day { get; set; }

        [Display(ResourceType = typeof (MyResources), Name = "UsageSummaryEncryptsDisplay")]
        public long Encrypts { get; set; }

        [Display(ResourceType = typeof (MyResources), Name = "UsageSummaryDecryptsDisplay")]
        public int Decrypts { get; set; }

        [Display(ResourceType = typeof (MyResources), Name = "UsageSummaryPartialDisplay"), 
        BooleanDisplayValues(typeof(MyResources), trueValueName: "ActiveDisplay", falseValueName: "InactiveDisplay")]
        public bool IsPartial { get; set; }
    }
}