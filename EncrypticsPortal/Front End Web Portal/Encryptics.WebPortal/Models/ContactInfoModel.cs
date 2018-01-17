using System.ComponentModel.DataAnnotations;

namespace Encryptics.WebPortal.Models
{
    public class ContactInfoModel
    {
        [Display(Name = "Email Address")]
        [MaxLength(100)]
        [StringLength(100)]
        public string Email { get; set; }

        [Display(Name = "Phone (Main)")]
        [MaxLength(20)]
        [StringLength(20)]
        public string Phone { get; set; }

        [Display(Name = "Phone (Mobile)")]
        [MaxLength(20)]
        [StringLength(20)]
        public string Mobile { get; set; }

        [Display(Name = "Phone (Fax)")]
        [MaxLength(20)]
        [StringLength(20)]
        public string Fax { get; set; }

        [Display(Name = "Address")]
        [MaxLength(100)]
        [StringLength(100)]
        public string Address1 { get; set; }

        [Display(Name = "Address (continued)")]
        [MaxLength(100)]
        [StringLength(100)]
        public string Address2 { get; set; }

        [Display(Name = "City")]
        [MaxLength(100)]
        [StringLength(100)]
        public string City { get; set; }

        [Display(Name = "State/Province")]
        [MaxLength(100)]
        [StringLength(100)]
        public string State { get; set; }

        [Display(Name = "Zip Code")]
        [MaxLength(20)]
        [StringLength(20)]
        public string ZipCode { get; set; }

        [Display(Name = "Country")]
        [MaxLength(100)]
        [StringLength(100)]
        public string Country { get; set; }

        [Display(Name = "Region/County")]
        [MaxLength(100)]
        [StringLength(100)]
        public string Region { get; set; }
    }
}