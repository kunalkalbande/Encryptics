using Encryptics.WebPortal.Models;
using System.ComponentModel.DataAnnotations;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class CompanyDetailsModel  : CompanyModel
    {
        [Display(Name = "Company Abbreviation")]
        [MaxLength(10), StringLength(10)]
        public string Abbreviation { get; set; }

        public ContactInfoModel ContactInfo { get; set; }

        [Display(Name = "Business Contact Name")]
        [MaxLength(50), StringLength(50)]
        public string Contact1 { get; set; }

        [Display(Name = "Business Contact Phone Number")]
        [MaxLength(50), StringLength(50)]
        public string Contact1PhoneNumber { get; set; }

        [Display(Name = "Administrative Contact Name")]
        [MaxLength(50), StringLength(50)]
        public string Contact2 { get; set; }

        [Display(Name = "Administrative Contact Phone Number")]
        [MaxLength(50), StringLength(50)]
        public string Contact2PhoneNumber { get; set; }

        [Display(Name = "ParentId")]
        public long ParentId { get; set; }

        [Display(Name = "Parent Company Name")]
        public string ParentCompanyName { get; set; }

        public int RequirePasswordChangeDays { get; set; }

        public bool RequireAccountApproval { get; set; }

        [Display(Name="Is In Trial Mode")]
        public bool IsInTrialMode { get; set; }

        public long UserCount { get; set; }

        public long DeviceCount { get; set; }

        public long PolicyCount { get; set; }

        public long LexiconCount { get; set; }

        public long EncryptionCount { get; set; }

        public long UnactivatedCount { get; set; }
    }
}