using System.Collections.Generic;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Linq;
using System.Web.Security;

namespace Encryptics.WebPortal.IdentityModel
{
    public class EncrypticsRoleProvider : RoleProvider
    {
        static readonly IDictionary<string, IList<string>> _userRoles = new Dictionary<string, IList<string>>();
        private static readonly IList<string> _roles = new List<string>();

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            Trace.TraceInformation("Initializing Role Provider");

            base.Initialize(name, config);
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            Trace.TraceInformation("IsUserInRole");

            Debug.Print("username: {0}", username);
            Debug.Print("rolename: {0}", roleName);

            return UserExists(username) && _userRoles[username].Contains(roleName);
        }

        private static bool UserExists(string username)
        {
            Trace.TraceInformation("UserExists");

            Debug.Print("username: {0}", username);

            return _userRoles.ContainsKey(username);
        }

        public override string[] GetRolesForUser(string username)
        {
            Trace.TraceInformation("GetRolesForUser");

            Debug.Print("username: {0}", username);

            return UserExists(username) ? _userRoles[username].ToArray() : new string[] { };
        }

        public override void CreateRole(string roleName)
        {
            Trace.TraceInformation("CreateRole");

            Debug.Print("rolename: {0}", roleName);

            _roles.Add(roleName);
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            Trace.TraceInformation("DeleteRole");

            Debug.Print("rolename: {0}", roleName);

            if (!RoleExists(roleName))
            {
                throw new ProviderException("Role does not exist.");
            }

            if (throwOnPopulatedRole && GetUsersInRole(roleName).Length > 0)
            {
                throw new ProviderException("Cannot delete a populated role.");
            }

            return _roles.Remove(roleName);
        }

        public override bool RoleExists(string roleName)
        {
            Trace.TraceInformation("RoleExists");

            Debug.Print("rolename: {0}", roleName);

            return _roles.Contains(roleName);
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            Trace.TraceInformation("AddUsersToRoles");

            Debug.Print("usernames: {0}", usernames.Aggregate((s1, s2) => ", " + s2));

            Debug.Print("roleNames: {0}", roleNames.Aggregate((s1, s2) => ", " + s2));

            foreach (var username in usernames)
            {
                roleNames.Where(roleName => !IsUserInRole(username, roleName)).ToList().ForEach(r =>
                    {
                        if (!UserExists(username))
                            _userRoles[username] = new List<string>();

                        _userRoles[username].Add(r);
                    });
            }
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            Trace.TraceInformation("RemoveUsersFromRoles");

            Debug.Print("usernames: {0}", usernames.Aggregate((s1, s2) => ", " + s2));

            Debug.Print("roleNames: {0}", roleNames.Aggregate((s1, s2) => ", " + s2));

            usernames.Where(UserExists).ToList()
                .ForEach(u => roleNames.Where(u.Contains).ToList()
                    .ForEach(r => _userRoles[u].Remove(r)));
        }

        public override string[] GetUsersInRole(string roleName)
        {
            Trace.TraceInformation("GetUsersInRole");

            Debug.Print("roleName: {0}", roleName);

            return _userRoles.Where(u => u.Value.Contains(roleName))
                .Select(s => s.Key).ToArray();
        }

        public override string[] GetAllRoles()
        {
            Trace.TraceInformation("GetAllRoles");

            return _roles.ToArray();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            Trace.TraceInformation("FindUsersInRole");

            Debug.Print("rolename: {0}", roleName);

            Debug.Print("usernameToMatch: {0}", usernameToMatch);
            
            return _userRoles.Where(u => u.Key == usernameToMatch && u.Value.Contains(roleName))
                .Select(s => s.Key).ToArray();
        }

        public override string ApplicationName { get; set; }
    }
}