using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Encryptics.WebPortal.Models
{
    /// <summary>
    /// Get User details from Azure
    /// </summary>
    public class AzureUserDetail
    {
       
        public string objectType { get; set; }
        public string objectId { get; set; }
        public object deletionTimestamp { get; set; }
        public bool accountEnabled { get; set; }
        public object ageGroup { get; set; }
        public List<object> assignedLicenses { get; set; }
        public List<object> assignedPlans { get; set; }
        public string city { get; set; }
        public object companyName { get; set; }
        public object consentProvidedForMinor { get; set; }
        public string country { get; set; }
        public object creationType { get; set; }
        public object department { get; set; }
        public object dirSyncEnabled { get; set; }
        public string displayName { get; set; }
        public object employeeId { get; set; }
        public string facsimileTelephoneNumber { get; set; }
        public string givenName { get; set; }
        public object immutableId { get; set; }
        public object isCompromised { get; set; }
        public object jobTitle { get; set; }
        public object lastDirSyncTime { get; set; }
        public object legalAgeGroupClassification { get; set; }
        public string mail { get; set; }
        public string mailNickname { get; set; }
        public string mobile { get; set; }
        public object onPremisesDistinguishedName { get; set; }
        public object onPremisesSecurityIdentifier { get; set; }
        public List<string> otherMails { get; set; }
        public string passwordPolicies { get; set; }
        public object physicalDeliveryOfficeName { get; set; }
        public string postalCode { get; set; }
        public object preferredLanguage { get; set; }
        public List<object> provisionedPlans { get; set; }
        public List<object> provisioningErrors { get; set; }
        public List<object> proxyAddresses { get; set; }
        public DateTime refreshTokensValidFromDateTime { get; set; }
        public object showInAddressList { get; set; }
        public List<object> signInNames { get; set; }
        public object sipProxyAddress { get; set; }
        public string state { get; set; }
        public string streetAddress { get; set; }
        public string surname { get; set; }
        public string telephoneNumber { get; set; }
        public object usageLocation { get; set; }
        public List<object> userIdentities { get; set; }
        public string userPrincipalName { get; set; }
        public string userType { get; set; }
    }
}