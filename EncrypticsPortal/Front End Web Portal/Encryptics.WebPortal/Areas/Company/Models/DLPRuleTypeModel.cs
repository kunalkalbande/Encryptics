using System.Collections.Generic;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class DLPRuleTypeModel
    {
        public DLPPolicyId? PolicyId { get; set; }
        public long Id { get; set; }
        public string Description { get; set; }
        public IEnumerable<DLPRuleModel> Rules { get; set; }
        public bool IsUsed { get; set; }
    }
}