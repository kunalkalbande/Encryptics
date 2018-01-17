using Encryptics.WebPortal.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Encryptics.WebPortal.Areas.CompanyAdmin.Models
{
    public class SoftwareReleaseModel
    {
        [Display(Name = "Download URL")]
        [RegularExpression(@"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?", ErrorMessage = "Downalod URL must be the correct format.")]
        [Required]
        public Uri DownloadUri { get; set; }

        [Display(Name = "Release Type")]
        [Required]
        public bool IsMajorRelease { get; set; }

        [Display(Name = "Release Type")]
        [Required]
        public bool SomethingElse { get; set; }

        [Display(Name = "Patch Notes")]
        [Required]
        public string ReleaseNotes { get; set; }

        //public string Version { get; set; }
        [RegularExpression(@"^(?:(\d+)\.)?(?:(\d+)\.)?(?:(\d+)\.)(\*|\d+)$", ErrorMessage = "Version number must be of the form #.#.#.#")]
        [Display(Name = "Version Number")]
        [Required]
        public VersionInfo Version { get; set; }

        [Display(Name = "Product or Platorm")]
        [Required]
        public int ProductId { get; set; }

        public long VersionId { get; set; }

        public string ReleaseType
        {
            get { return IsMajorRelease ? "Major" : "Minor"; }
        }

        public VersionStatus Status { get; set; }

        public bool IsGlobalMinimumVersion { get; set; }
    }
}