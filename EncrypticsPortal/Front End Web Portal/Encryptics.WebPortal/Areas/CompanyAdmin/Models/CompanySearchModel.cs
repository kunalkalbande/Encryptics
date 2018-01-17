using System.Linq;
using Encryptics.WebPortal.Models;

namespace Encryptics.WebPortal.Areas.CompanyAdmin.Models
{
    public class CompanySearchModel : PageableViewModel<CompanyListItemModel>
    {
        public string SearchTerm { get; set; }

        //public long ItemCount { get { return DataItems.Count(); } }
        public long ItemCount { get; set; }
    }
}