using Encryptics.WebPortal.Areas.UserAdmin.Models;
using Encryptics.WebPortal.Helpers;
using Encryptics.WebPortal.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using GlobalResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Areas.CompanyAdmin.Models
{
    public class UserSearchModel
    {
        private static readonly IEnumerable<SelectListItem> _searchLocations = EnumHelper.SelectListFor<UserSearchParametersModel.UserSearchLocation>();
        private static readonly IEnumerable<SelectListItem> _searchType = EnumHelper.SelectListFor<UserSearchParametersModel.UserSearchType>();

        public UserSearchModel()
        {
            SearchLocation = UserSearchParametersModel.UserSearchLocation.Both;
            SearchParameters = new UserSearchParametersModel();
        }

        public long? EntityId { get; set; }

        [Display(ResourceType = typeof (GlobalResources), Name = "DisplaySearchFor")]
        public UserSearchParametersModel.UserSearchLocation SearchLocation { get; set; }

        [Required(ErrorMessageResourceType = typeof (GlobalResources), ErrorMessageResourceName = "ErrorMessageMustEnterEmail")]
        public string SearchTerm { get; set; }

        public UserSearchParametersModel SearchParameters { get; set; }

        public PageableViewModel<ActiveAccountsListItemModel> ActiveAccounts { get; set; }

        public PageableViewModel<ActiveAccountsListItemModel> SuspendedAccounts { get; set; }

        public PageableViewModel<PendingAccountsListItemModel> PendingAccounts { get; set; }

        public IEnumerable<SelectListItem> AllSearchLocations { get { return _searchLocations; } }

        public IEnumerable<SelectListItem> AllSearchTypes { get { return _searchType; } }
    }
}