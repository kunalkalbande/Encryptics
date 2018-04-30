using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.WsFederation;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Helpers;
using System.Configuration;
using Microsoft.Owin.Security.Google;

[assembly: OwinStartup(typeof(Encryptics.WebPortal.Startup))]

namespace Encryptics.WebPortal
{
    public class Startup
    {
      
        #region Identity Provider details
        private static readonly string _azureClientId = ConfigurationManager.AppSettings["azureClientId"];
        private static readonly string _azureClientSecret = ConfigurationManager.AppSettings["azureClientSecret"];
        private static readonly string _azureTenantId = ConfigurationManager.AppSettings["azureTenantId"];
        private string adfsMetadataAddress = ConfigurationManager.AppSettings["adfsMetadataAddress"];
        private string adfsWtrelam = ConfigurationManager.AppSettings["adfsWtrelam"];
        private string adfsMetadataAddress1 = ConfigurationManager.AppSettings["adfsMetadataAddress1"];
        private string adfsWtrelam1 = ConfigurationManager.AppSettings["adfsWtrelam1"];
        private string azureMetadataAddress= ConfigurationManager.AppSettings["azureMetadataAddress"];
        private string azureWtrelam = ConfigurationManager.AppSettings["azureWtrelam"];
        private string googleClientId = ConfigurationManager.AppSettings["googleClientId"];
        private string googleClientSecret = ConfigurationManager.AppSettings["googleClientSecret"];
        #endregion

        #region public property
        public static string AzureClientId { get { return _azureClientId; } }
        public static string AzureClientSecret { get { return _azureClientSecret; } }
        public static string AzureTenantId { get { return _azureTenantId; } }
        #endregion

       
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            try
            {
                    app.UseWsFederationAuthentication(
                    new WsFederationAuthenticationOptions
                    {
                        // Caption = "adfs",
                        MetadataAddress = adfsMetadataAddress,
                        Wtrealm =adfsWtrelam,
                        BackchannelCertificateValidator = new CertificateValidator(),
                        AuthenticationType = "adfs",
                        CallbackPath = new PathString("/"),
                        AuthenticationMode = AuthenticationMode.Passive,
                       
                        Notifications = new WsFederationAuthenticationNotifications
                        {
                            RedirectToIdentityProvider = n =>
                              {
                                  var userName = n.OwinContext.Response.Headers["UserName"];
                                  string user = userName;
                                  n.ProtocolMessage.SetParameter("login_hint", user);
                                  n.ProtocolMessage.SetParameter("AlwaysRequireAuthentication", "true");
                                  return Task.FromResult(0);
                              },
                            SecurityTokenValidated = n =>
                            {

                                System.Diagnostics.Trace.TraceInformation("Login successful.");
                                var id = n.AuthenticationTicket.Identity;
                                List<Claim> claims = new List<Claim>();
                                foreach (var claim in id.Claims)
                                {
                                    claims.Add(claim);
                                }
                                return Task.FromResult(0);
                            }

                        }
                    });
                app.UseWsFederationAuthentication(
                    new WsFederationAuthenticationOptions
                    {
                        // Caption = "adfs",
                        MetadataAddress = adfsMetadataAddress1,
                        Wtrealm = adfsWtrelam1,
                        BackchannelCertificateValidator = new CertificateValidator(),
                        AuthenticationType = "adfs1",
                        CallbackPath = new PathString("/Logout.aspx"),
                        AuthenticationMode = AuthenticationMode.Passive,
                        Notifications = new WsFederationAuthenticationNotifications
                        {
                            RedirectToIdentityProvider = n =>
                            {

                                var userName = n.OwinContext.Response.Headers["UserName"];
                                string user = userName;
                                n.ProtocolMessage.SetParameter("login_hint", user);
                                n.ProtocolMessage.SetParameter("AlwaysRequireAuthentication", "true");
                                return Task.FromResult(0);
                            },
                            SecurityTokenValidated = n =>
                            {

                                System.Diagnostics.Trace.TraceInformation("Login successful.");
                                var id = n.AuthenticationTicket.Identity;
                                List<Claim> claims = new List<Claim>();
                                foreach (var claim in id.Claims)
                                {
                                    claims.Add(claim);
                                }
                                return Task.FromResult(0);
                            }

                        }
                    });
                app.UseWsFederationAuthentication(
                 new WsFederationAuthenticationOptions
                 {
                    
                     Wtrealm = azureWtrelam,
                     MetadataAddress = azureMetadataAddress,
                     AuthenticationType = "azure",
                     CallbackPath = new PathString("/Home/SignOutCallback"),

                     // AuthenticationMode = AuthenticationMode.Passive,
                     Notifications = new WsFederationAuthenticationNotifications
                     {
                         RedirectToIdentityProvider = n =>
                         {
                             var userName = n.OwinContext.Response.Headers["UserName"];
                             string user = userName;
                             n.ProtocolMessage.SetParameter("login_hint", user);
                             return Task.FromResult(0);
                         },

                         SecurityTokenValidated = n =>
                         {
                             var id = n.AuthenticationTicket.Identity;
                             List<Claim> claims = new List<Claim>();
                             foreach (var claim in id.Claims)
                             {
                                 var type = claim.Type;
                                 claims.Add(claim);
                             }
                             return Task.FromResult(0);
                         }

                     }
                 });
                var google = new GoogleOAuth2AuthenticationOptions
                {
                    Provider = new GoogleOAuth2AuthenticationProvider
                    {
                        OnApplyRedirect = n =>
                        {
                            var userName = n.OwinContext.Response.Headers["UserName"];
                            string user = userName;
                            var uri = n.RedirectUri;
                            n.Response.Redirect(string.Format(uri + "&login_hint={0}", user));
                        }
                    },
                    AuthenticationType = "Google",
                    CallbackPath = new PathString("/google"),
                    ClientId = googleClientId,
                    ClientSecret = googleClientSecret,


                };
                google.Scope.Add("profile");
                app.UseGoogleAuthentication(google);

                AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            }
            catch (Exception ex)
            {

            }
        }
    }
}
