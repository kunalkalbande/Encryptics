using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Encryptics.WebPortal.Areas.UserAccount.Models
{
    public class InsertableUserAccountModel : EditableUserAccountModel
    {

        //[Required]
        [DataType(DataType.EmailAddress)]
        //[RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", ErrorMessage = "Invalid email address format")]
        [Compare("UserName", ErrorMessage = "The email address and confirmation email address do not match.")]
        [Display(Name = "Confirm Email Address | UserName")]
        [XmlIgnore]
        public string ConfirmUserName { get; set; }

        [Required(ErrorMessage = "You must select Yes or No to deterine if the user will be able to download Encrytpics for Email after creating their password.")]
        [Display(Name = "Make the Encryptics for Email Download Available?")]
        [XmlIgnore]
        public bool DownloadSoftware { get; set; }

        [Required(ErrorMessage = "You must select Yes or No to deterine if the user will be be granted a license during activation.")]
        [Display(Name = "Assign the user a license?")]
        public bool AssignLicense { get; set; }

        [XmlIgnore]
        [Display(Name="Errors")]
        public string ValidationError { get; set; }

        [XmlIgnore]
        public bool HasValidationErrors {  get { return !string.IsNullOrEmpty(ValidationError); } }
    }

    [XmlRoot("Accts")]
    public class UploadAccountsModel
    {
        [XmlIgnore]
        public string FileName { get; set; }

        [XmlElement("Acct")]
        public List<InsertableUserAccountModel> Accounts { get; set; }
    }
}