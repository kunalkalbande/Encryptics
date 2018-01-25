//using Microsoft.Web.WebPages.OAuth;

namespace Encryptics.WebPortal
{
    public static class AuthConfig
    {
        public static string UserName;
        public static bool LogOut = false;
        public static string AuthType;
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            //OAuthWebSecurity.RegisterFacebookClient(
            //    appId: "662554360451203",
            //    appSecret: "e4971660fb5b3ff8550b32f4e07b06cf");

            //OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
