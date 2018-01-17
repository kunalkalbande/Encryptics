using System.Web.Http;
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
                //if(string.IsNullOrEmpty(tenant))
                //{
                //    return false;
                //}
                //else
                //{
                //    return true;
                //}
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
