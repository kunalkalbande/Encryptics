using Encryptics.WebPortal.Models;

namespace Encryptics.WebPortal.Areas.UserAccount.Models
{
    public class AccountOverviewModel
    {
        public UserAccountModel Account { get; set; }
        public PageableViewModel<UsageSummaryModel> Usage { get; set; }
        public PageableViewModel<DeviceModel> Devices { get; set; }
    }
}