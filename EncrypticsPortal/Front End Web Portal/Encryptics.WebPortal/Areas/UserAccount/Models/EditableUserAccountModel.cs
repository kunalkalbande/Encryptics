using Encryptics.WebPortal.Models;

namespace Encryptics.WebPortal.Areas.UserAccount.Models
{
    public class EditableUserAccountModel : UserAccountModel
    {
        public ContactInfoModel ContactInfo { get; set; }
    }
}