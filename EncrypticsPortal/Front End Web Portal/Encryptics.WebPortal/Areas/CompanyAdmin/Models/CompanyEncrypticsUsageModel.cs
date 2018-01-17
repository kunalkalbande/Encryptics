using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Encryptics.WebPortal.Areas.CompanyAdmin.Models
{
    public class CompanyEncrypticsUsageModel
    {
        public string CompanyName { get; set; }
        public DateTime MonthUsed { get; set; }
        public int Usage { get; set; }
    }
}