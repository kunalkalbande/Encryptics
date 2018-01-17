using System.Data;

namespace Encryptics.WebPortal.Areas.CompanyAdmin.Models
{
    public class CompanyAdminModel
    {
        public DataTable EncryptsData { get; set; }
        public DataTable DecryptsData { get; set; }
        public DataTable LicenseExpirationData { get; set; }
    }
}