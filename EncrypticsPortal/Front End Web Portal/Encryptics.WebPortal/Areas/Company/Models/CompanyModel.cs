using System;
using System.ComponentModel.DataAnnotations;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class CompanyModel
    {
        public long Id { get; set; }

        [Display(Name = "Company")]
        [Required, MaxLength(100), StringLength(100)]
        public string Name { get; set; }

        [Display(Name = "Company Type")]
        public CompanyType EntityType { get; set; }

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = "Licenses")]
        public LicensingModel LicenseSummary { get; set; }

        [Display(Name = "Use Global Expiration Date?")]
        public bool UseGlobalExpirationDate { get; set; }

        [Display(Name = "Global Expiration Date")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true, NullDisplayText = "")]
        public DateTime? GlobalExpirationDate { get; set; }

        [Display(Name = "PBP Enabled")]
        public bool IsPBPEnabled { get; set; }

        [Display(Name = "ZDP Enabled")]
        public bool IsZDPEnabled { get; set; }
    }
}