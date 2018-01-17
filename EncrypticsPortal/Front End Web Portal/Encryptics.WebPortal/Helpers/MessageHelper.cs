using System.ComponentModel.DataAnnotations;
using MyResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Helpers
{
    public enum ChangePasswordMessageId
    {
        [Display(ResourceType=typeof(MyResources), Name = "ChangePasswordSuccessMessage")]
        ChangePasswordSuccess,
        [Display(ResourceType=typeof(MyResources), Name = "SetPasswordSuccessMessage")]
        SetPasswordSuccess,
        [Display(ResourceType=typeof(MyResources), Name = "RemoveLoginSuccessMessage")]
        RemoveLoginSuccess,
    }
}