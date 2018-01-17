using System;
using System.Linq;
using System.Threading.Tasks;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;

namespace Encryptics.WebPortal.DependencyResolution
{
    public class MockPortalService : PortalServiceSoap, IDisposable
    {
        private static readonly TokenAuth _token = new TokenAuth
            {
                Token = "xyz",
                Status = 100
            };

        public Task<GetUsageRightsGroupUsersArchivedResponse> GetUsageRightsGroupUsersArchivedAsync(
            GetUsageRightsGroupUsersArchivedRequest request)
        {
            throw new NotImplementedException();
        }

        public GetUserAuthorizedActionResponse GetUserAuthorizedAction(GetUserAuthorizedActionRequest request)
        {
            return new GetUserAuthorizedActionResponse(_token, true);
        }

        public Task<GetUserAuthorizedActionResponse> GetUserAuthorizedActionAsync(GetUserAuthorizedActionRequest request)
        {
            var taskSource = new TaskCompletionSource<GetUserAuthorizedActionResponse>();

            taskSource.SetResult(GetUserAuthorizedAction(request));

            return taskSource.Task;
        }

        public GetUserAuthorizedActionsResponse GetUserAuthorizedActions(GetUserAuthorizedActionsRequest request)
        {
            return new GetUserAuthorizedActionsResponse(_token, request.actions.Select(action => new AuthorizedAction
                {
                    Action = action,
                    IsAuthorized = true
                }).ToArray());
        }

        public Task<GetUserAuthorizedActionsResponse> GetUserAuthorizedActionsAsync(
            GetUserAuthorizedActionsRequest request)
        {
            var taskSource = new TaskCompletionSource<GetUserAuthorizedActionsResponse>();

            taskSource.SetResult(GetUserAuthorizedActions(request));

            return taskSource.Task;
        }

        public GetUserCompaniesResponse GetUserCompanies(GetUserCompaniesRequest request)
        {
            return new GetUserCompaniesResponse(_token, new[]
                {
                    new CompanyListItem
                        {
                            Id = 100,
                            LicensingInfo =
                                new LicensingInfo {ActiveLicenses = 10, AvailableLicenses = 4, UsedLicenses = 6},
                            Name = "Company 1",
                            Role = RoleType.SuperAdmin
                        },
                    new CompanyListItem
                        {
                            Id = 200,
                            LicensingInfo =
                                new LicensingInfo {ActiveLicenses = 15, AvailableLicenses = 10, UsedLicenses = 5},
                            Name = "Company 2",
                            Role = RoleType.SuperAdmin
                        },
                    new CompanyListItem
                        {
                            Id = 300,
                            LicensingInfo =
                                new LicensingInfo {ActiveLicenses = 12, AvailableLicenses = 5, UsedLicenses = 7},
                            Name = "Company 3",
                            Role = RoleType.SuperAdmin
                        },
                    new CompanyListItem
                        {
                            Id = 400,
                            LicensingInfo =
                                new LicensingInfo {ActiveLicenses = 22, AvailableLicenses = 3, UsedLicenses = 19},
                            Name = "Company 4",
                            Role = RoleType.SuperAdmin
                        },
                    new CompanyListItem
                        {
                            Id = 500,
                            LicensingInfo =
                                new LicensingInfo {ActiveLicenses = 100, AvailableLicenses = 45, UsedLicenses = 55},
                            Name = "Company 5",
                            Role = RoleType.SuperAdmin
                        }
                });
        }

        public Task<GetUserCompaniesResponse> GetUserCompaniesAsync(GetUserCompaniesRequest request)
        {
            var taskSource = new TaskCompletionSource<GetUserCompaniesResponse>();

            taskSource.SetResult(GetUserCompanies(request));

            return taskSource.Task;
        }

        public GetUserDecryptionsResponse GetUserDecryptions(GetUserDecryptionsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetUserDecryptionsResponse> GetUserDecryptionsAsync(GetUserDecryptionsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetAccountDetailsResponse GetAccountDetails(GetAccountDetailsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetAccountDetailsResponse> GetAccountDetailsAsync(GetAccountDetailsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetUserEncryptionsResponse GetUserEncryptions(GetUserEncryptionsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetUserEncryptionsResponse> GetUserEncryptionsAsync(GetUserEncryptionsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetUserEntityRolesResponse GetUserEntityRoles(GetUserEntityRolesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetUserEntityRolesResponse> GetUserEntityRolesAsync(GetUserEntityRolesRequest request)
        {
            throw new NotImplementedException();
        }

        public GetUserSecurityQuestionResponse GetUserSecurityQuestion(GetUserSecurityQuestionRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetUserSecurityQuestionResponse> GetUserSecurityQuestionAsync(GetUserSecurityQuestionRequest request)
        {
            throw new NotImplementedException();
        }

        public SecurityQuestion[] GetUserSecurityQuestions(string email)
        {
            throw new NotImplementedException();
        }

        public Task<SecurityQuestion[]> GetUserSecurityQuestionsAsync(string email)
        {
            throw new NotImplementedException();
        }

        public GetUserUsageRightsResponse GetUserUsageRights(GetUserUsageRightsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetUserUsageRightsResponse> GetUserUsageRightsAsync(GetUserUsageRightsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetUserUsageSummaryByMonthResponse GetUserUsageSummaryByMonth(GetUserUsageSummaryByMonthRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetUserUsageSummaryByMonthResponse> GetUserUsageSummaryByMonthAsync(
            GetUserUsageSummaryByMonthRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertAccessGroupResponse InsertAccessGroup(InsertAccessGroupRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertAccessGroupResponse> InsertAccessGroupAsync(InsertAccessGroupRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertAccessGroupUserResponse InsertAccessGroupUser(InsertAccessGroupUserRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertAccessGroupUserResponse> InsertAccessGroupUserAsync(InsertAccessGroupUserRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertCompanyResponse InsertCompany(InsertCompanyRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertCompanyResponse> InsertCompanyAsync(InsertCompanyRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertCompanyDLPTermResponse InsertCompanyDLPTerm(InsertCompanyDLPTermRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertCompanyDLPTermResponse> InsertCompanyDLPTermAsync(InsertCompanyDLPTermRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertCompanyDomainResponse InsertCompanyDomain(InsertCompanyDomainRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertCompanyDomainResponse> InsertCompanyDomainAsync(InsertCompanyDomainRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertDLPPublicationResponse InsertDLPPublication(InsertDLPPublicationRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertDLPPublicationResponse> InsertDLPPublicationAsync(InsertDLPPublicationRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertDLPRuleResponse InsertDLPRule(InsertDLPRuleRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertDLPRuleResponse> InsertDLPRuleAsync(InsertDLPRuleRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertEntityRoleUserResponse InsertEntityRoleUser(InsertEntityRoleUserRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertEntityRoleUserResponse> InsertEntityRoleUserAsync(InsertEntityRoleUserRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertEntityRoleUsersResponse InsertEntityRoleUsers(InsertEntityRoleUsersRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertEntityRoleUsersResponse> InsertEntityRoleUsersAsync(InsertEntityRoleUsersRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertGWPublicationResponse InsertGWPublication(InsertGWPublicationRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertGWPublicationResponse> InsertGWPublicationAsync(InsertGWPublicationRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertImportUsersRequestResponse InsertImportUsersRequest(InsertImportUsersRequestRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertImportUsersRequestResponse> InsertImportUsersRequestAsync(
            InsertImportUsersRequestRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertLicenseTransferResponse InsertLicenseTransfer(InsertLicenseTransferRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertLicenseTransferResponse> InsertLicenseTransferAsync(InsertLicenseTransferRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertRequestUserTransferResponse InsertRequestUserTransfer(InsertRequestUserTransferRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertRequestUserTransferResponse> InsertRequestUserTransferAsync(
            InsertRequestUserTransferRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertResetPasswordRequestResponse InsertResetPasswordRequest(InsertResetPasswordRequestRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertResetPasswordRequestResponse> InsertResetPasswordRequestAsync(
            InsertResetPasswordRequestRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertServerKeyResponse InsertServerKey(InsertServerKeyRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertServerKeyResponse> InsertServerKeyAsync(InsertServerKeyRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertSoftwareReleaseResponse InsertSoftwareRelease(InsertSoftwareReleaseRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertSoftwareReleaseResponse> InsertSoftwareReleaseAsync(InsertSoftwareReleaseRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertUsageRightsGroupResponse InsertUsageRightsGroup(InsertUsageRightsGroupRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertUsageRightsGroupResponse> InsertUsageRightsGroupAsync(InsertUsageRightsGroupRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertUsageRightsGroupUserResponse InsertUsageRightsGroupUser(InsertUsageRightsGroupUserRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertUsageRightsGroupUserResponse> InsertUsageRightsGroupUserAsync(
            InsertUsageRightsGroupUserRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertUserResponse InsertUser(InsertUserRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertUserResponse> InsertUserAsync(InsertUserRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertUserAdminCompaniesResponse InsertUserAdminCompanies(InsertUserAdminCompaniesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertUserAdminCompaniesResponse> InsertUserAdminCompaniesAsync(
            InsertUserAdminCompaniesRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertUsersByBulkResponse InsertUsersByBulk(InsertUsersByBulkRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertUsersByBulkResponse> InsertUsersByBulkAsync(InsertUsersByBulkRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertUserLicenseResponse InsertUserLicense(InsertUserLicenseRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertUserLicenseResponse> InsertUserLicenseAsync(InsertUserLicenseRequest request)
        {
            throw new NotImplementedException();
        }

        public InsertUserSecurityQuestionsResponse InsertUserSecurityQuestions(
            InsertUserSecurityQuestionsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<InsertUserSecurityQuestionsResponse> InsertUserSecurityQuestionsAsync(
            InsertUserSecurityQuestionsRequest request)
        {
            throw new NotImplementedException();
        }

        public LogDebugRecordResponse LogDebugRecord(LogDebugRecordRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<LogDebugRecordResponse> LogDebugRecordAsync(LogDebugRecordRequest request)
        {
            throw new NotImplementedException();
        }

        public RemoveAccessGroupUserResponse RemoveAccessGroupUser(RemoveAccessGroupUserRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<RemoveAccessGroupUserResponse> RemoveAccessGroupUserAsync(RemoveAccessGroupUserRequest request)
        {
            throw new NotImplementedException();
        }

        public RemoveCompanyResponse RemoveCompany(RemoveCompanyRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<RemoveCompanyResponse> RemoveCompanyAsync(RemoveCompanyRequest request)
        {
            throw new NotImplementedException();
        }

        public RemoveCompanyDLPTermResponse RemoveCompanyDLPTerm(RemoveCompanyDLPTermRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<RemoveCompanyDLPTermResponse> RemoveCompanyDLPTermAsync(RemoveCompanyDLPTermRequest request)
        {
            throw new NotImplementedException();
        }

        public RemoveCompanyDomainResponse RemoveCompanyDomain(RemoveCompanyDomainRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<RemoveCompanyDomainResponse> RemoveCompanyDomainAsync(RemoveCompanyDomainRequest request)
        {
            throw new NotImplementedException();
        }

        public RemoveDLPRuleResponse RemoveDLPRule(RemoveDLPRuleRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<RemoveDLPRuleResponse> RemoveDLPRuleAsync(RemoveDLPRuleRequest request)
        {
            throw new NotImplementedException();
        }

        public RemovePendingAccountResponse RemovePendingAccount(RemovePendingAccountRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<RemovePendingAccountResponse> RemovePendingAccountAsync(RemovePendingAccountRequest request)
        {
            throw new NotImplementedException();
        }

        public RemoveUsageRightsGroupResponse RemoveUsageRightsGroup(RemoveUsageRightsGroupRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<RemoveUsageRightsGroupResponse> RemoveUsageRightsGroupAsync(RemoveUsageRightsGroupRequest request)
        {
            throw new NotImplementedException();
        }

        public RemoveUsageRightsGroupUserResponse RemoveUsageRightsGroupUser(RemoveUsageRightsGroupUserRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<RemoveUsageRightsGroupUserResponse> RemoveUsageRightsGroupUserAsync(
            RemoveUsageRightsGroupUserRequest request)
        {
            throw new NotImplementedException();
        }

        public RemoveUserResponse RemoveUser(RemoveUserRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<RemoveUserResponse> RemoveUserAsync(RemoveUserRequest request)
        {
            throw new NotImplementedException();
        }

        public RemoveUserAdminCompaniesResponse RemoveUserAdminCompanies(RemoveUserAdminCompaniesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<RemoveUserAdminCompaniesResponse> RemoveUserAdminCompaniesAsync(
            RemoveUserAdminCompaniesRequest request)
        {
            throw new NotImplementedException();
        }

        public RemoveUserDeviceResponse RemoveUserDevice(RemoveUserDeviceRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<RemoveUserDeviceResponse> RemoveUserDeviceAsync(RemoveUserDeviceRequest request)
        {
            throw new NotImplementedException();
        }

        public RemoveUserLicensesResponse RemoveUserLicenses(RemoveUserLicensesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<RemoveUserLicensesResponse> RemoveUserLicensesAsync(RemoveUserLicensesRequest request)
        {
            throw new NotImplementedException();
        }

        public bool ResendActivation(string account_name)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ResendActivationAsync(string account_name)
        {
            throw new NotImplementedException();
        }

        public bool ResendDeviceActivation(string account_name, string hwid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ResendDeviceActivationAsync(string account_name, string hwid)
        {
            throw new NotImplementedException();
        }

        public ResendRequestUserTransferResponse ResendRequestUserTransfer(ResendRequestUserTransferRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<ResendRequestUserTransferResponse> ResendRequestUserTransferAsync(
            ResendRequestUserTransferRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateAccessGroupResponse UpdateAccessGroup(UpdateAccessGroupRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateAccessGroupResponse> UpdateAccessGroupAsync(UpdateAccessGroupRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateCompanyResponse UpdateCompany(UpdateCompanyRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateCompanyResponse> UpdateCompanyAsync(UpdateCompanyRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateCompanyLexiconsResponse UpdateCompanyLexicons(UpdateCompanyLexiconsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateCompanyLexiconsResponse> UpdateCompanyLexiconsAsync(UpdateCompanyLexiconsRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateCompanyDLPConfigResponse UpdateCompanyDLPConfig(UpdateCompanyDLPConfigRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateCompanyDLPConfigResponse> UpdateCompanyDLPConfigAsync(UpdateCompanyDLPConfigRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateCompanyDLPSettingsResponse UpdateCompanyDLPSettings(UpdateCompanyDLPSettingsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateCompanyDLPSettingsResponse> UpdateCompanyDLPSettingsAsync(
            UpdateCompanyDLPSettingsRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateCompanyEmailTemplateResponse UpdateCompanyEmailTemplate(UpdateCompanyEmailTemplateRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateCompanyEmailTemplateResponse> UpdateCompanyEmailTemplateAsync(
            UpdateCompanyEmailTemplateRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateCompanyGWConfigResponse UpdateCompanyGWConfig(UpdateCompanyGWConfigRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateCompanyGWConfigResponse> UpdateCompanyGWConfigAsync(UpdateCompanyGWConfigRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateDeviceActivationResponse UpdateDeviceActivation(UpdateDeviceActivationRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateDeviceActivationResponse> UpdateDeviceActivationAsync(UpdateDeviceActivationRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateDeviceStatusResponse UpdateDeviceStatus(UpdateDeviceStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateDeviceStatusResponse> UpdateDeviceStatusAsync(UpdateDeviceStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateDLPRuleDetailsResponse UpdateDLPRuleDetails(UpdateDLPRuleDetailsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateDLPRuleDetailsResponse> UpdateDLPRuleDetailsAsync(UpdateDLPRuleDetailsRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateDLPRuleStatusResponse UpdateDLPRuleStatus(UpdateDLPRuleStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateDLPRuleStatusResponse> UpdateDLPRuleStatusAsync(UpdateDLPRuleStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateDLPRulesVisibilityResponse UpdateDLPRulesVisibility(UpdateDLPRulesVisibilityRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateDLPRulesVisibilityResponse> UpdateDLPRulesVisibilityAsync(
            UpdateDLPRulesVisibilityRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateEntitySoftwareReleaseResponse UpdateEntitySoftwareRelease(
            UpdateEntitySoftwareReleaseRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateEntitySoftwareReleaseResponse> UpdateEntitySoftwareReleaseAsync(
            UpdateEntitySoftwareReleaseRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateGWSettingResponse UpdateGWSetting(UpdateGWSettingRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateGWSettingResponse> UpdateGWSettingAsync(UpdateGWSettingRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdatePasswordResponse UpdatePassword(UpdatePasswordRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdatePasswordResponse> UpdatePasswordAsync(UpdatePasswordRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateRequestUserTransferResponse UpdateRequestUserTransfer(UpdateRequestUserTransferRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateRequestUserTransferResponse> UpdateRequestUserTransferAsync(
            UpdateRequestUserTransferRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateSoftwareReleaseResponse UpdateSoftwareRelease(UpdateSoftwareReleaseRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateSoftwareReleaseResponse> UpdateSoftwareReleaseAsync(UpdateSoftwareReleaseRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateSoftwareReleaseGlobalMinimumResponse UpdateSoftwareReleaseGlobalMinimum(
            UpdateSoftwareReleaseGlobalMinimumRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateSoftwareReleaseGlobalMinimumResponse> UpdateSoftwareReleaseGlobalMinimumAsync(
            UpdateSoftwareReleaseGlobalMinimumRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateSoftwareReleaseActiveStatusResponse UpdateSoftwareReleaseActiveStatus(
            UpdateSoftwareReleaseActiveStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateSoftwareReleaseActiveStatusResponse> UpdateSoftwareReleaseActiveStatusAsync(
            UpdateSoftwareReleaseActiveStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateUsageRightsGroupResponse UpdateUsageRightsGroup(UpdateUsageRightsGroupRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateUsageRightsGroupResponse> UpdateUsageRightsGroupAsync(UpdateUsageRightsGroupRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateUsageRightsGroupStatusResponse UpdateUsageRightsGroupStatus(
            UpdateUsageRightsGroupStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateUsageRightsGroupStatusResponse> UpdateUsageRightsGroupStatusAsync(
            UpdateUsageRightsGroupStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateUserResponse UpdateUser(UpdateUserRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateUserResponse> UpdateUserAsync(UpdateUserRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateUserAdminStatusResponse UpdateUserAdminStatus(UpdateUserAdminStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateUserAdminStatusResponse> UpdateUserAdminStatusAsync(UpdateUserAdminStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateUserCompanyResponse UpdateUserCompany(UpdateUserCompanyRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateUserCompanyResponse> UpdateUserCompanyAsync(UpdateUserCompanyRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateUserContactInfoResponse UpdateUserContactInfo(UpdateUserContactInfoRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateUserContactInfoResponse> UpdateUserContactInfoAsync(UpdateUserContactInfoRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateUserPasswordResponse UpdateUserPassword(UpdateUserPasswordRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateUserPasswordResponse> UpdateUserPasswordAsync(UpdateUserPasswordRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateUserResetPasswordResponse UpdateUserResetPassword(UpdateUserResetPasswordRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateUserResetPasswordResponse> UpdateUserResetPasswordAsync(UpdateUserResetPasswordRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateUserSecurityQuestionResponse UpdateUserSecurityQuestion(UpdateUserSecurityQuestionRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateUserSecurityQuestionResponse> UpdateUserSecurityQuestionAsync(
            UpdateUserSecurityQuestionRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateUserStatusResponse UpdateUserStatus(UpdateUserStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateUserStatusResponse> UpdateUserStatusAsync(UpdateUserStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateUserUnlockResponse UpdateUserUnlock(UpdateUserUnlockRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateUserUnlockResponse> UpdateUserUnlockAsync(UpdateUserUnlockRequest request)
        {
            throw new NotImplementedException();
        }

        public UpdateUserUsageRightsResponse UpdateUserUsageRights(UpdateUserUsageRightsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateUserUsageRightsResponse> UpdateUserUsageRightsAsync(UpdateUserUsageRightsRequest request)
        {
            throw new NotImplementedException();
        }

        public UserLoginResponse UserLogin(UserLoginRequest request)
        {
            var tokenAuth = new TokenAuth
                {
                    Status = TokenStatus.NewToken,
                    Token = "xyz"
                };

            return new UserLoginResponse(tokenAuth, new UserAccount
                {
                    Company = "Encryptics",
                    UserName = request.account_name,
                    EntityId = 1,
                    Id = 100
                });
        }

        public Task<UserLoginResponse> UserLoginAsync(UserLoginRequest request)
        {
            var taskSource = new TaskCompletionSource<UserLoginResponse>();

            taskSource.SetResult(UserLogin(request));

            return taskSource.Task;
        }

        public UserLogoutResponse UserLogout(UserLogoutRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UserLogoutResponse> UserLogoutAsync(UserLogoutRequest request)
        {
            throw new NotImplementedException();
        }

        public TokenLoginResponse TokenLogin(TokenLoginRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<TokenLoginResponse> TokenLoginAsync(TokenLoginRequest request)
        {
            throw new NotImplementedException();
        }

        public ValidateTokenResponse ValidateToken(ValidateTokenRequest request)
        {
            var tokenAuth = request.TokenAuth;
            tokenAuth.Status = TokenStatus.Succes;
            return new ValidateTokenResponse { TokenAuth = tokenAuth, ValidateTokenResult = true };
        }

        public Task<ValidateTokenResponse> ValidateTokenAsync(ValidateTokenRequest request)
        {
            throw new NotImplementedException();
        }

        public bool VerifyUserSecurityQuestion(string email, int question_number, string answer_hash)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyUserSecurityQuestionAsync(string email, int question_number, string answer_hash)
        {
            throw new NotImplementedException();
        }

        public AccountActivationResponse AccountActivation(AccountActivationRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<AccountActivationResponse> AccountActivationAsync(AccountActivationRequest request)
        {
            throw new NotImplementedException();
        }

        public AccountActivationByIDResponse AccountActivationByID(AccountActivationByIDRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<AccountActivationByIDResponse> AccountActivationByIDAsync(AccountActivationByIDRequest request)
        {
            throw new NotImplementedException();
        }

        public AccountActivationWithPasswordResponse AccountActivationWithPassword(
            AccountActivationWithPasswordRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<AccountActivationWithPasswordResponse> AccountActivationWithPasswordAsync(
            AccountActivationWithPasswordRequest request)
        {
            throw new NotImplementedException();
        }

        public AccountRegistrationStatus AccountRegistration(string account_name, string first_name, string last_name,
                                                             string password,
                                                             SecurityQuestion[] security_questions,
                                                             OriginSite registration_site)
        {
            throw new NotImplementedException();
        }

        public Task<AccountRegistrationStatus> AccountRegistrationAsync(string account_name, string first_name,
                                                                        string last_name, string password,
                                                                        SecurityQuestion[] security_questions,
                                                                        OriginSite registration_site)
        {
            throw new NotImplementedException();
        }

        public ExpireTokenSessionResponse ExpireTokenSession(ExpireTokenSessionRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<ExpireTokenSessionResponse> ExpireTokenSessionAsync(ExpireTokenSessionRequest request)
        {
            throw new NotImplementedException();
        }

        public GetAccountActivationLinkStatusResponse GetAccountActivationLinkStatus(
            GetAccountActivationLinkStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetAccountActivationLinkStatusResponse> GetAccountActivationLinkStatusAsync(
            GetAccountActivationLinkStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public GetAccountsListResponse GetAccountsList(GetAccountsListRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetAccountsListResponse> GetAccountsListAsync(GetAccountsListRequest request)
        {
            throw new NotImplementedException();
        }

        public GetAccessGroupDetailsResponse GetAccessGroupDetails(GetAccessGroupDetailsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetAccessGroupDetailsResponse> GetAccessGroupDetailsAsync(GetAccessGroupDetailsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetAccessGroupsResponse GetAccessGroups(GetAccessGroupsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetAccessGroupsResponse> GetAccessGroupsAsync(GetAccessGroupsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetAccessGroupUsersResponse GetAccessGroupUsers(GetAccessGroupUsersRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetAccessGroupUsersResponse> GetAccessGroupUsersAsync(GetAccessGroupUsersRequest request)
        {
            throw new NotImplementedException();
        }

        public GetAccessGroupUsersArchivedResponse GetAccessGroupUsersArchived(
            GetAccessGroupUsersArchivedRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetAccessGroupUsersArchivedResponse> GetAccessGroupUsersArchivedAsync(
            GetAccessGroupUsersArchivedRequest request)
        {
            throw new NotImplementedException();
        }

        public GetCompanyDetailsResponse GetCompanyDetails(GetCompanyDetailsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetCompanyDetailsResponse> GetCompanyDetailsAsync(GetCompanyDetailsRequest request)
        {
            var task = new Task<GetCompanyDetailsResponse>(() => new GetCompanyDetailsResponse
                {
                    TokenAuth = _token,
                    GetCompanyDetailsResult = new Company
                        {
                            Id = request.entity_id,
                            Abbreviation = string.Format("C{0}", request.entity_id),
                            Contact1Name = "Contact Name",
                            Name = string.Format("Comapny {0}", request.entity_id),
                            Contact1Phone =  "",
                            Contact2Name = "",
                            Contact2Phone = "",
                            ContactInfo = new ContactInfo
                                {
                                    Address1 = "",
                                    Address2 = "",
                                    City = "",
                                    Country = "",
                                    Email = "",
                                    Fax = "",
                                    Mobile = "",
                                    Phone = "",
                                    Region = "",
                                    State = "",
                                    ZipCode = ""
                                },
                                CountList = new CompanyCountList
                                    {
                                        DeviceCount = 1,
                                        EncryptionCount = 1,
                                        LexiconCount = 1,
                                        PolicyCount = 1,
                                        SuspendedCount = 1,
                                        UnactivatedCount = 1,
                                        UserCount = 1
                                    },
                                    CreatedDate = DateTime.Today.AddYears(-1),
                                    GlobalExpirationDate = DateTime.Today.AddYears(1),
                                    IsAntiMalwareVisible = true,
                                    IsDLPVisible = true,
                                    IsTrialModeActive = false,
                                    LicensingInfo = new LicensingInfo
                                        {
                                            ActiveLicenses = 10,
                                            AvailableLicenses = 10,
                                            UsedLicenses = 10
                                        },
                                        ParentId = 0,
                                        ParentName = "",
                                        RequireAccountApproval = false,
                                        RequirePasswordChangeDays = 0,
                                        Type = CompanyType.Direct_Customer,
                                        UseGlobalExpirationDate = true
                        }
                });
            return task;
        }

        public GetCompanyDevicesResponse GetCompanyDevices(GetCompanyDevicesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetCompanyDevicesResponse> GetCompanyDevicesAsync(GetCompanyDevicesRequest request)
        {
            throw new NotImplementedException();
        }

        public GetCompanyDLPConfigResponse GetCompanyDLPConfig(GetCompanyDLPConfigRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetCompanyDLPConfigResponse> GetCompanyDLPConfigAsync(GetCompanyDLPConfigRequest request)
        {
            throw new NotImplementedException();
        }

        public GetCompanyDomainsResponse GetCompanyDomains(GetCompanyDomainsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetCompanyDomainsResponse> GetCompanyDomainsAsync(GetCompanyDomainsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetCompanyEmailTemplateResponse GetCompanyEmailTemplate(GetCompanyEmailTemplateRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetCompanyEmailTemplateResponse> GetCompanyEmailTemplateAsync(GetCompanyEmailTemplateRequest request)
        {
            throw new NotImplementedException();
        }

        public GetCompanyGeneologyResponse GetCompanyGeneology(GetCompanyGeneologyRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetCompanyGeneologyResponse> GetCompanyGeneologyAsync(GetCompanyGeneologyRequest request)
        {
            throw new NotImplementedException();
        }

        public GetCompanyGWConfigResponse GetCompanyGWConfig(GetCompanyGWConfigRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetCompanyGWConfigResponse> GetCompanyGWConfigAsync(GetCompanyGWConfigRequest request)
        {
            throw new NotImplementedException();
        }

        public GetDeviceDetailsResponse GetDeviceDetails(GetDeviceDetailsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetDeviceDetailsResponse> GetDeviceDetailsAsync(GetDeviceDetailsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetDeviceListResponse GetDeviceList(GetDeviceListRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetDeviceListResponse> GetDeviceListAsync(GetDeviceListRequest request)
        {
            throw new NotImplementedException();
        }

        public GetEncryptionDetailsResponse GetEncryptionDetails(GetEncryptionDetailsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetEncryptionDetailsResponse> GetEncryptionDetailsAsync(GetEncryptionDetailsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetCompanyDLPSummaryResponse GetCompanyDLPSummary(GetCompanyDLPSummaryRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetCompanyDLPSummaryResponse> GetCompanyDLPSummaryAsync(GetCompanyDLPSummaryRequest request)
        {
            throw new NotImplementedException();
        }

        public GetDLPJustificationsResponse GetDLPJustifications(GetDLPJustificationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetDLPJustificationsResponse> GetDLPJustificationsAsync(GetDLPJustificationsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetDLPPoliciesResponse GetDLPPolicies(GetDLPPoliciesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetDLPPoliciesResponse> GetDLPPoliciesAsync(GetDLPPoliciesRequest request)
        {
            throw new NotImplementedException();
        }

        public GetDLPRuleResponse GetDLPRule(GetDLPRuleRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetDLPRuleResponse> GetDLPRuleAsync(GetDLPRuleRequest request)
        {
            throw new NotImplementedException();
        }

        public GetDLPRulesResponse GetDLPRules(GetDLPRulesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetDLPRulesResponse> GetDLPRulesAsync(GetDLPRulesRequest request)
        {
            throw new NotImplementedException();
        }

        public GetDLPRulesVisibilityResponse GetDLPRulesVisibility(GetDLPRulesVisibilityRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetDLPRulesVisibilityResponse> GetDLPRulesVisibilityAsync(GetDLPRulesVisibilityRequest request)
        {
            throw new NotImplementedException();
        }

        public GetDLPRuleTypesResponse GetDLPRuleTypes(GetDLPRuleTypesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetDLPRuleTypesResponse> GetDLPRuleTypesAsync(GetDLPRuleTypesRequest request)
        {
            throw new NotImplementedException();
        }

        public GetDLPViolationsResponse GetDLPViolations(GetDLPViolationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetDLPViolationsResponse> GetDLPViolationsAsync(GetDLPViolationsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetDLPViolationsRuleOverviewByRuleTypeResponse GetDLPViolationsRuleOverviewByRuleType(
            GetDLPViolationsRuleOverviewByRuleTypeRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetDLPViolationsRuleOverviewByRuleTypeResponse> GetDLPViolationsRuleOverviewByRuleTypeAsync(
            GetDLPViolationsRuleOverviewByRuleTypeRequest request)
        {
            throw new NotImplementedException();
        }

        public GetDLPViolationsRuleTypeOverviewResponse GetDLPViolationsRuleTypeOverview(
            GetDLPViolationsRuleTypeOverviewRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetDLPViolationsRuleTypeOverviewResponse> GetDLPViolationsRuleTypeOverviewAsync(
            GetDLPViolationsRuleTypeOverviewRequest request)
        {
            throw new NotImplementedException();
        }

        public GetEntityRoleUsersResponse GetEntityRoleUsers(GetEntityRoleUsersRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetEntityRoleUsersResponse> GetEntityRoleUsersAsync(GetEntityRoleUsersRequest request)
        {
            throw new NotImplementedException();
        }

        public GetEntityUsageSummariesResponse GetEntityUsageSummaries(GetEntityUsageSummariesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetEntityUsageSummariesResponse> GetEntityUsageSummariesAsync(GetEntityUsageSummariesRequest request)
        {
            throw new NotImplementedException();
        }

        public GetGrantableCompaniesResponse GetGrantableCompanies(GetGrantableCompaniesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetGrantableCompaniesResponse> GetGrantableCompaniesAsync(GetGrantableCompaniesRequest request)
        {
            throw new NotImplementedException();
        }

        public GetGrantableRolesResponse GetGrantableRoles(GetGrantableRolesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetGrantableRolesResponse> GetGrantableRolesAsync(GetGrantableRolesRequest request)
        {
            throw new NotImplementedException();
        }

        public GetImportUsersAccountsResponse GetImportUsersAccounts(GetImportUsersAccountsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetImportUsersAccountsResponse> GetImportUsersAccountsAsync(GetImportUsersAccountsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetImportUsersRequestsResponse GetImportUsersRequests(GetImportUsersRequestsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetImportUsersRequestsResponse> GetImportUsersRequestsAsync(GetImportUsersRequestsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetLexiconDLPTermsResponse GetLexiconDLPTerms(GetLexiconDLPTermsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetLexiconDLPTermsResponse> GetLexiconDLPTermsAsync(GetLexiconDLPTermsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetMalwareFilesResponse GetMalwareFiles(GetMalwareFilesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetMalwareFilesResponse> GetMalwareFilesAsync(GetMalwareFilesRequest request)
        {
            throw new NotImplementedException();
        }

        public GetMalwareFileTypeRiskProfileResponse GetMalwareFileTypeRiskProfile(
            GetMalwareFileTypeRiskProfileRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetMalwareFileTypeRiskProfileResponse> GetMalwareFileTypeRiskProfileAsync(
            GetMalwareFileTypeRiskProfileRequest request)
        {
            throw new NotImplementedException();
        }

        public GetMalwareFileTypesResponse GetMalwareFileTypes(GetMalwareFileTypesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetMalwareFileTypesResponse> GetMalwareFileTypesAsync(GetMalwareFileTypesRequest request)
        {
            throw new NotImplementedException();
        }

        public GetMalwareFileTypesOverviewResponse GetMalwareFileTypesOverview(
            GetMalwareFileTypesOverviewRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetMalwareFileTypesOverviewResponse> GetMalwareFileTypesOverviewAsync(
            GetMalwareFileTypesOverviewRequest request)
        {
            throw new NotImplementedException();
        }

        public GetMalwareRiskOverviewResponse GetMalwareRiskOverview(GetMalwareRiskOverviewRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetMalwareRiskOverviewResponse> GetMalwareRiskOverviewAsync(GetMalwareRiskOverviewRequest request)
        {
            throw new NotImplementedException();
        }

        public GetPendingAccountsResponse GetPendingAccounts(GetPendingAccountsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetPendingAccountsResponse> GetPendingAccountsAsync(GetPendingAccountsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetResetPasswordLinkStatusResponse GetResetPasswordLinkStatus(GetResetPasswordLinkStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetResetPasswordLinkStatusResponse> GetResetPasswordLinkStatusAsync(
            GetResetPasswordLinkStatusRequest request)
        {
            throw new NotImplementedException();
        }

        public GetSoftwarePlatformsResponse GetSoftwarePlatforms(GetSoftwarePlatformsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetSoftwarePlatformsResponse> GetSoftwarePlatformsAsync(GetSoftwarePlatformsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetSoftwareReleasesResponse GetSoftwareReleases(GetSoftwareReleasesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetSoftwareReleasesResponse> GetSoftwareReleasesAsync(GetSoftwareReleasesRequest request)
        {
            throw new NotImplementedException();
        }

        public GetTopCompaniesByDecryptionsResponse GetTopCompaniesByDecryptions(
            GetTopCompaniesByDecryptionsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetTopCompaniesByDecryptionsResponse> GetTopCompaniesByDecryptionsAsync(
            GetTopCompaniesByDecryptionsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetTopCompaniesByEncryptionsResponse GetTopCompaniesByEncryptions(
            GetTopCompaniesByEncryptionsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetTopCompaniesByEncryptionsResponse> GetTopCompaniesByEncryptionsAsync(
            GetTopCompaniesByEncryptionsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetTopCompaniesByExpirationResponse GetTopCompaniesByExpiration(
            GetTopCompaniesByExpirationRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<GetTopCompaniesByExpirationResponse> GetTopCompaniesByExpirationAsync(
            GetTopCompaniesByExpirationRequest request)
        {
            return new GetTopCompaniesByExpirationResponse
                {
                    TokenAuth = _token,
                    GetTopCompaniesByExpirationResult = new TopCompaniesByExpirationReport
                        {
                            CompanyExpirations =  new []
                                {
                                   new CompanyExpirationListItem
                                       {
                                           Id = 100,
                                           ExpirationDate = DateTime.Today.AddDays(-15),
                                           Name = "Company 1",
                                           UseGlobalExpirationDate =  false
                                       },
                                   new CompanyExpirationListItem
                                       {
                                           Id = 200,
                                           ExpirationDate = DateTime.Today.AddDays(-15),
                                           Name = "Company 2",
                                           UseGlobalExpirationDate =  false
                                       },
                                   new CompanyExpirationListItem
                                       {
                                           Id = 300,
                                           ExpirationDate = DateTime.Today.AddDays(-15),
                                           Name = "Company 3",
                                           UseGlobalExpirationDate =  false
                                       },
                                   new CompanyExpirationListItem
                                       {
                                           Id = 400,
                                           ExpirationDate = DateTime.Today.AddDays(-15),
                                           Name = "Company 4",
                                           UseGlobalExpirationDate =  false
                                       },
                                   new CompanyExpirationListItem
                                       {
                                           Id = 500,
                                           ExpirationDate = DateTime.Today.AddDays(-15),
                                           Name = "Company 5",
                                           UseGlobalExpirationDate =  false
                                       }
                                },
                            TotalRecords = 10
                        }
                };
        }

        public GetTopCompaniesUsageSummariesByDecryptionsResponse GetTopCompaniesUsageSummariesByDecryptions(
            GetTopCompaniesUsageSummariesByDecryptionsRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<GetTopCompaniesUsageSummariesByDecryptionsResponse> GetTopCompaniesUsageSummariesByDecryptionsAsync(
            GetTopCompaniesUsageSummariesByDecryptionsRequest request)
        {
            return new GetTopCompaniesUsageSummariesByDecryptionsResponse(_token, new[]
                {
                   new CompanyUsageCount
                        {
                            Count = 25,
                            DateOfCount =  DateTime.Today.AddDays(-15),
                            EntityId =  100,
                            Name = "Company 1"
                        },
                        new CompanyUsageCount
                        {
                            Count = 75,
                            DateOfCount =  DateTime.Today.AddDays(-15),
                            EntityId =  200,
                            Name = "Company 2"
                        },
                        new CompanyUsageCount
                        {
                            Count = 150,
                            DateOfCount =  DateTime.Today.AddDays(-15),
                            EntityId =  300,
                            Name = "Company 3"
                        },
                        new CompanyUsageCount
                        {
                            Count = 300,
                            DateOfCount =  DateTime.Today.AddDays(-15),
                            EntityId =  200,
                            Name = "Company 4"
                        },
                        new CompanyUsageCount
                        {
                            Count = 600,
                            DateOfCount =  DateTime.Today.AddDays(-15),
                            EntityId =  500,
                            Name = "Company 5"
                        }
                });
        }

        public GetTopCompaniesUsageSummariesByEncryptionsResponse GetTopCompaniesUsageSummariesByEncryptions(
            GetTopCompaniesUsageSummariesByEncryptionsRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<GetTopCompaniesUsageSummariesByEncryptionsResponse> GetTopCompaniesUsageSummariesByEncryptionsAsync(
            GetTopCompaniesUsageSummariesByEncryptionsRequest request)
        {
            return new GetTopCompaniesUsageSummariesByEncryptionsResponse
                {
                    TokenAuth = _token,
                    GetTopCompaniesUsageSummariesByEncryptionsResult = new[]
                        {
                            new CompanyUsageCount
                                {
                                    Count = 25,
                                    DateOfCount =  DateTime.Today.AddDays(-15),
                                    EntityId =  100,
                                    Name = "Company 1"
                                },
                                new CompanyUsageCount
                                {
                                    Count = 75,
                                    DateOfCount =  DateTime.Today.AddDays(-15),
                                    EntityId =  200,
                                    Name = "Company 2"
                                },
                                new CompanyUsageCount
                                {
                                    Count = 150,
                                    DateOfCount =  DateTime.Today.AddDays(-15),
                                    EntityId =  300,
                                    Name = "Company 3"
                                },
                                new CompanyUsageCount
                                {
                                    Count = 300,
                                    DateOfCount =  DateTime.Today.AddDays(-15),
                                    EntityId =  200,
                                    Name = "Company 4"
                                },
                                new CompanyUsageCount
                                {
                                    Count = 600,
                                    DateOfCount =  DateTime.Today.AddDays(-15),
                                    EntityId =  500,
                                    Name = "Company 5"
                                }
                        }
                };
        }

        public GetTopUsersByEncryptionsResponse GetTopUsersByEncryptions(GetTopUsersByEncryptionsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetTopUsersByEncryptionsResponse> GetTopUsersByEncryptionsAsync(
            GetTopUsersByEncryptionsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetTopUsersByViolationsResponse GetTopUsersByViolations(GetTopUsersByViolationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetTopUsersByViolationsResponse> GetTopUsersByViolationsAsync(GetTopUsersByViolationsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetTopViolatedDLPRulesResponse GetTopViolatedDLPRules(GetTopViolatedDLPRulesRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetTopViolatedDLPRulesResponse> GetTopViolatedDLPRulesAsync(GetTopViolatedDLPRulesRequest request)
        {
            throw new NotImplementedException();
        }

        public GetUsageRightsGroupDetailsResponse GetUsageRightsGroupDetails(GetUsageRightsGroupDetailsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetUsageRightsGroupDetailsResponse> GetUsageRightsGroupDetailsAsync(
            GetUsageRightsGroupDetailsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetUsageRightsGroupsResponse GetUsageRightsGroups(GetUsageRightsGroupsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetUsageRightsGroupsResponse> GetUsageRightsGroupsAsync(GetUsageRightsGroupsRequest request)
        {
            throw new NotImplementedException();
        }

        public GetUsageRightsGroupUsersResponse GetUsageRightsGroupUsers(GetUsageRightsGroupUsersRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetUsageRightsGroupUsersResponse> GetUsageRightsGroupUsersAsync(
            GetUsageRightsGroupUsersRequest request)
        {
            throw new NotImplementedException();
        }

        public GetUsageRightsGroupUsersArchivedResponse GetUsageRightsGroupUsersArchived(
            GetUsageRightsGroupUsersArchivedRequest request)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            
        }
    }
}