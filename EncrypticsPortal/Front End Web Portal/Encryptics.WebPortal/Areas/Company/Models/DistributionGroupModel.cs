using Encryptics.WebPortal.Models;
using System.ComponentModel.DataAnnotations;
using MyResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class DistributionGroupModel 
    {
        public long GroupId { get; set; }

        [Display(Name="Name")]
        [DisplayFormat(NullDisplayText = "(Empty)")]
        public string GroupName { get; set; }

        [Display(Name="Members")]
        public int MemberCount { get; set; }

        [Display(Name="Status")]
        [BooleanDisplayValues(typeof(MyResources), trueValueName: "ActiveDisplay", falseValueName: "InactiveDisplay")]
        [UIHint("DeviceSessionStatus")]
        public bool IsActive { get; set; }

        public PageableViewModel<DistributionGroupMemberModel> GroupMempers { get; set; }
    }
}