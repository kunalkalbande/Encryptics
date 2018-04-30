using System;
using System.Collections.Generic;
using PortalCommon;
using PortalServerDataAccess;
using ServicesCommon;

namespace PortalServer
{
    public class PortalServerAPI
    {
        public bool HandleExpireTokenSession(ref TokenAuth header, long entityId, long userId)
        {
            if (ValidateHeaders("HandleExpireTokenSession", ref header) == false)
                return false;

            PortalServerDA da = new PortalServerDA();

            bool isSuccess = false;

            try
            {
                UserIdentifiers userIdents = da.GetUserIdentifiers(header.Token);

                if (userIdents != null)
                    isSuccess = da.ExpireTokenSession(userIdents.EntityId, userIdents.Id, entityId, userId, null, header.Token);
            }
            catch (Exception e)
            {
                da.LogAppRecord("PortalServerAPI", "HandleExpireTokenSession", "Exception: " + e.Message);
            }

            return isSuccess;
        }

        public UserAccount HandleGetAccountDetails(ref TokenAuth header, long entityId, long userId)
        {
            if (ValidateHeaders("HandleGetAccountDetails", ref header) == false)
                return null;

            PortalServerDA da = new PortalServerDA();

            UserAccount userAcc = new UserAccount();

            try
            {
                UserIdentifiers userIds = da.GetUserIdentifiers(header.Token);

                if (userIds != null)
                    userAcc = da.GetUserAccount(userIds.EntityId, userIds.Id, entityId, userId);
            }
            catch (Exception e)
            {
                da.LogAppRecord("PortalServerAPI", "HandleGetAccountDetails", "Exception: " + e.Message);
            }

            return userAcc;
        }

        public bool HandleGetUserAuthorizedAction(ref TokenAuth header, long entityId, long userId, string action)
        {
            if (ValidateHeaders("HandleGetUserAuthorizedAction", ref header) == false)
                return false;

            PortalServerDA da = new PortalServerDA();

            bool result = false;

            try
            {
                UserIdentifiers userIdents = da.GetUserIdentifiers(header.Token);

                if (userIdents != null)
                    result = da.GetUserAuthorizedAction(userIdents.EntityId, userIdents.Id, entityId, userId, action);
            }
            catch (Exception e)
            {
                da.LogAppRecord("PortalServerAPI", "HandleGetUserAuthorizedAction", "Exception: " + e.Message);
            }

            return result;
        }

        public List<AuthorizedAction> HandleGetUserAuthorizedActions(ref TokenAuth header, long entityId, long userId, string[] actions)
        {
            if (ValidateHeaders("HandleGetUserAuthorizedActions", ref header) == false)
                return null;

            PortalServerDA da = new PortalServerDA();

            List<AuthorizedAction> result = null;

            try
            {
                UserIdentifiers userIdents = da.GetUserIdentifiers(header.Token);

                if (userIdents != null)
                    result = da.GetUserAuthorizedActions(userIdents.EntityId, userIdents.Id, entityId, userId, actions);
            }
            catch (Exception e)
            {
                da.LogAppRecord("PortalServerAPI", "HandleGetUserAuthorizedActions", "Exception: " + e.Message);
            }

            return result;
        }

        public List<CompanyListItem> HandleGetUserCompanies(ref TokenAuth header, long userId, CompanyStatus companyStatus)
        {
            if (ValidateHeaders("HandleGetUserCompanies", ref header) == false)
                return null;

            PortalServerDA da = new PortalServerDA();

            List<CompanyListItem> enc = null;

            try
            {
                UserIdentifiers userIdents = da.GetUserIdentifiers(header.Token);

                if (userIdents != null)
                    enc = da.GetUserCompanies(userIdents.EntityId, userIdents.Id, userId, companyStatus);
            }
            catch (Exception e)
            {
                da.LogAppRecord("PortalServerAPI", "GetUserCompanies", "Exception: " + e.Message);
            }

            return enc;
        }

        public bool HandleUpdateUserCompany(ref TokenAuth header, long entityId, long userId, long newEntityId, bool transferLicenses)
        {
            if (ValidateHeaders("HandleUpdateUserCompany", ref header) == false)
                return false;

            PortalServerDA da = new PortalServerDA();

            bool isSuccess = false;

            try
            {
                UserIdentifiers userIdents = da.GetUserIdentifiers(header.Token);

                if (userIdents != null)
                    isSuccess = da.UpdateUserCompany(userIdents.EntityId, userIdents.Id, entityId, userId, newEntityId, transferLicenses);
            }
            catch (Exception e)
            {
                da.LogAppRecord("PortalServerAPI", "HandleUpdateUserCompany", "Exception: " + e.Message);
            }

            return isSuccess;
        }

        public bool HandleUpdateUserContactInfo(ref TokenAuth header, long userId, string firstName, string lastName, ContactInfo userContactInfo)
        {
            if (ValidateHeaders("HandleUpdateUserContactInfo", ref header) == false)
                return false;

            PortalServerDA da = new PortalServerDA();

            bool isSuccess = false;

            try
            {
                UserIdentifiers userIdents = da.GetUserIdentifiers(header.Token);

                if (userIdents != null)
                    isSuccess = da.UpdateUserContactInfo(userId, firstName, lastName, userContactInfo);
            }
            catch (Exception e)
            {
                da.LogAppRecord("PortalServerAPI", "HandleUpdateUserContactInfo", "Exception: " + e.Message);
            }

            return isSuccess;
        }

        public UserAccount HandleUserLogin(ref TokenAuth header, string accountName, string password)
        {
            PortalServerDA da = new PortalServerDA();

            da.LogAppRecord("PortalServerAPI", "HandleUserLogin", "1");

            if (header != null && !TokenValidation.ValidateNonce(ref header, "HandleUserLogin"))
                return null;

            da.LogAppRecord("PortalServerAPI", "HandleUserLogin", "2");

            if (string.IsNullOrEmpty(accountName) == false)
                da.LogAppRecord("PortalServerAPI", "HandleUserLogin", "accountName: " + accountName);
            else
                da.LogAppRecord("PortalServerAPI", "HandleUserLogin", "accountName is null");

            if (string.IsNullOrEmpty(password) == false)
                da.LogAppRecord("PortalServerAPI", "HandleUserLogin", "password: " + password);
            else
                da.LogAppRecord("PortalServerAPI", "HandleUserLogin", "password is null");

            UserAccount userAcc = new UserAccount();

            da.LogAppRecord("PortalServerAPI", "HandleUserLogin", "3");

            try
            {
                // SEE IF ACCOUNT IS LOCKED OUT
                if (LoginSecurity.CheckLockout(accountName))
                {
                    da.LogAppRecord("PortalServerAPI", "HandleUserLogin", "4");
                    userAcc.IsLockedOut = true;
                }
                else if (LoginSecurity.CheckPassword(accountName, password, null))
                {
                    da.LogAppRecord("PortalServerAPI", "HandleUserLogin", "5");

                    UserIdentifiers userIds = da.GetUserIdentifiers(accountName, null);

                    if (userIds != null)
                    {
                        da.LogAppRecord("PortalServerAPI", "HandleUserLogin", "6");

                        userAcc = da.GetUserAccount(userIds.EntityId, userIds.Id, userIds.EntityId, userIds.Id);

                        da.LogAppRecord("PortalServerAPI", "HandleUserLogin", "7");

                        header = new TokenAuth(TokenAuth.TokenStatus.SUCCESS_WITH_NEW_TOKEN)
                        {
                            Token = ServicesCommonDA.GenerateSessionToken(accountName, string.Empty, ServicesCommonDA.TokenRole.PORTAL)
                        };
                    }
                }
            }
            catch (Exception e)
            {
                da.LogAppRecord("PortalServerAPI", "HandleUserLogin", "Exception: " + e.Message);
            }

            da.LogAppRecord("PortalServerAPI", "HandleUserLogin", "8");

            return userAcc;
        }

        public bool HandleValidateToken(ref TokenAuth header, long userId)
        {
            if (ValidateHeaders("HandleValidateToken", ref header) == false)
                return false;

            PortalServerDA da = new PortalServerDA();

            bool isSuccess = false;

            try
            {
                isSuccess = da.ValidateToken(header.Token, userId);
            }
            catch (Exception e)
            {
                da.LogAppRecord("PortalServerAPI", "HandleValidateToken", "Exception: " + e.Message);
            }

            return isSuccess;
        }

        private static bool ValidateHeaders(string method, ref TokenAuth header)
        {
            if (!TokenValidation.ValidateToken(ref header) || !TokenValidation.ValidateNonce(ref header, method))
                return false;

            header.Status = ServicesCommonDA.AuthenticateToken(header.Token);

            if (header.GetStatus() != TokenAuth.TokenStatus.SUCCESS &&
                header.GetStatus() != TokenAuth.TokenStatus.SUCCESS_WITH_NEW_TOKEN)
                return false;

            // Extend the lifespan of the token
            if (ServicesCommonDA.UpdateTokenExpDate(header.Token, null) == false)
                new PortalServerDA().LogAppRecord("PortalServerAPI", method, "UpdateTokenExpDate failed: " + header.Token);

            return true;
        }
        public string GetTenant(string accountName)
        {
            string tenant = null;
            tenant = ServicesCommonDA.GetTenantIdByEmail(accountName);
            return tenant;
        }
        public UserAccount HandleUserLogin(ref TokenAuth header, string accountName)
        {
            PortalServerDA da = new PortalServerDA();
            UserAccount userAcc = new UserAccount();
            try
            {
                if (header != null && !TokenValidation.ValidateNonce(ref header, "HandleUserLogin"))
                    return null;
                da.LogAppRecord("PortalServerAPI", "HandleUserLogin", "5");

                UserIdentifiers userIds = da.GetUserIdentifiers(accountName, null);

                if (userIds != null)
                {
                    da.LogAppRecord("PortalServerAPI", "HandleUserLogin", "6");

                    userAcc = da.GetUserAccount(userIds.EntityId, userIds.Id, userIds.EntityId, userIds.Id);

                    da.LogAppRecord("PortalServerAPI", "HandleUserLogin", "7");

                    header = new TokenAuth(TokenAuth.TokenStatus.SUCCESS_WITH_NEW_TOKEN)
                    {
                        Token = ServicesCommonDA.GenerateSessionToken(accountName, string.Empty, ServicesCommonDA.TokenRole.PORTAL)
                    };
                }
            }
            catch (Exception ex)
            {

            }
            return userAcc;
        }
    }
}
