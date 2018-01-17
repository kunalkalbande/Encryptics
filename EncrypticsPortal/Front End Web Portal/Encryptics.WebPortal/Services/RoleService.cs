using System.Web.Security;

namespace Encryptics.WebPortal.Services
{
    class RoleService : IRoleService
    {
        public void AddUserToRole(string userName, string roleName)
        {
            if (Roles.GetRolesForUser(userName).Length != 0) return;

            if (!Roles.IsUserInRole(userName, roleName))
            {
                Roles.AddUserToRole(userName, roleName);
            }
        }
    }
}