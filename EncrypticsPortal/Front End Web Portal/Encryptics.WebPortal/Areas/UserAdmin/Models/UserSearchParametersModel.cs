using Encryptics.WebPortal.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MyResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Areas.UserAdmin.Models
{
    public class UserSearchParametersModel : PageableViewModel<AccountsListItemModel>
    {
        public UserSearchParametersModel()
        {
            SortField = "Email";
            SortOrder = "ASC";
            CurrentPage = 1;
            PageSize = 15;
            SearchType = UserSearchType.Any;
            SearchTerm = String.Empty;
        }

        public long ItemCount { get; set; }

        public string SortField { get; set; }

        public string SortOrder { get; set; }

        public override int PageCount
        {
            get
            {
                var pageCount = ((int)ItemCount + PageSize - 1) / PageSize;

                return pageCount > 0 ? pageCount : 1;
            }
        }

        [Display(ResourceType = typeof (MyResources), Name = "DisplaySearchBy")]
        public UserSearchType SearchType { get; set; }

        public string SearchTerm { get; set; }

        public enum UserSearchType
        {
            Any,
            Email,
            [Display(ResourceType = typeof (MyResources), Name = "DisplayFirstName")]
            FirstName,
            [Display(ResourceType = typeof (MyResources), Name = "DisplayLastName")]
            LastName
        }

        public enum UserSearchLocation
        {
            Active,
            Pending,
            Both
        }
    }

    public class ActiveAccountsSearchModel : UserSearchParametersModel
    {
        public IEnumerable<ActiveAccountsListItemModel> ActiveAccounts { get; set; }
    }

    public class PendingAccountsSearchModel : UserSearchParametersModel
    {
        public IEnumerable<PendingAccountsListItemModel> PendingAccounts { get; set; }
    }
}