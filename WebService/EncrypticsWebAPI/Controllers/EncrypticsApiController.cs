using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Results;
using ServicesCommon;

namespace EncrypticsWebAPI.Controllers
{
    public class EncrypticsApiController : ApiController
    {
        [AcceptVerbs("OPTIONS")]
        public HttpResponseMessage Options()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        public ResponseMessageResult BuildResponse(object responseBytes, TokenAuth token, bool created, string locationURI)
        {
            HttpStatusCode code = Helpers.ResponseHttpStatusCode(token);
            
            if (created && code == HttpStatusCode.OK)
                code = HttpStatusCode.Created;
            
            HttpResponseMessage response = Request.CreateResponse(code, responseBytes);
            
            if (string.IsNullOrEmpty(locationURI) == false)
                response.Headers.Add("Location", locationURI);
            
            if (token != null)
            {
                if(!string.IsNullOrEmpty(token.Token))
                    response.Headers.Add(TokenAuth.TokenAuth_ID, token.Token);
                
                response.Headers.Add(TokenAuth.TokenAuth_Status, token.Status.ToString());
            }
            
            return ResponseMessage(response);
        }

        public ResponseMessageResult BuildResponse(object responseBytes, TokenAuth token)
        {
            return BuildResponse(responseBytes, token, false, null);
        }

        public ResponseMessageResult BuildCreateResponse(object responseBytes, TokenAuth token, string locationURI)
        {
            return BuildResponse(responseBytes, token, true, locationURI);
        }

        public TokenAuth ParseTokenHeaders(HttpHeaders headers)
        {
            return ParseTokenHeaders(headers, false);
        }

        public TokenAuth ParseTokenHeaders(HttpHeaders headers, bool returnDefault)
        {
            if (headers.Contains(TokenAuth.TokenAuth_ID) || headers.Contains(TokenAuth.TokenAuth_Nonce))
            {
                return new TokenAuth()
                    {
                        Token = headers.Contains(TokenAuth.TokenAuth_ID) ? Request.Headers.GetValues(TokenAuth.TokenAuth_ID).First() : null,
                        Nonce = headers.Contains(TokenAuth.TokenAuth_Nonce) ? Request.Headers.GetValues(TokenAuth.TokenAuth_Nonce).First() : null
                    };
            }

            return returnDefault ? new TokenAuth { Status = (int)TokenAuth.TokenStatus.NONE } : null;
        }
        
    }
}
