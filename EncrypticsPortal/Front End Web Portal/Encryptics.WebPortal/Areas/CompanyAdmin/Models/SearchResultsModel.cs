using Encryptics.WebPortal.Areas.UserAdmin.Models;

namespace Encryptics.WebPortal.Areas.CompanyAdmin.Models
{
    public class SearchResultsModel
    {
        public string SearchTerm { get; set; }

        public CompanySearchModel CompanySearchResults { get; set; }

        public ActiveAccountsSearchModel ActiveAccountSearchParameters { get; set; }

        public PendingAccountsSearchModel PendingAccountSearchParameters { get; set; }

        public ActiveAccountsSearchModel SuspendedAccountSearchParameters { get; set; }
    }
}