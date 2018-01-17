
using System.Web.Services.Protocols;

namespace ServicesCommon
{
    public class TokenAuth : SoapHeader
    {
        public const string TokenAuth_ID = "TokenAuth_ID";
        public const string TokenAuth_Status = "TokenAuth_Status";
        public const string TokenAuth_Nonce = "TokenAuth_Nonce";

        public string Token = "";
        public int Status;
        public string Nonce;

        public TokenAuth()
        { }

        public TokenAuth(TokenStatus status)
        {
            SetStatus(status);
        }

        public void SetStatus(TokenStatus status)
        {
            Status = (int)status;
        }

        public TokenStatus GetStatus()
        {
            return (TokenStatus)Status;
        }

        public enum TokenStatus
        {
            NONE = 0,
            SUCCESS = 100,
            SUCCESS_WITH_NEW_TOKEN = 101,
            FAILED_UNKNOWN = 200,
            FAILED_WITH_EXCEPTION = 201,
            FAILED_EXPIRED = 202,
            FAILED_MISSING_TOKEN = 203,
            FAILED_BAD_TOKEN = 204,
            FAILED_BAD_HWID = 205,
            FAILED_BAD_LOCATION = 206,
            FAILED_BAD_REQUEST = 207,
            FAILED_EXPIRED_BY_USER = 208,
            FAILED_BAD_KEYS = 209,
            FAILED_BAD_NONCE = 210
        }

    }

    public class TokenValidation
    {
        public static bool ValidateToken(ref TokenAuth header)
        {
            if (header == null || header.Token == null || header.Token == string.Empty)
            {
                ServicesCommonDA.LogAppRecord("LicensingService", "ValidateToken", "header == null || header.Token == null || header.Token == string.Empty");
                header = new TokenAuth(TokenAuth.TokenStatus.FAILED_MISSING_TOKEN);
                return false;
            }

            if (header.Token.Length != 36)
            {
                ServicesCommonDA.LogAppRecord("LicensingService", "ValidateToken", "header.Token.Length != 36");
                header = new TokenAuth(TokenAuth.TokenStatus.FAILED_BAD_TOKEN);
                return false;
            }

            return true;
        }

        public static bool ValidateToken(ref TokenAuth header, string calledFrom)
        {
            if (header == null || string.IsNullOrEmpty(header.Token))
            {
                //ServicesCommonDA.LogAppRecord("LicensingService", "ValidateToken:" + calledFrom, "header == null || header.Token == null || header.Token == string.Empty");
                header = new TokenAuth(TokenAuth.TokenStatus.FAILED_MISSING_TOKEN);
                return false;
            }

            if (header.Token.Length != 36)
            {
                ServicesCommonDA.LogAppRecord("LicensingService", "ValidateToken:" + calledFrom, "header.Token.Length != 36");
                header = new TokenAuth(TokenAuth.TokenStatus.FAILED_BAD_TOKEN);
                return false;
            }

            header.SetStatus(TokenAuth.TokenStatus.NONE);

            return true;
        }

        public static bool ValidateNonce(ref TokenAuth header, string serviceName)
        {
            if (string.IsNullOrEmpty(header.Nonce) == false)
            {
                if (ServicesCommonDA.IsExistingServiceRequest(serviceName, header.Token, header.Nonce))
                {
                    header = new TokenAuth(TokenAuth.TokenStatus.FAILED_BAD_NONCE);
                    return false;
                }

                ServicesCommonDA.InsertServiceRequest(serviceName, header.Token, header.Nonce);
            }

            return true;
        }

        public static bool ValidateNonce(ref TokenAuth header, string identifier, string serviceName)
        {
            if (string.IsNullOrEmpty(header.Nonce) == false)
            {
                if (ServicesCommonDA.IsExistingServiceRequest(serviceName, identifier, header.Nonce))
                {
                    header = new TokenAuth(TokenAuth.TokenStatus.FAILED_BAD_NONCE);
                    return false;
                }

                ServicesCommonDA.InsertServiceRequest(serviceName, identifier, header.Nonce);
            }

            return true;
        }
    }
}
