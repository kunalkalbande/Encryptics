using System;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class CompanyUsageSummaryModel
    {
        public DateTime Date { get; set; }
        public long Encrypts { get; set; }
        public long Decrypts { get; set; }
    }
}