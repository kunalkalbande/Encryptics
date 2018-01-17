using System.Collections.Generic;
using System.ComponentModel;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class DLPPolicyModel
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public IEnumerable<DLPRuleTypeModel> RuleTypes { get; set; }
        public bool IsUsed { get; set; }
    }

    public enum DLPPolicyId
    {
        [Description("Default")]
        Undefined = 0,
        [Description("Government Issued Identification numbers")]
        GIIN = 1,
        [Description("Protected Health Information")]
        PHI = 2,
        [Description("Individually Identifiable Information")]
        I3 = 3,
        [Description("Financial and Banking")]
        FAndB = 4,
        [Description("Policies defined to handle white lists and black lists")]
        WhiteBlackList = 5,
        [Description("Policies defined to handle attachments")]
        Attachments = 6,
        [Description("Policies defined to handle exact matches of user specified terms")]
        UserPolicies = 7
    }
}