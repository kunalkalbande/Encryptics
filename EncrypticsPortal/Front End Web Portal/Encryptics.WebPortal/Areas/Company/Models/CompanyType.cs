using System.ComponentModel.DataAnnotations;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public enum CompanyType
    {
        [Display(Name="Direct Customer")]
        DirectCustomer,
        Reseller,
        [Display(Name = "Reseller Customer")]
        ResellerCustomer,
        Master
    }
}