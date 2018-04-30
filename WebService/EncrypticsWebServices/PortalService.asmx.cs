using System;
using System.Collections.Generic;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Script.Services;
using PortalServer;
using PortalCommon;
using ServicesCommon;

namespace EncrypticsWebServices
{
    /// <summary>
    /// Summary description for PortalService
    /// </summary>
    [WebService(Namespace = "http://www.encryptics.net/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]
    public class PortalService : WebService
    {
        public TokenAuth TokenHeader;

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [SoapHeader("TokenHeader", Direction = SoapHeaderDirection.InOut)]
        public bool GetUserAuthorizedAction(long entity_id, long user_id, string action)
        {
            return new PortalServerAPI().HandleGetUserAuthorizedAction(ref TokenHeader, entity_id, user_id, action);
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [SoapHeader("TokenHeader", Direction = SoapHeaderDirection.InOut)]
        public List<AuthorizedAction> GetUserAuthorizedActions(long entity_id, long user_id, string[] actions)
        {
            return new PortalServerAPI().HandleGetUserAuthorizedActions(ref TokenHeader, entity_id, user_id, actions);
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [SoapHeader("TokenHeader", Direction = SoapHeaderDirection.InOut)]
        public List<CompanyListItem> GetUserCompanies(long admin_entity_id, long admin_id, long user_id, CompanyStatus company_status)
        {
            return new PortalServerAPI().HandleGetUserCompanies(ref TokenHeader, user_id, company_status);
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [SoapHeader("TokenHeader", Direction = SoapHeaderDirection.InOut)]
        public UserAccount GetAccountDetails(long entity_id, long user_id)
        {
            return new PortalServerAPI().HandleGetAccountDetails(ref TokenHeader, entity_id, user_id);
        }
        
        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [SoapHeader("TokenHeader", Direction = SoapHeaderDirection.InOut)]
        public bool UpdateUserCompany(long entity_id, long user_id, long new_entity_id, bool transfer_licenses)
        {
            return new PortalServerAPI().HandleUpdateUserCompany(ref TokenHeader, entity_id, user_id, new_entity_id, transfer_licenses);
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [SoapHeader("TokenHeader", Direction = SoapHeaderDirection.InOut)]
        public bool UpdateUserContactInfo(long user_id, string first_name, string last_name, ContactInfo user_contact_info)
        {
            return new PortalServerAPI().HandleUpdateUserContactInfo(ref TokenHeader, user_id, first_name, last_name, user_contact_info);
        }
        
        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [SoapHeader("TokenHeader", Direction = SoapHeaderDirection.InOut)]
        public UserAccount UserLogin(string account_name, string password)
        {
            return new PortalServerAPI().HandleUserLogin(ref TokenHeader, account_name, password);
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [SoapHeader("TokenHeader", Direction = SoapHeaderDirection.InOut)]
        public bool UserLogout(long entity_id, long user_id)
        {
            return new PortalServerAPI().HandleExpireTokenSession(ref TokenHeader, entity_id, user_id);
        }
        
        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [SoapHeader("TokenHeader", Direction = SoapHeaderDirection.InOut)]
        public bool ValidateToken(long user_id)
        {
            return new PortalServerAPI().HandleValidateToken(ref TokenHeader, user_id);
        }
        
    }
}
