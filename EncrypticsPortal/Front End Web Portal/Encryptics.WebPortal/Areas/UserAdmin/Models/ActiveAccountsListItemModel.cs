using Encryptics.WebPortal.Areas.UserAccount.Models;
using System.ComponentModel.DataAnnotations;

namespace Encryptics.WebPortal.Areas.UserAdmin.Models
{
    public class ActiveAccountsListItemModel : AccountsListItemModel
    {
        [Display(Name = "Account Type")]
        public AccountType Type { get; set; }
    }
}