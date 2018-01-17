
namespace Encryptics.WebPortal.Areas.UserAdmin.Models
{
    public class AccountsFileUploadModel
    {
        public long Id { get; set; }
        public long EntityId { get; set; }
        public bool SoftwarePreinstalled { get; set; }
        public bool  ReserveLicenses { get; set; }
    }
}