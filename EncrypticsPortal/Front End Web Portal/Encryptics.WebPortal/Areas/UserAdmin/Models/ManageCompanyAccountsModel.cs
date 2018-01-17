namespace Encryptics.WebPortal.Areas.UserAdmin.Models
{
    public class ManageCompanyAccountsModel
    {
        public long EntityId { get; set; }
        public string CompanyName { get; set; }
        public ActiveAccountsSearchModel ActiveAccounts { get; set; }
        public PendingAccountsSearchModel PendingAccounts { get; set; }
        public ActiveAccountsSearchModel SuspendedAccounts { get; set; }
    }
}