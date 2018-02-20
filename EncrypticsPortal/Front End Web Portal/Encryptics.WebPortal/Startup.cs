﻿using System;
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
        private static readonly string _azureClientId = "f46f44d3-f950-459d-8cd7-50b0a30da48c";
        private static readonly string _azureClientSecret = "6vap2Qt2mU5AmD7RxMWt3xV5a50QzhhYxkZkXs6LFbw=";
        private static readonly string _azureTenantId = "6bdbcf21-53c5-4629-82a6-7ae4a487a821";
        public static string AzureClientId { get { return _azureClientId; } }
        public static string AzureClientSecret { get { return _azureClientSecret; } }
        public static string AzureTenantId { get { return _azureTenantId; } }
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
                        Wtrealm = "https://localhost:44356/",
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
                     // AuthenticateRequestSigningBehavior = SigningBehavior.Never,// or add a signing certificate
                     //  Wtrealm = "https://synpreshitoutlook075.onmicrosoft.com/8ceb8256-b74e-4ae6-bd16-dd6de975dedf",
                     //  MetadataAddress = "https://login.microsoftonline.com/d9b0ee78-8419-4455-b52c-64e4cb2e453f/FederationMetadata/2007-06/FederationMetadata.xml",
                     Wtrealm = "https://tanvikansarasynerzip.onmicrosoft.com/1f02f5c0-b981-43a2-808b-c2920ecc1aa9",// "https://synpreshitoutlook075.onmicrosoft.com/8ceb8256-b74e-4ae6-bd16-dd6de975dedf",
                     MetadataAddress = "https://login.microsoftonline.com/6bdbcf21-53c5-4629-82a6-7ae4a487a821/federationmetadata/2007-06/federationmetadata.xml",//"https://login.microsoftonline.com/d9b0ee78-8419-4455-b52c-64e4cb2e453f/FederationMetadata/2007-06/FederationMetadata.xml",
                     // BackchannelCertificateValidator = null,

                     //   Wreply = "https://localhost:44365/Home/Authenticate/",
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
