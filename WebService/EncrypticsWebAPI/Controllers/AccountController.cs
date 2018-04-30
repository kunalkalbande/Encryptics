using System.Web.Http;
using System.Linq;
using System.Collections.Generic;
using ServicesCommon;
using PortalCommon;
using PortalServer;
using EncrypticsWebAPI.Models;
using System.Threading.Tasks;

namespace EncrypticsWebAPI.Controllers
{
    [RoutePrefix("accounts")]
    public class AccountController : EncrypticsApiController
    {
        [HttpGet]
        [Route("{entityId:long:min(0)}/{userId:long:min(0)}/details")]
        public IHttpActionResult GetAccountDetails(long entityId, long userId)
        {
            if (Request.Headers.Contains(TokenAuth.TokenAuth_ID) == false)
                return BadRequest();

            TokenAuth token = new TokenAuth { Token = Request.Headers.GetValues(TokenAuth.TokenAuth_ID).First() };

            UserAccount response = new PortalServerAPI().HandleGetAccountDetails(ref token, entityId, userId);

            return BuildResponse(response, token);
        }

        [HttpPost]
        [Route("{userId:long:min(0)}/authorizedaction/{entityId:long:min(0)}")]
        public IHttpActionResult GetUserAuthorizedAction(long userId, long entityId, [FromBody] string action)
        {
            if (Request.Headers.Contains(TokenAuth.TokenAuth_ID) == false)
                return BadRequest();

            TokenAuth token = new TokenAuth { Token = Request.Headers.GetValues(TokenAuth.TokenAuth_ID).First() };

            bool response = new PortalServerAPI().HandleGetUserAuthorizedAction(ref token, entityId, userId, action);

            return BuildResponse(response, token);
        }

        [HttpPost]
        [Route("{userId:long:min(0)}/authorizedactions/{entityId:long:min(0)}")]
        public IHttpActionResult GetUserAuthorizedActions(long userId, long entityId, [FromBody] string[] actions)
        {
            if (Request.Headers.Contains(TokenAuth.TokenAuth_ID) == false)
                return BadRequest();

            TokenAuth token = new TokenAuth { Token = Request.Headers.GetValues(TokenAuth.TokenAuth_ID).First() };

            List<AuthorizedAction> response = new PortalServerAPI().HandleGetUserAuthorizedActions(ref token, entityId, userId, actions);

            return BuildResponse(response, token);
        }
        [HttpPost]
        [Route("{userId:long:min(0)}/updateusercontactinfo/{firstName}/{lastName}")]
        public IHttpActionResult UpdateUserContactInfo(long userId, string firstName, string lastName, [FromBody]ContactInfo userContactInfo)
        { 
            if (Request.Headers.Contains(TokenAuth.TokenAuth_ID) == false)
                return BadRequest();

            TokenAuth token = new TokenAuth { Token = Request.Headers.GetValues(TokenAuth.TokenAuth_ID).First() };

            bool response = new PortalServerAPI().HandleUpdateUserContactInfo(ref token, userId,firstName,lastName, userContactInfo);

            return BuildResponse(response, token);
        }
        [HttpGet]
        [Route("{userId:long:min(0)}/companies/{companyStatus:int:min(0)}")]
        public IHttpActionResult GetUserCompanies(long userId, int companyStatus = 0)
        {
            if (Request.Headers.Contains(TokenAuth.TokenAuth_ID) == false)
                return BadRequest();

            TokenAuth token = new TokenAuth { Token = Request.Headers.GetValues(TokenAuth.TokenAuth_ID).First() };

            List<CompanyListItem> response = new PortalServerAPI().HandleGetUserCompanies(ref token, userId, (CompanyStatus)companyStatus);

            return BuildResponse(response, token);
        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login([FromBody]LoginModel model)
        {
            ServicesCommonDA.LogAppRecord("AccountController", "Login", "1");

            TokenAuth token = ParseTokenHeaders(Request.Headers, true);

            ServicesCommonDA.LogAppRecord("AccountController", "Login", "2");

            UserAccount response = new PortalServerAPI().HandleUserLogin(ref token, model.AccountName, model.Password);

            ServicesCommonDA.LogAppRecord("AccountController", "Login", "3");

            return BuildResponse(response, token);
        }

        [HttpPut]
        [Route("{entityId:long:min(0)}/{userId:long:min(0)}/company/{newEntityId:long:min(0)}/{transferLicenses?}")]
        public IHttpActionResult UpdateUserCompany(long entityId, long userId, long newEntityId, bool transferLicenses = false)
        {
            if (Request.Headers.Contains(TokenAuth.TokenAuth_ID) == false)
                return BadRequest();

            TokenAuth token = new TokenAuth { Token = Request.Headers.GetValues(TokenAuth.TokenAuth_ID).First() };

            bool response = new PortalServerAPI().HandleUpdateUserCompany(ref token, entityId, userId, newEntityId, transferLicenses);

            return BuildResponse(response, token);
        }

        [HttpPost]
        [Route("{user_id:long:min(0)}/tokens/validate")]
        public IHttpActionResult ValidateToken(long user_id)
        {
            TokenAuth token = ParseTokenHeaders(Request.Headers, false);

            bool response = new PortalServerAPI().HandleValidateToken(ref token, user_id);

            return BuildResponse(response, token);
        }

        [HttpPost]
        [Route("isTenantAvailable")]
        public IHttpActionResult IsTenantAvailable([FromBody]LoginModel model)
        {
            return base.Ok(Task.Run(() => {
                string tenant = new PortalServerAPI().GetTenant(model.AccountName);
                return tenant;
              
            }));
           
        }

        [HttpPost]
        [Route("getUserIdentifier")]
        public IHttpActionResult GetUserIdentifier([FromBody]LoginModel model)
        {
            ServicesCommonDA.LogAppRecord("AccountController", "Login", "1");

            TokenAuth token = ParseTokenHeaders(Request.Headers, true);
            UserAccount response = new PortalServerAPI().HandleUserLogin(ref token, model.AccountName);

            return BuildResponse(response, token);
        }


    }
}
