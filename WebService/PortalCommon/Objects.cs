using System;
using ServicesCommon;

namespace PortalCommon
{
    // CLASSES //
    
    public class ContactInfo
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Fax { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
    }
    
    public class PortalRoleListItem
    {
        public long Id { get; set; }
        public string Title { get; set; }
    }
    
    public class UserAccount
    {
        public long Id { get; set; }
        public int DepartmentId { get; set; }
        public long EntityId { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Company { get; set; }
        public bool IsLockedOut { get; set; }
        public bool IsSuspended { get; set; }
        public bool IsUsingTemporaryPassword { get; set; }
        public bool IsForcePasswordChange { get; set; }
        public DateTime LastPasswordChange { get; set; }
        public ContactInfo ContactInfo { get; set; }
        public AccountType Type { get; set; }
        //public RoleType Role { get; set; }
        public PortalRoleListItem Role { get; set; }
        public UserAccountCountList CountList { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LicenseCreatedDate { get; set; }
        public DateTime? LicenseExpirationDate { get; set; }
        public bool IsAwaitingApproval { get; set; }
    }

    public class UserAccountCountList
    {
        public long AliasCount { get; set; }
        public long DeviceCount { get; set; }
        public long EncryptionCount { get; set; }
    }
    
    public class UserIdentifiers
    {
        public long Id { get; set; }
        public long EntityId { get; set; }
    }
    
}
