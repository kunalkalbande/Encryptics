using Encryptics.WebPortal.Areas.Company.Models;
using System.ComponentModel.DataAnnotations;

namespace Encryptics.WebPortal.Areas.UserAdmin.Models
{
    public class PendingAccountsListItemModel : AccountsListItemModel
    {
        [Display(Name = "Account Type")]
        public bool IsLicenseReserved { get; set; }

        public PendingUserAccountStatus AccountStatus { get; set; }
    }
}
