using System;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Helpers
{
    public static class AntiForgeryTokenHelper
    {
        public static string GetAntiForgeryToken(this Controller controller)
        {
            return GetAntiForgeryToken();
        }

        public static string GetAntiForgeryToken()
        {
            string cookieToken, formToken;

            AntiForgery.GetTokens(null, out cookieToken, out formToken);

            return String.Format("{0}:{1}", cookieToken, formToken);
        }
    }
}