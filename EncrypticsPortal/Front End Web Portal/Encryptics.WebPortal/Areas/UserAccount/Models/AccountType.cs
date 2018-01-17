using System.ComponentModel.DataAnnotations;
using MyResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Areas.UserAccount.Models
{
    public enum AccountType
    {
        [Display(ResourceType = typeof(MyResources), Name = "AccountLicenseTypeProfessional")]
        Professional,
        [Display(ResourceType = typeof(MyResources), Name = "AccountLicenseTypeFree")]
        Free,
        [Display(ResourceType = typeof(MyResources), Name = "AccountLicenseTypeTrial")]
        Trial,
    }
}