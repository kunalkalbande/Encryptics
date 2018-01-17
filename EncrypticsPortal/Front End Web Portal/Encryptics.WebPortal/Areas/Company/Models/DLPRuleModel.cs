using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class DLPRuleModel
    {
        public DLPRuleModel()
        {
            Terms = string.Empty;
        }

        public DLPPolicyId PolicyId { get; set; }

        public int RuleTypeId { get; set; }

        public long RuleId { get; set; }

        public long? EntityRuleId { get; set; }

        [Required, MaxLength(4000, ErrorMessage = "The length of Description cannot exceed 4000 characters.")]
        public string Description { get; set; }

        [Display(Description = "Enable this rule.")]
        public bool Enabled { get; set; }

        [Display(Name = "Terms")]
        public string[] BWList { get; set; }

        [Display(Name = "Severity")]
        public SeverityLevel Severity { get; set; }

        [Display(Name = "Default DRM")]
        public DRMType DefaultDRM { get; set; }

        [Required(AllowEmptyStrings = true), MaxLength(4000, ErrorMessage = "The length of Terms cannot exceed 4000 characters."), DisplayFormat(ConvertEmptyStringToNull = false, NullDisplayText="")]
        public string Terms { get; set; }

        [Display(Name = "Example")]
        public string Formats { get; set; }

        public string RegexTerm { get; set; }
    }

    public enum SeverityLevel
    {
        [Display(Name = "Must encrypt")]
        Level1 = 1,
        [Display(Name = "Can send without encryption required with justification")]
        Level2 = 2,
        [Display(Name = "Can send without encryption")]
        Level3 = 3,
        [Display(Name = "Can never send this data")]
        Level4 = 4
    }


    public enum DRMType
    {
        [Display(Name = "None")]
        NoDRM = 0,
        [Display(Name = "Prevent Save")]
        PreventSave = 1,
        [Display(Name = "Prevent Save and Print")]
        PreventSavePrint = 2,
        [Display(Name = "Prevent Save, Print, and Copy")]
        PreventSavePrintCopy = 3,
        [Display(Name = "Prevent Save, Print, Copy, and Forward")]
        PreventAll = 4
    }
}