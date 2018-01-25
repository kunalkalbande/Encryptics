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
using Microsoft.Owin.Security.Google;

[assembly: OwinStartup(typeof(Encryptics.WebPortal.Startup))]

namespace Encryptics.WebPortal
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            try
            {


                //  app.UseActiveDirectoryFederationServicesBearerAuthentication(new Microsoft.Owin.Security.ActiveDirectory.ActiveDirectoryFederationServicesBearerAuthenticationOptions() { Audience = realm, MetadataEndpoint = adfsMetadata });
                app.UseWsFederationAuthentication(
                    new WsFederationAuthenticationOptions
                    {
                        // Caption = "adfs",
                        MetadataAddress = "https://172.25.29.30/federationmetadata/2007-06/federationmetadata.xml",
                        Wtrealm = "https://idtp376/Home/adfsSignin",
                        BackchannelCertificateValidator = new CertificateValidator(),
                        AuthenticationType = "adfs",
                        CallbackPath = new PathString("/"),
                        // AuthenticationMode = AuthenticationMode.Passive,
                        Notifications = new WsFederationAuthenticationNotifications
                        {
                            RedirectToIdentityProvider=n=>
                            {
                                string user = AuthConfig.UserName;
                                AuthConfig.UserName = string.Empty;
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
                     // AuthenticateRequestSigningBehavior = SigningBehavior.Never,// or add a signing certificate
                     Wtrealm = "https://synpreshitoutlook075.onmicrosoft.com/8ceb8256-b74e-4ae6-bd16-dd6de975dedf",
                     MetadataAddress = "https://login.microsoftonline.com/d9b0ee78-8419-4455-b52c-64e4cb2e453f/FederationMetadata/2007-06/FederationMetadata.xml",
                     // BackchannelCertificateValidator = null,

                     //   Wreply = "https://localhost:44365/Home/Authenticate/",
                     AuthenticationType = "azure",
                     CallbackPath = new PathString("/Home/SignOutCallback"),

                     // AuthenticationMode = AuthenticationMode.Passive,
                     Notifications = new WsFederationAuthenticationNotifications
                     {
                         RedirectToIdentityProvider = n =>
                         {
                             string user = AuthConfig.UserName;
                             AuthConfig.UserName = string.Empty;
                             n.ProtocolMessage.SetParameter("login_hint", user);
                             return Task.FromResult(0);
                         },

                         SecurityTokenValidated = n =>
                         {
                             var id = n.AuthenticationTicket.Identity;
                             List<Claim> claims = new List<Claim>();
                             var name = id.Claims.ToList().Find(x => x.Type == "http://schemas.microsoft.com/identity/claims/displayname");
                             if (name != null)
                             {
                                 claims.Add(new Claim("Name", name.Value));
                             }
                             foreach (var claim in id.Claims)
                             {
                                 var type = claim.Type;
                                 claims.Add(claim);

                             }

                             return Task.FromResult(0);
                         }

                     }
                 });
                app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
                {
                    AuthenticationType = "Google",
                    CallbackPath = new PathString("/google"),
                    ClientId = "828032771696-1igqmtj83g649uf2f8t36n61roqj7c21.apps.googleusercontent.com",
                    ClientSecret = "rDRjrLFYyTE9DmGy97HmHg_J",
                   
                });
                AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            }
            catch (Exception ex)
            {

            }
        }
    }
}
