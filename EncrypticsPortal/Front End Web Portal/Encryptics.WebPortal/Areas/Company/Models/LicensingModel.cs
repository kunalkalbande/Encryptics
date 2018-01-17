using System.ComponentModel.DataAnnotations;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class LicensingModel
    {
        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString="{0:N0}")]
        public long ActiveLicenses { get; set; }

        [Display(Name = "Available")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public long AvailableLicenses { get; set; }

        [Display(Name = "Used")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public long UsedLicenses { get; set; }
    }
}