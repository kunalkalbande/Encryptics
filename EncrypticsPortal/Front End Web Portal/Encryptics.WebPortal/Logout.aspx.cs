using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Encryptics.WebPortal
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string host =  Request.Url.Host+":" + Request.Url.Port;

            var callbackUrl = "https://"+host+"/Home/SignOutCallback";
            Response.Redirect(callbackUrl);
        }
    }
}