namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class UserEncryptionModel
    {
        public long UserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public long Encryptions { get; set; }
    }
}