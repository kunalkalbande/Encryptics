using System.Collections.Generic;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class DLPRulesVisibilityModel
    {
        public IEnumerable<PolicyVisibilityModel> Policies { get; set; }
    }

    public class PolicyVisibilityModel
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public IEnumerable<RuleTypeVisibilityModel> RuleTypes { get; set; }
    }

    public class RuleTypeVisibilityModel
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public IEnumerable<RulesVisibilityModel> Rules { get; set; }
    }

    public class RulesVisibilityModel
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public bool IsVisible { get; set; }
    }
}