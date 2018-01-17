using System.ComponentModel.DataAnnotations;
using Encryptics.WebPortal.Models;
using MyResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class UsageRightsGroupModel
    {
        public long GroupId { get; set; }

        [Display(Name = "Name")]
        [DisplayFormat(NullDisplayText = "(Empty)")]
        public string GroupName { get; set; }

        [Display(Name = "Members")]
        public int MemberCount { get; set; }

        [Display(Name = "Status")]
        [BooleanDisplayValues(typeof(MyResources), trueValueName: "ActiveDisplay", falseValueName: "InactiveDisplay")]
        [UIHint("DeviceSessionStatus")]
        public bool IsActive { get; set; }

        public PageableViewModel<UsageRightsGroupMemberModel> GroupMempers { get; set; }
    }

    public class EditableUsageRightsGroupModel : UsageRightsGroupModel
    {
        [Display(Name = "Encrypt Copy")]
        public UsageRightsEncryptOption EncryptCopy { get; set; }

        [Display(Name = "Encrypt Forward")]
        public UsageRightsEncryptOption EncryptForward { get; set; }

        [Display(Name = "Encrypt Print")]
        public UsageRightsEncryptOption EncryptPrint { get; set; }

        [Display(Name = "Encrypt Save")]
        public UsageRightsEncryptOption EncryptSave { get; set; }

        [Display(Name = "Encrypt Sunrise")]
        public UsageRightsEncryptOption EncryptSunrise { get; set; }

        [Display(Name = "Encrypt Sunset")]
        public UsageRightsEncryptOption EncryptSunset { get; set; }

        [Display(Name = "Decrypt Copy")]
        public UsageRightsDecryptOption DecryptCopy { get; set; }

        [Display(Name = "Decrypt Forward")]
        public UsageRightsDecryptOption DecryptForward { get; set; }

        [Display(Name = "Decrypt Print")]
        public UsageRightsDecryptOption DecryptPrint { get; set; }

        [Display(Name = "Decrypt Save")]
        public UsageRightsDecryptOption DecryptSave { get; set; }

        public enum UsageRightsEncryptOption
        {
            Allow,
            Enforce,
            Disable,
        }

        public enum UsageRightsDecryptOption
        {
            Allow,
            Disable,
        }
    }
}