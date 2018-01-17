using System;

namespace Encryptics.WebPortal.Areas.UserAccount.Models
{
    public class UserEmailModel
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ProfileName { get; set; }
        public DateTime Sent { get; set; }
        public string Status { get; set; }
        public string BodyType { get; set; }
    }
}