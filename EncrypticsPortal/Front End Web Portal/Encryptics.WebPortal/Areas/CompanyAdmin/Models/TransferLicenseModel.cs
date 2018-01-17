using System.ComponentModel.DataAnnotations;
using Encryptics.WebPortal.Attributes;

namespace Encryptics.WebPortal.Areas.CompanyAdmin.Models
{
    public class TransferLicenseModel
    {
        [Required, Display(Name="Transfer From")]
        public string FromEntityName { get; set; }
        [Required, MinValue(1, "The company selected to transfer licenses from does not exist."), Display(Name = "Transfer From")]
        public long FromEntityId { get; set; }
        [Required, Display(Name = "Transfer To")]
        public string ToEntityName { get; set; }
        [Required, MinValue(1, "The company selected to receive licenses does not exist."), Display(Name = "Transfer To")]
        public long ToEntityId { get; set; }
        [Required]
        public LicensePool FromLicensePool { get; set; }
        [Required]
        public LicensePool ToLicensePool { get; set; }
        [Display(Name="Transfer Amount"), Required, MinValue(1, "Transfer amount must be greater than zero.")]
        public int TransferAmount { get; set; }
    }

    public enum LicensePool
    {
        [Display(Name = "Active")]
        ActivePool,
        [Display(Name="Unassigned")]
        AvailablePool
    }
}