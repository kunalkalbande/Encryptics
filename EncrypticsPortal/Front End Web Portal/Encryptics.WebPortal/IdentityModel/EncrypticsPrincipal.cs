using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Security;
using StructureMap;

namespace Encryptics.WebPortal.IdentityModel
{
    public class EncrypticsPrincipal : RolePrincipal
    {
        private /*static*/ readonly PortalServiceSoap _portalService;

        //static EncrypticsPrincipal()
        //{
        //    _portalService = ObjectFactory.GetInstance<PortalServiceSoap>();
        //}

        public EncrypticsPrincipal(IIdentity identity, PortalServiceSoap portalService)
            : base(identity)
        {
            ViewPermissions = new Dictionary<string, bool>();
            _portalService = portalService;
        }

        public EncrypticsPrincipal(IIdentity identity, string token, long entityId, long userId, PortalServiceSoap portalService)
            : this(identity, portalService)
        {
            ViewPermissions = new Dictionary<string, bool>();
            Token = token;
            EntityId = entityId;
            UserId = userId;
        }
        public string UserName { get; set; }
        public string Auth { get; set; }
        public string Token { get; set; }
        public long EntityId { get; set; }
        public long UserId { get; set; }
        public long CompanyCount { get; set; }
        public int UTCOffset { get; set; }
        public bool UsesDST { get; set; }
        public Dictionary<string, bool> ViewPermissions { get; set; }

        public bool HasPermission(string permission)
        {
            return ViewPermissions != null && ViewPermissions.ContainsKey(permission) && ViewPermissions[permission];
        }

        public static EncrypticsPrincipal CurrentEncrypticsUser
        {
            get
            {
                var encrypticsUser = Current as EncrypticsPrincipal ?? GetCurrentUser();

                return encrypticsUser;
            }
        }

        private static EncrypticsPrincipal GetCurrentUser()
        {
            var portalService = ObjectFactory.GetInstance<PortalServiceSoap>();

            return new EncrypticsPrincipal(Current.Identity, portalService);
        }

        public async Task SetViewPermissionsAsync(long entityId, IEnumerable<string> permissions)
        {
            var tokenAuth = new TokenAuth { Token = Token };
            var request = new GetUserAuthorizedActionsRequest(tokenAuth, entityId, UserId, permissions.ToArray());
            var response = await _portalService.GetUserAuthorizedActionsAsync(request);

            if (response.TokenAuth.Status != TokenStatus.Succes || response.GetUserAuthorizedActionsResult == null)
                return;

            var viewPermissions =
                response.GetUserAuthorizedActionsResult.Select(t => new { t.Action, t.IsAuthorized })
                        .ToDictionary(r => r.Action, r => r.IsAuthorized);

            ViewPermissions = ViewPermissions.Concat(viewPermissions).ToDictionary(x => x.Key, y => y.Value);
        }

        public async Task SetViewPermissionsAsync(IEnumerable<string> permissions)
        {
            await SetViewPermissionsAsync(EntityId, permissions);
        }

        public void SetViewPermissions(long entityId, IEnumerable<string> permissions)
        {
            var permissionList = permissions as List<string> ?? permissions.ToList();

            permissionList.RemoveAll(s => ViewPermissions.Keys.Contains(s));

            if (!permissionList.Any()) return;

            var tokenAuth = new TokenAuth { Token = Token };
            var request = new GetUserAuthorizedActionsRequest(tokenAuth, entityId, UserId, permissionList.ToArray());

            var response = _portalService.GetUserAuthorizedActions(request);

            if (response.TokenAuth.Status != TokenStatus.Succes || response.GetUserAuthorizedActionsResult == null) return;

            var viewPermissions =
                response.GetUserAuthorizedActionsResult.Select(t => new { t.Action, t.IsAuthorized })
                        .ToDictionary(r => r.Action, r => r.IsAuthorized);

            ViewPermissions = ViewPermissions.Concat(viewPermissions).ToDictionary(x => x.Key, y => y.Value);
        }

        public void SetViewPermissions(IEnumerable<string> permissions)
        {
            SetViewPermissions(EntityId, permissions);
        }
    }
}