using Encryptics.WebPortal.Areas.UserAdmin.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using MyResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class CompanySummaryModel : CompanyModel
    {
        public long ActiveUserAccountTotal { get; set; }

        public long SuspendedUserAccountTotal { get; set; }

        public long PendingUserAccountTotal { get; set; }

        [Display(ResourceType = typeof (MyResources), Name = "CompanySummaryModel_EncrypticsUsage_Usage")]
        public DataTable EncrypticsUsage { get; set; }

        [Display(Name = "Domain Restrictions")]
        public IEnumerable<CompanyDomainModel> Domains { get; set; }

        public ActiveAccountsSearchModel ActiveAccountSearchParameters { get; set; }

        public PendingAccountsSearchModel PendingAccountSearchParameters { get; set; }

        public DataTable TopFiveUsers { get; set; }

        [Display(Name = "Encrypted Violations")]
        public long PBPViolations { get; set; }

        [Display(Name = "Justified Exceptions")]
        public long PBPExceptions { get; set; }

        [Display(Name = "Total Violations")]
        public long ZDPViolations { get; set; }

        [Display(Name = "Violating Users")]
        public long ZDPViolatingUsers { get; set; }

        public bool ZDPEnabled { get; set; }

        public bool PBPEnabled { get; set; }
    }
}