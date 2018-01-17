using Encryptics.WebPortal.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class CompanyProductVersionModel : IComparable<CompanyProductVersionModel>
    {
        public long VersionId { get; set; }
        
        public long EntityId { get; set; }

        public int ProductId { get; set; }
        
        [Display(Name="Release Version")]
        public VersionInfo VersionNumber { get; set; }
        
        public Uri Url { get; set; }
        
        [Display(Name = "Patch Notes")]
        public string ReleaseNotes { get; set; }
        
        public VersionStatus Status { get; set; }
        [Display(Name="Release Type")]
        
        public VersionSeverity Severity { get; set; }
        
        public bool IsCurrentCompanyVersion { get; set; }

        public bool IsGlobalMinimumVersion { get; set; }

        public bool IsCompanyVersion { get; set; }

        public int CompareTo(CompanyProductVersionModel other)
        {
            if (VersionNumber > other.VersionNumber) return -1;

            return VersionNumber == other.VersionNumber ? 0 : 1;
        }
    }
}