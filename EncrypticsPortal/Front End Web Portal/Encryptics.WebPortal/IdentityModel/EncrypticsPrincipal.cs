using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Security;
using StructureMap;
using System.Net;
using System;
using Newtonsoft.Json;

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
            var cli = new WebClient();
            string jsonstring = JsonConvert.SerializeObject(permissions.ToArray());
            cli.Headers.Add("TokenAuth_ID", tokenAuth.Token);
            cli.Headers[HttpRequestHeader.ContentType] = "application/json";
            string url = String.Format("http://idtp376/EncrypticsWebAPI/v2/accounts/{0}/authorizedactions/{1}", UserId, entityId);
            List<AuthorizedAction> actions = null;
            try
            {
                actions = JsonConvert.DeserializeObject<List<AuthorizedAction>>(cli.UploadString(url, jsonstring));
                tokenAuth = new TokenAuth();
                WebHeaderCollection myWebHeaderCollection = cli.ResponseHeaders;
                if (myWebHeaderCollection.GetValues("tokenauth_id") != null)
                {
                    tokenAuth.Token = myWebHeaderCollection.GetValues("tokenauth_id")[0];
                    tokenAuth.Status = Convert.ToInt32(myWebHeaderCollection.GetValues("tokenauth_status")[0]);
                }
                GetUserAuthorizedActionsResponse response = new GetUserAuthorizedActionsResponse() { GetUserAuthorizedActionsResult = actions.ToArray(), TokenAuth = tokenAuth };
                //var request = new GetUserAuthorizedActionsRequest(tokenAuth, entityId, UserId, permissions.ToArray());
                //var response = await _portalService.GetUserAuthorizedActionsAsync(request);

                if (response.TokenAuth.Status != TokenStatus.Succes || response.GetUserAuthorizedActionsResult == null)
                    return;

                var viewPermissions =
                    response.GetUserAuthorizedActionsResult.Select(t => new { t.Action, t.IsAuthorized })
                            .ToDictionary(r => r.Action, r => r.IsAuthorized);

                ViewPermissions = ViewPermissions.Concat(viewPermissions).ToDictionary(x => x.Key, y => y.Value);
            }
            catch (Exception ex)
            {

            }
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
            //var request = new GetUserAuthorizedActionsRequest(tokenAuth, entityId, UserId, permissionList.ToArray());
            if (UserId == 0) return;
            //var response = _portalService.GetUserAuthorizedActions(request);
            var cli = new WebClient();
            string jsonstring = JsonConvert.SerializeObject(permissionList.ToArray());
            cli.Headers.Add("TokenAuth_ID", tokenAuth.Token);
            cli.Headers[HttpRequestHeader.ContentType] = "application/json";
            string url = String.Format("http://idtp376/EncrypticsWebAPI/v2/accounts/{0}/authorizedactions/{1}", UserId, entityId);
            List<AuthorizedAction> actions = null;

            actions = JsonConvert.DeserializeObject<List<AuthorizedAction>>(cli.UploadString(url, jsonstring));
            tokenAuth = new TokenAuth();
            WebHeaderCollection myWebHeaderCollection = cli.ResponseHeaders;
            if (myWebHeaderCollection.GetValues("tokenauth_id") != null)
            {
                tokenAuth.Token = myWebHeaderCollection.GetValues("tokenauth_id")[0];
                tokenAuth.Status = Convert.ToInt32(myWebHeaderCollection.GetValues("tokenauth_status")[0]);
            }
            GetUserAuthorizedActionsResponse response = new GetUserAuthorizedActionsResponse() { GetUserAuthorizedActionsResult = actions.ToArray(), TokenAuth = tokenAuth };
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