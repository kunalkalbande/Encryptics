using System.ComponentModel.DataAnnotations;

namespace Encryptics.WebPortal.Models
{
    public class SoftwareReleaseProductModel
    {
        public int ProductId { get; set; }

        [Display(Name="Name")]
        public string ProductName { get; set; }
    }
}