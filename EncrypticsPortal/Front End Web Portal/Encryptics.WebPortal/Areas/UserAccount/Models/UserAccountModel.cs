using Encryptics.WebPortal.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using MyResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Areas.UserAccount.Models
{
    public class UserAccountModel
    {
        //private WebPortalRole _primaryWebPortalRole;

        [XmlIgnore]
        [Required(ErrorMessage = "Invalid user name or id specified.")]
        public long UserId { get; set; }

        [XmlIgnore]
        public long EntityId { get; set; }

        [Display(ResourceType = typeof(MyResources), Name = "FirstNameDisplay")]
        [MaxLength(50, ErrorMessageResourceType = typeof(MyResources), ErrorMessageResourceName = "FirstNameLengthErrorMessage")]
        [StringLength(50, ErrorMessageResourceType = typeof(MyResources), ErrorMessageResourceName = "FirstNameLengthErrorMessage")]
        [XmlElement("FName")]
        public string FirstName { get; set; }

        [Display(ResourceType = typeof(MyResources), Name = "LastNameDisplay")]
        [MaxLength(50, ErrorMessageResourceType = typeof(MyResources), ErrorMessageResourceName = "LastNameLengthErrorMessage")]
        [StringLength(50, ErrorMessageResourceType = typeof(MyResources), ErrorMessageResourceName = "LastNameLengthErrorMessage")]
        [XmlElement("LName")]
        public string LastName { get; set; }

        [Display(ResourceType = typeof(MyResources), Name = "NameDisplay")]
        [XmlIgnore]
        public string Name { get { return string.IsNullOrEmpty(string.Format("{0} {1}", FirstName, LastName).Trim()) ? UserName : string.Format("{0} {1}", FirstName, LastName).Trim(); } }

        [Display(ResourceType = typeof(MyResources), Name = "CompanyNameDisplay")]
        [XmlIgnore]
        public string CompanyName { get; set; }

        [Display(ResourceType = typeof(MyResources), Name = "UserNameDisplay")]
        [XmlElement("Email")]
        [Required]
        [RegularExpression(
            @"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$",
            ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "InvalidEmailAddressFormatErrorMessage")]
        public string UserName { get; set; }

        [Display(ResourceType = typeof(MyResources), Name = "AccountStatusDisplay")]
        [XmlIgnore]
        //[Required] // TODO: why did i make this required?
        public string AccountStatus { get; set; }

        [Display(ResourceType = typeof(MyResources), Name = "LicenseTypeDisplay")]
        [XmlIgnore]
        public AccountType LicenseType { get; set; }

        [Display(ResourceType = typeof(MyResources), Name = "DateRegisteredDisplay")]
        [XmlIgnore]
        public DateTime DateRegistered { get; set; }

        [Display(ResourceType = typeof(MyResources), Name = "DateLicenseCreatedDisplay")]
        [XmlIgnore]
        public DateTime DateLicenseCreated { get; set; }

        [Display(ResourceType = typeof(MyResources), Name = "DateLicenseExpiresDisplay")]
        [XmlIgnore]
        public DateTime DateLicenseExpires { get; set; }

        [Display(ResourceType = typeof(MyResources), Name = "DatePasswordChangedDisplay")]
        [XmlIgnore]
        public DateTime DatePasswordChanged { get; set; }

        [XmlIgnore]
        [Display(ResourceType = typeof (MyResources), Name = "RoleDisplay")]
        public WebPortalRole PrimaryRole { get; set; }

        [XmlIgnore]
        [Display(ResourceType = typeof(MyResources), Name = "RoleDisplay")]
        public string AdminPortalRoleName { get { return PrimaryRole.RoleName; } }

        [XmlIgnore]
        [Display(ResourceType = typeof(MyResources), Name = "RoleDisplay")]
        public long AdminPortalRoleId { get; set; }

        public bool IsSuspended { get; set; }

        public bool IsLockedOut { get; set; }
    }
}