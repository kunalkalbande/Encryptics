using System;
using System.Net;
using ServicesCommon;

namespace EncrypticsWebAPI
{
    public class Helpers
    {
        public static HttpStatusCode ResponseHttpStatusCode(TokenAuth token)
        {
            if (token == null)
                return HttpStatusCode.Unauthorized;

            switch (token.GetStatus())
            {
                case TokenAuth.TokenStatus.NONE:
                    return HttpStatusCode.OK;
                case TokenAuth.TokenStatus.SUCCESS:
                    return HttpStatusCode.OK;
                case TokenAuth.TokenStatus.SUCCESS_WITH_NEW_TOKEN:
                    return HttpStatusCode.OK;
                case TokenAuth.TokenStatus.FAILED_UNKNOWN:
                    return HttpStatusCode.BadRequest;
                case TokenAuth.TokenStatus.FAILED_WITH_EXCEPTION:
                    return HttpStatusCode.InternalServerError;
                case TokenAuth.TokenStatus.FAILED_EXPIRED:
                    return HttpStatusCode.BadRequest;
                case TokenAuth.TokenStatus.FAILED_MISSING_TOKEN:
                    return HttpStatusCode.BadRequest;
                case TokenAuth.TokenStatus.FAILED_BAD_TOKEN:
                    return HttpStatusCode.BadRequest;
                case TokenAuth.TokenStatus.FAILED_BAD_HWID:
                    return HttpStatusCode.BadRequest;
                case TokenAuth.TokenStatus.FAILED_BAD_LOCATION:
                    return HttpStatusCode.BadRequest;
                case TokenAuth.TokenStatus.FAILED_BAD_REQUEST:
                    return HttpStatusCode.BadRequest;
                case TokenAuth.TokenStatus.FAILED_EXPIRED_BY_USER:
                    return HttpStatusCode.BadRequest;
                case TokenAuth.TokenStatus.FAILED_BAD_KEYS:
                    return HttpStatusCode.BadRequest;
                case TokenAuth.TokenStatus.FAILED_BAD_NONCE:
                    return HttpStatusCode.BadRequest;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}