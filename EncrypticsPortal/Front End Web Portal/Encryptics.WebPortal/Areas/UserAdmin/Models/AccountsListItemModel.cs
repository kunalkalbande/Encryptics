using System;
using System.ComponentModel.DataAnnotations;
using Encryptics.WebPortal.Models;

namespace Encryptics.WebPortal.Areas.UserAdmin.Models
{
    public class AccountsListItemModel
    {
        public long Id { get; set; }

        public long EntityId { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Name")]
        //public string FullName { get { return string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName) ? UserName : string.Format("{0} {1}", FirstName, LastName); } }
        public string FullName { get { return string.Format("{0} {1}", FirstName, LastName); } }

        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Display(Name = "User Role")]
        public WebPortalRole UserWebPortalRole { get; set; }

        [Display(Name = "Licence Assigned On")]
        //[DisplayFormat(NullDisplayText = "", DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? LicenseCreated { get; set; }

        [Display(Name = "License Expires On")]
        //[DisplayFormat(NullDisplayText = "", DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? LicenseExpires { get; set; }

        public bool HasLicense { get { return LicenseCreated != null && LicenseExpires != null && DateTime.Now <= LicenseExpires; } }
    }
}