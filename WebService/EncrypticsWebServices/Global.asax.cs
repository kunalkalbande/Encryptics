using System;

namespace EncrypticsWebServices{
	/// <summary>
	/// Summary description for Global.
	/// </summary>
	public class Global :
		System.Web.HttpApplication {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		public Global() {
			InitializeComponent();
		}	
		
		protected void Application_Start(Object sender, EventArgs e)
        {
		}
 
		protected void Session_Start(Object sender, EventArgs e) {

		}

		protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            //if (!Request.IsLocal)
            //{
            //    switch (Request.Url.Scheme)
            //    {
            //        case "https":
            //            int hsts_lifespan = 300;
            //            if (System.Web.Configuration.WebConfigurationManager.AppSettings["HSTS"] != null)
            //            {
            //                if (int.TryParse(System.Web.Configuration.WebConfigurationManager.AppSettings["HSTS"], out hsts_lifespan) != true)
            //                    hsts_lifespan = 300;
            //            }
            //            Response.Headers.Remove("Strict-Transport-Security");
            //            Response.AddHeader("Strict-Transport-Security", "max-age=" + hsts_lifespan.ToString() + "; includeSubDomains");
            //            break;
            //        case "http":
            //            var path = "https://" + Request.Url.Host + Request.Url.PathAndQuery;
            //            Response.Status = "301 Moved Permanently";
            //            Response.AddHeader("Location", path);
            //            break;
            //    }
            //}
        }

		protected void Application_EndRequest(Object sender, EventArgs e) {

		}

		protected void Application_AuthenticateRequest(Object sender, EventArgs e) {

		}

		protected void Application_Error(Object sender, EventArgs e) {

		}

		protected void Session_End(Object sender, EventArgs e) {

		}

		protected void Application_End(Object sender, EventArgs e) {

		}

        protected void Application_PreSendRequestHeaders()
        {
            UpdateHeaders();
        }

        private void UpdateHeaders()
        {
            Response.Headers.Remove("Server");
            Response.Headers.Remove("X-AspNet-Version");
            Response.Headers.Remove("X-AspNetMvc-Version");
            Response.Headers.Remove("X-Powered-By");
            Response.Headers.Remove("Pragma");
            Response.Headers.Remove("Expires");
            
            Response.AppendHeader("Pragma", "no-cache"); // HTTP 1.0.
            Response.AppendHeader("Expires", "0"); // Proxies.

            Response.Cache.SetCacheability(System.Web.HttpCacheability.ServerAndNoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetRevalidation(System.Web.HttpCacheRevalidation.AllCaches);
        }

        #region Web Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {    
			this.components = new System.ComponentModel.Container();
		}
		#endregion
	}
}

