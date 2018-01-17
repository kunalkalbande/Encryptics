using System.Configuration;
using Encryptics.WebPortal.PortalService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class CompanyDLPSettingsModel
    {
        public CompanyDLPSettingsModel()
        {
            Policies = new List<DLPPolicy>();
        }
        public long Id { get; set; }

        [Display(Name = "Encrypt All Outgoing Messages")]
        public bool EnableAlwaysEncrypt { get; set; }

        [Display(Name = "Turn on Policy-Based Encryption")]
        [ContextHelp(@"Policy-Based Encryption enables you to define rules based on your company’s security policies. Use this option to enforce pre-defined policies from the list below, or create your own custom rules. The settings you choose will apply automatically to all messages sent from {0} users in this company")]
        public bool EnableDLP { get; set; }

        public List<DLPLexicon> Lexicons { get; set; }

        [Display(Name = "Policies")]
        public IEnumerable<DLPPolicy> Policies { get; set; }

        [Display(Name = "Define Data Rights Management (DRM) Settings ")]
        [ContextHelp(@"DRM settings enable you to manage the usage of data. In other words, you control what recipients can do with messages they receive. Use this option to prevent Forward, Copy, Print, and/or Save functions on recipient devices. The settings you choose will apply automatically to all messages sent from Encryptics users in this company.")]
        public DRMType DefaultDRMForAlwaysEncrypt { get; set; }

        [Display(Name = "Passive mode")]
        [ContextHelp(@"Passive mode will record all violations based on the rules you set below. Use this mode to gather metrics without notifying the user or enforcing encryptions.")]
        public bool EnablePassiveMode { get; set; }

        [Display(Name = "Encrypt on first violation")]
        [ContextHelp(@"This option will scan messages until a violation is found. When the first violation is found, the scan will end and the message will be encrypted without notifying the user. Note: because the scan ends as soon as the first violation is found, this option will not provide complete violation reports.")]
        public bool EncryptUponViolation { get; set; }

        [Display(Name = "Notify user on first violation")]
        [ContextHelp(@"This option will scan messages until a violation is found. When the first violation is found, the scan will end, notify the user of the violation, and provide the option to edit the message or cancel without sending. Note: because the scan ends as soon as the first violation is found, this option will not provide complete violation reports.")]
        public bool OneAndDone { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ContextHelpAttribute : Attribute, IMetadataAware
    {
        public ContextHelpAttribute(string helpText = null)
        {
            HelpText = helpText != null ? string.Format(helpText, ConfigurationManager.AppSettings["CompanyName"]) : null;
        }

        public string HelpText { get; set; }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues["HelpText"] = HelpText;
        }
    }
}