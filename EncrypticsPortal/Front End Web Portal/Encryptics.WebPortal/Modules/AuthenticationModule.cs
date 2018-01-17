using Encryptics.WebPortal.IdentityModel;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using Newtonsoft.Json;
using StructureMap;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;

namespace Encryptics.WebPortal.Modules
{
    public class AuthenticationModule : EncrypticsHttpModule, IHttpModule
    {
        private readonly PortalServiceSoap _portalService;
        public AuthenticationModule()
        {
            _portalService = ObjectFactory.GetInstance<PortalServiceSoap>();
        }

        public void Init(HttpApplication context)
        {
            Trace.TraceInformation(string.Format("Initializing {0} class.", GetType().Name));
            context.AuthenticateRequest += OnAuthenticateRequest;
            context.PostAcquireRequestState += OnPostAcquireRequestState;
        }

        public void Dispose()
        {
            Trace.TraceInformation(string.Format("Disposing {0} class.", GetType().Name));
            try
            {
                ((PortalServiceSoapClient)_portalService).Close();
            }
            catch (Exception closeException)
            {
                try
                {
                    Trace.TraceInformation("Close failed. Attempting Abort.");
                    Trace.TraceError(closeException.Message);
                    Debug.Print(closeException.StackTrace);
                    ((PortalServiceSoapClient)_portalService).Abort();
                }
                catch (Exception abortException)
                {
                    Trace.TraceInformation("Abort failed. Giving up.");
                    Trace.TraceError(abortException.Message);
                    Debug.Print(abortException.StackTrace);
                }
            }

            Trace.TraceInformation(string.Format("{0} disposed of.", GetType().Name));
        }

        private void OnAuthenticateRequest(object sender, EventArgs eventArgs)
        {
            Trace.TraceInformation(string.Format("{0} Beginning OnAuthenticateRequest. {0}", new string('*', 8)));
            var app = sender as HttpApplication;

            if (app == null) return;

            SetPrincipal(app);

            Trace.TraceInformation(string.Format("{0} Exiting OnAuthenticateRequest. {0}", new string('*', 8)));
        }

        private static void OnPostAcquireRequestState(object sender, EventArgs eventArgs)
        {
            Trace.TraceInformation(string.Format("{0} Beginning OnPostAcquireRequestState. {0}", new string('*', 8)));
            var app = sender as HttpApplication;
            if (app != null)
                Trace.TraceInformation(string.Format("Session Available: {0}", app.Context.Session != null));

            if (app != null && app.Context.Session != null)
            {
                if (app.Context.Session.Keys.Count == 0 && app.User.Identity.IsAuthenticated)
                {
                    if (app.User is EncrypticsPrincipal)
                    {
                        var user = app.User as EncrypticsPrincipal;
                        app.Session["UserId"] = user.UserId;
                        app.Session["EntityId"] = user.EntityId;
                        app.Session["token"] = user.Token;
                        app.Session["auth"] = user.Auth;
                        app.Session["UserName"] = user.UserName;
                    }
                    //else
                    //{
                    //    Trace.TraceWarning("Forms Authentication succesful, but, session not available. Bugging out.");
                    //    SignUserOut(app);                        
                    //}
                }
                Trace.TraceInformation(string.Format("Session keys: {0}", app.Context.Session.Keys.Count));
            }
            Trace.TraceInformation(string.Format("{0} Exiting OnAuthenticateRequest. {0}", new string('*', 8)));
        }

        private void SetPrincipal(HttpApplication app)
        {
            if (!app.Request.Cookies.AllKeys.Contains(FormsAuthentication.FormsCookieName))
            {
                Trace.TraceWarning("Forms Authentication cookie not in request cookies.");

                TrySetTemporaryPrinicipal(app);
            }
            else
            {
                HttpCookie httpCookie = app.Request.Cookies[FormsAuthentication.FormsCookieName];

                Debug.Print("Forms Authentication cookie: {0}.",
                            httpCookie != null ? (object)(httpCookie.Value) : "is null");
            }

            try
            {
                if (app.Context.User != null)
                {
                    SetEncrypticsPrincipal(app);
                }
                else
                {
                    SignUserOut(app);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format("Exception caught during forms authentication decryption: {0}", e.Message));
                Debug.Print(e.StackTrace);
                SignUserOut(app);
            }
        }

        private void SetEncrypticsPrincipal(HttpApplication app)
        {
            string userName = app.Context.User.Identity.Name;

            if (string.IsNullOrEmpty(userName)) return;

            dynamic userData = GetUserData(app.User.Identity);
            var tokenAuth = new TokenAuth { Token = userData.Token };
            var request = new ValidateTokenRequest(tokenAuth,
                                                   (long)userData.UserId);
            //   ValidateTokenResponse response = _portalService.ValidateToken(request);
            var cli = new WebClient();
            string url = String.Format("http://idtp376/EncrypticsWebAPI/v2/accounts/{0}/tokens/validate", userData.UserId);
            cli.Headers.Add("tokenauth_id", tokenAuth.Token);
            cli.Headers[HttpRequestHeader.ContentType] = "application/json";
            ValidateTokenResponse response = null;
            try
            {
                var result = JsonConvert.DeserializeObject<bool>(cli.UploadString(url,string.Empty));
                 tokenAuth = new TokenAuth();
                WebHeaderCollection myWebHeaderCollection = cli.ResponseHeaders;
                if (myWebHeaderCollection.GetValues("tokenauth_id") != null)
                {
                    tokenAuth.Token = myWebHeaderCollection.GetValues("tokenauth_id")[0];
                    tokenAuth.Status = Convert.ToInt32(myWebHeaderCollection.GetValues("tokenauth_status")[0]);
                }
                response = new ValidateTokenResponse(tokenAuth, result);

                

                Trace.TraceInformation(string.Format("Logged in as {0}", userName));

                if (!response.ValidateTokenResult || response.TokenAuth.Status != TokenStatus.Succes)
                {
                    Trace.TraceWarning("Token validation failed. Bugging out.");

                    Debug.Print("Token status: {0}",
                                (object)TokenStatus.ErrorMessageDictionary[response.TokenAuth.Status]);

                    SignUserOut(app);
                }
                else
                {
                    Trace.TraceInformation("Creating Encrytpics Principal.");
                    var encrypticsPrincipal = new EncrypticsPrincipal(app.User.Identity, _portalService)
                    {
                        Token = userData.Token,
                        EntityId = userData.EntityId,
                        UserId = userData.UserId,
                        Auth = userData.AuthenticationType,
                        UserName = userName
                    };

                    // TODO: Can probably get rid of the Roles functionality now...
                    //string roleName = userData.Role;
                    //var otherRoles = Roles.GetRolesForUser(userName).ToList().Except(new[] { roleName }).ToArray();
                    //if (otherRoles.Any())
                    //{
                    //    Roles.RemoveUserFromRoles(userName, otherRoles);
                    //}
                    //if (!Roles.IsUserInRole(userName, roleName))
                    //{
                    //    Roles.AddUserToRole(userName, roleName);
                    //}

                    app.Context.User = Thread.CurrentPrincipal = encrypticsPrincipal;

                    var companiesRequest = new GetUserCompaniesRequest
                    {
                        TokenAuth = tokenAuth,
                        admin_entity_id = userData.EntityId,
                        user_id = userData.UserId
                    };

                    Trace.TraceInformation("Requesting Entity Admin Roles.");

                    var userCompaniesResponse = _portalService.GetUserCompanies(companiesRequest);

                    //********syn: remove hardcode response//
                    userCompaniesResponse = new GetUserCompaniesResponse()
                    {
                        TokenAuth = response.TokenAuth,
                        GetUserCompaniesResult = new CompanyListItem[] { new CompanyListItem { Id = 1, IsActive = true, Name = "syn", Role = RoleType.Admin } }
                    };
                    //********syn: remove hardcode response//

                    encrypticsPrincipal.CompanyCount = userCompaniesResponse.GetUserCompaniesResult.LongLength;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void TrySetTemporaryPrinicipal(HttpApplication app)
        {
            dynamic requestJson = ParseJsonRequestObject(app);

            try
            {
                string encryptedTicket = requestJson == null ? string.Empty : requestJson.ticket ?? string.Empty;

                Debug.Print("Encrypted ticket: {0}", encryptedTicket);

                if (string.IsNullOrEmpty(encryptedTicket))
                {
                    app.Context.User =
                        Thread.CurrentPrincipal = new GenericPrincipal(new LoggedOutIdentity(string.Empty), new[] { "User" });

                    return;
                }

                FormsAuthenticationTicket authenticationTicket = FormsAuthentication.Decrypt(encryptedTicket);

                if (authenticationTicket != null)
                {
                    app.Context.User =
                        Thread.CurrentPrincipal = new RolePrincipal(new FormsIdentity(authenticationTicket));
                }
            }
            catch
            {
                Trace.TraceWarning("Could not decrypt Authentication Ticket.");
            }
        }

        private static dynamic ParseJsonRequestObject(HttpApplication app)
        {
            var bytes = new byte[app.Request.InputStream.Length];
            app.Request.InputStream.Read(bytes, 0, bytes.Length);
            app.Request.InputStream.Position = 0;
            string requestInput = Encoding.ASCII.GetString(bytes);
            if (string.IsNullOrEmpty(requestInput))
            {
                Trace.TraceWarning("Nothing sent in request data.");
                if (app.Request.QueryString.Count > 0 && app.Request.QueryString.AllKeys.Contains("ticket"))
                {
                    Trace.TraceInformation("Trying with query string value.");
                    Debug.Print("Ticket QueryString value: {0}", app.Request.QueryString["ticket"]);
                    requestInput = app.Request.QueryString["ticket"];
                }
            }
            try
            {
                return JsonConvert.DeserializeObject(requestInput);
            }
            catch
            {
                Trace.TraceWarning("Could not deserialize as JSON.");
            }

            return null;
        }

        private static void SignUserOut(HttpApplication app)
        {
            if (app.User != null)
            {
                string userName = app.User.Identity.Name;
               
                FormsAuthentication.SignOut();
                //app.Session.Abandon();

                // clear authentication cookie
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "") { Expires = DateTime.Now.AddYears(-1) };
                app.Response.Cookies.Add(authCookie);

                // clear session cookie (not necessary for your current problem but i would recommend you do it anyway)
                DeleteCookie(app, "ASP.NET_SessionId");
                DeleteCookie(app, "ticket");
                DeleteCookie(app, "username");

                Thread.CurrentPrincipal =
                    app.Context.User = new GenericPrincipal(new LoggedOutIdentity(userName), new string[] { });
            }

            RedirectResponse(app);
        }

        private static dynamic GetUserData(IIdentity identity)
        {
            return JsonConvert.DeserializeObject(((FormsIdentity)identity).Ticket.UserData);
        }

        private static void DeleteCookie(HttpApplication app, string cookieName)
        {
            if (app.Request.Cookies[cookieName] == null) return;
            var myCookie = new HttpCookie(cookieName, string.Empty) { Expires = DateTime.Now.AddDays(-1d) };
            app.Response.Cookies.Add(myCookie);
        }
    }
}