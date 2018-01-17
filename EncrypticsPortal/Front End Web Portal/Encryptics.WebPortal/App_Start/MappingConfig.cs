using AutoMapper;
using Encryptics.WebPortal.Areas.Company.Models;
using Encryptics.WebPortal.Areas.Company.Models.Reports.Malware;
using Encryptics.WebPortal.Areas.Company.Models.Reports.PBP;
using Encryptics.WebPortal.Areas.CompanyAdmin.Models;
using Encryptics.WebPortal.Areas.UserAccount.Models;
using Encryptics.WebPortal.Areas.UserAdmin.Models;
using Encryptics.WebPortal.IdentityModel;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using System;
using System.Linq;
using AccountType = Encryptics.WebPortal.Areas.UserAccount.Models.AccountType;
using MyResources = Encryptics.WebPortal.Properties.Resources;
using ServiceCompanyDLPConfig = Encryptics.WebPortal.PortalService.CompanyDLPConfig;
using ServiceDLPPolicy = Encryptics.WebPortal.PortalService.DLPPolicy;
using ServiceDLPRule = Encryptics.WebPortal.PortalService.DLPRule;
using ServiceDLPRuleType = Encryptics.WebPortal.PortalService.DLPRuleType;
using ServiceDLPRuleViolationCount = Encryptics.WebPortal.PortalService.DLPRuleViolationCount;
using ServiceDLPUserViolationCount = Encryptics.WebPortal.PortalService.DLPUserViolationCount;
using ServiceDLPViolation = Encryptics.WebPortal.PortalService.DLPViolation;
using ServiceDLPViolationsRuleTypeOverviewItem = Encryptics.WebPortal.PortalService.DLPViolationsRuleTypeOverviewItem;
using ServiceMalwarefFileTypeOverviewItem = Encryptics.WebPortal.PortalService.MalwareFileTypeOverviewItem;
using ServiceMalwareFileItem = Encryptics.WebPortal.PortalService.MalwareFileItem;
using ServiceMalwareFileTypeRiskProfileItem = Encryptics.WebPortal.PortalService.MalwareFileTypeRiskProfileItem;
using ServiceMalwareRiskOverviewItem = Encryptics.WebPortal.PortalService.MalwareRiskOverviewItem;
using ServiceUserAdminStatus = Encryptics.WebPortal.PortalService.UserAdminStatus;
using ServiceUserEncryptionCount = Encryptics.WebPortal.PortalService.UserEncryptionCount;
using UserEncryptionCount = Encryptics.WebPortal.Areas.Company.Models.Reports.UserEncryptionCount;

// ReSharper disable CheckNamespace

namespace Encryptics.WebPortal
// ReSharper restore CheckNamespace
{
    public class MappingConfig
    {
        public static void RegisterMaps()
        {
            MapUserAccountDetails();
        }

        private static void MapUserAccountDetails()
        {
            Mapper.CreateMap<PortalRoleListItem, WebPortalRole>()
                  .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Title));

            Mapper.CreateMap<UserAccount, UserAccountModel>()
                  .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company))
                  .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.EntityId))
                  .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FName))
                  .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LName))
                  .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                  .ForMember(dest => dest.IsSuspended, opt => opt.MapFrom(src => src.IsSuspended))
                  .ForMember(dest => dest.AccountStatus, opt => opt.MapFrom(src => src.IsSuspended
                                                                                       ? MyResources.SuspendedDisplay
                                                                                       : MyResources.ActiveDisplay))
                  .ForMember(dest => dest.LicenseType, opt => opt.MapFrom(src => src.Type))
                  .ForMember(dest => dest.PrimaryRole, opt => opt.MapFrom(src => src.Role))
                  .ForMember(dest => dest.AdminPortalRoleId, opt => opt.MapFrom(src => src.Role.Id))
                  .ForMember(dest => dest.DateRegistered, opt => opt.MapFrom(src => src.CreatedDate))
                  .ForMember(dest => dest.DateLicenseCreated, opt => opt.MapFrom(src => src.LicenseCreatedDate))
                  .ForMember(dest => dest.DateLicenseExpires, opt => opt.MapFrom(src => src.LicenseExpirationDate))
                  .ForMember(dest => dest.DatePasswordChanged, opt => opt.MapFrom(src => src.LastPasswordChange))
                  .ForMember(dest => dest.IsLockedOut, opt => opt.MapFrom(src => src.IsLockedOut));

            Mapper.CreateMap<Device, DeviceModel>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                  .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.DeviceType))
                  .ForMember(dest => dest.DateDeployed, opt => opt.MapFrom(src => src.DateDeployed))
                  .ForMember(dest => dest.Encrypts, opt => opt.MapFrom(src => src.EncryptedCount))
                  .ForMember(dest => dest.Decrypts, opt => opt.MapFrom(src => src.DecryptedCount))
                  .ForMember(dest => dest.HasActiveSession, opt => opt.MapFrom(src => src.ActiveSession))
                  .ForMember(dest => dest.TokenId, opt => opt.MapFrom(src => src.TokenID))
                  .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            Mapper.CreateMap<UsageSummary, UsageSummaryModel>()
                  .ForMember(dest => dest.Day, opt => opt.MapFrom(src => src.Day))
                  .ForMember(dest => dest.Month, opt => opt.MapFrom(src => src.Month))
                  .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
                  .ForMember(dest => dest.Encrypts, opt => opt.MapFrom(src => src.EncryptCount))
                  .ForMember(dest => dest.Decrypts, opt => opt.MapFrom(src => src.DecryptCount))
                  .ForMember(dest => dest.IsPartial, opt => opt.MapFrom(src => src.IsPartial));

            Mapper.CreateMap<CompanyListItem, CompanyModel>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom((src => src.Id)))
                  .ForMember(dest => dest.Name, opt => opt.MapFrom((src => src.Name)));

            Mapper.CreateMap<CompanyListItem, CompanyListItemModel>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom((src => src.Id)))
                  .ForMember(dest => dest.Name, opt => opt.MapFrom((src => src.Name)))
                  .ForMember(dest => dest.Licensing, opt => opt.MapFrom((src => src.LicensingInfo)));

            Mapper.CreateMap<LicensingInfo, LicensingModel>()
                  .ForMember(dest => dest.ActiveLicenses, opt => opt.MapFrom((src => src.ActiveLicenses)))
                  .ForMember(dest => dest.UsedLicenses, opt => opt.MapFrom((src => src.UsedLicenses)))
                  .ForMember(dest => dest.AvailableLicenses,
                             opt => opt.MapFrom((src => src.ActiveLicenses - src.UsedLicenses)));

            Mapper.CreateMap<LicensingModel, LicensingInfo>()
                  .ForMember(dest => dest.ActiveLicenses, opt => opt.MapFrom((src => src.ActiveLicenses)))
                  .ForMember(dest => dest.UsedLicenses, opt => opt.MapFrom((src => src.UsedLicenses)))
                  .ForMember(dest => dest.AvailableLicenses,
                             opt => opt.MapFrom((src => src.ActiveLicenses - src.UsedLicenses)));

            Mapper.CreateMap<Company, CompanyDetailsModel>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom((src => src.Id)))
                  .ForMember(dest => dest.ParentId, opt => opt.MapFrom((src => src.ParentId)))
                  .ForMember(dest => dest.ParentCompanyName, opt => opt.MapFrom((src => src.ParentName)))
                  .ForMember(dest => dest.Name, opt => opt.MapFrom((src => src.Name)))
                  .ForMember(dest => dest.Abbreviation, opt => opt.MapFrom((src => src.Abbreviation)))
                  .ForMember(dest => dest.Contact1, opt => opt.MapFrom((src => src.Contact1Name)))
                  .ForMember(dest => dest.Contact1PhoneNumber, opt => opt.MapFrom((src => src.Contact1Phone)))
                  .ForMember(dest => dest.Contact2, opt => opt.MapFrom((src => src.Contact2Name)))
                  .ForMember(dest => dest.Contact2PhoneNumber, opt => opt.MapFrom((src => src.Contact2Phone)))
                  .ForMember(dest => dest.ContactInfo, opt => opt.MapFrom((src => src.ContactInfo)))
                  .ForMember(dest => dest.LicenseSummary, opt => opt.MapFrom((src => src.LicensingInfo)))
                  .ForMember(dest => dest.EntityType, opt => opt.MapFrom(src => src.Type))
                  .ForMember(dest => dest.UserCount, opt => opt.MapFrom((src => src.CountList.UserCount)))
                  .ForMember(dest => dest.DeviceCount, opt => opt.MapFrom((src => src.CountList.DeviceCount)))
                  .ForMember(dest => dest.EncryptionCount, opt => opt.MapFrom((src => src.CountList.EncryptionCount)))
                  .ForMember(dest => dest.LexiconCount, opt => opt.MapFrom((src => src.CountList.LexiconCount)))
                  .ForMember(dest => dest.PolicyCount, opt => opt.MapFrom((src => src.CountList.PolicyCount)))
                  .ForMember(dest => dest.UnactivatedCount, opt => opt.MapFrom((src => src.CountList.UnactivatedCount)))
                  .ForMember(dest => dest.GlobalExpirationDate, opt => opt.MapFrom((src => src.GlobalExpirationDate)))
                  .ForMember(dest => dest.IsZDPEnabled, opt => opt.MapFrom((src => src.IsAntiMalwareVisible)))
                  .ForMember(dest => dest.IsPBPEnabled, opt => opt.MapFrom((src => src.IsDLPVisible)))
                  .ForMember(dest => dest.IsInTrialMode, opt => opt.MapFrom((src => src.IsTrialModeActive)))
                  .ForMember(dest => dest.RequireAccountApproval,
                             opt => opt.MapFrom((src => src.RequireAccountApproval)))
                  .ForMember(dest => dest.RequirePasswordChangeDays,
                             opt => opt.MapFrom((src => src.RequirePasswordChangeDays)));

            Mapper.CreateMap<CompanyDetailsModel, Company>()
                  .ForMember(dest => dest.Name, opt => opt.MapFrom((src => src.Name)))
                  .ForMember(dest => dest.Abbreviation, opt => opt.MapFrom((src => src.Abbreviation)))
                  .ForMember(dest => dest.Contact1Name, opt => opt.MapFrom((src => src.Contact1)))
                  .ForMember(dest => dest.Contact1Phone, opt => opt.MapFrom((src => src.Contact1PhoneNumber)))
                  .ForMember(dest => dest.Contact2Name, opt => opt.MapFrom((src => src.Contact2)))
                  .ForMember(dest => dest.Contact2Phone, opt => opt.MapFrom((src => src.Contact2PhoneNumber)))
                  .ForMember(dest => dest.ContactInfo, opt => opt.MapFrom((src => src.ContactInfo)))
                  .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.EntityType))
                  .ForMember(dest => dest.GlobalExpirationDate, opt => opt.MapFrom((src => src.GlobalExpirationDate)))
                  .ForMember(dest => dest.UseGlobalExpirationDate,
                             opt => opt.MapFrom((src => src.UseGlobalExpirationDate)))
                  .ForMember(dest => dest.Id, opt => opt.MapFrom((src => src.Id)))
                  .ForMember(dest => dest.IsAntiMalwareVisible, opt => opt.MapFrom((src => src.IsZDPEnabled)))
                  .ForMember(dest => dest.IsDLPVisible, opt => opt.MapFrom((src => src.IsPBPEnabled)))
                  .ForMember(dest => dest.IsTrialModeActive, opt => opt.MapFrom((src => src.IsInTrialMode)))
                  .ForMember(dest => dest.LicensingInfo, opt => opt.MapFrom((src => src.LicenseSummary)))
                  .ForMember(dest => dest.ParentName, opt => opt.MapFrom((src => src.ParentCompanyName)))
                  .ForMember(dest => dest.ParentId, opt => opt.MapFrom((src => src.ParentId)))
                  .ForMember(dest => dest.RequireAccountApproval,
                             opt => opt.MapFrom((src => src.RequireAccountApproval)))
                  .ForMember(dest => dest.RequirePasswordChangeDays,
                             opt => opt.MapFrom((src => src.RequirePasswordChangeDays)))
                  .ForMember(dest => dest.UseGlobalExpirationDate,
                             opt => opt.MapFrom((src => src.UseGlobalExpirationDate)))
                  .ForMember(dest => dest.CountList, opt => opt.MapFrom(src => new CompanyCountList
                      {
                          DeviceCount = src.DeviceCount,
                          EncryptionCount = src.EncryptionCount,
                          LexiconCount = src.LexiconCount,
                          PolicyCount = src.PolicyCount,
                          UnactivatedCount = src.UnactivatedCount,
                          UserCount = src.UserCount
                      }));

            Mapper.CreateMap<Company, CompanySummaryModel>()
                  .ForMember(dest => dest.Name, opt => opt.MapFrom((src => src.Name)))
                  .ForMember(dest => dest.EntityType, opt => opt.MapFrom((src => src.Type)))
                  .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom((src => src.CreatedDate)))
                  .ForMember(dest => dest.LicenseSummary, opt => opt.MapFrom((src => src.LicensingInfo)))
                  .ForMember(dest => dest.ZDPEnabled, opt => opt.MapFrom(src => src.IsAntiMalwareVisible))
                  .ForMember(dest => dest.PBPEnabled, opt => opt.MapFrom(src => src.IsDLPVisible))
                //.ForMember(dest => dest.PendingAccounts, opt => opt.Ignore())
                //.ForMember(dest => dest.UserAccounts, opt => opt.Ignore())
                  .ForMember(dest => dest.ActiveUserAccountTotal, opt => opt.MapFrom(src => src.CountList.UserCount))
                  .ForMember(dest => dest.SuspendedUserAccountTotal, opt => opt.MapFrom(src => src.CountList.SuspendedCount))
                  .ForMember(dest => dest.PendingUserAccountTotal,
                             opt => opt.MapFrom(src => src.CountList.UnactivatedCount));

            Mapper.CreateMap<ContactInfo, ContactInfoModel>()
                  .ForMember(dest => dest.Address1, opt => opt.MapFrom((src => src.Address1)))
                  .ForMember(dest => dest.Address2, opt => opt.MapFrom((src => src.Address2)))
                  .ForMember(dest => dest.City, opt => opt.MapFrom((src => src.City)))
                  .ForMember(dest => dest.Country, opt => opt.MapFrom((src => src.Country)))
                  .ForMember(dest => dest.Email, opt => opt.MapFrom((src => src.Email)))
                  .ForMember(dest => dest.Fax, opt => opt.MapFrom((src => src.Fax)))
                  .ForMember(dest => dest.Mobile, opt => opt.MapFrom((src => src.Mobile)))
                  .ForMember(dest => dest.Phone, opt => opt.MapFrom((src => src.Phone)))
                  .ForMember(dest => dest.Region, opt => opt.MapFrom((src => src.Region)))
                  .ForMember(dest => dest.State, opt => opt.MapFrom((src => src.State)))
                  .ForMember(dest => dest.ZipCode, opt => opt.MapFrom((src => src.ZipCode)));

            Mapper.CreateMap<ContactInfoModel, ContactInfo>()
                  .ForMember(dest => dest.Address1, opt => opt.MapFrom((src => src.Address1)))
                  .ForMember(dest => dest.Address2, opt => opt.MapFrom((src => src.Address2)))
                  .ForMember(dest => dest.City, opt => opt.MapFrom((src => src.City)))
                  .ForMember(dest => dest.Country, opt => opt.MapFrom((src => src.Country)))
                  .ForMember(dest => dest.Email, opt => opt.MapFrom((src => src.Email)))
                  .ForMember(dest => dest.Fax, opt => opt.MapFrom((src => src.Fax)))
                  .ForMember(dest => dest.Mobile, opt => opt.MapFrom((src => src.Mobile)))
                  .ForMember(dest => dest.Phone, opt => opt.MapFrom((src => src.Phone)))
                  .ForMember(dest => dest.Region, opt => opt.MapFrom((src => src.Region)))
                  .ForMember(dest => dest.State, opt => opt.MapFrom((src => src.State)))
                  .ForMember(dest => dest.ZipCode, opt => opt.MapFrom((src => src.ZipCode)));

            Mapper.CreateMap<UserAccount, EditableUserAccountModel>()
                  .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.EntityId))
                  .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company))
                  .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FName))
                  .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LName))
                  .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                  .ForMember(dest => dest.AccountStatus, opt => opt.MapFrom(src => src.IsSuspended
                                                                                       ? MyResources.SuspendedDisplay
                                                                                       : MyResources.ActiveDisplay))
                  .ForMember(dest => dest.LicenseType, opt => opt.MapFrom(src => src.Type.ToString()))
                  .ForMember(dest => dest.DatePasswordChanged, opt => opt.MapFrom(src => src.LastPasswordChange))
                  .ForMember(dest => dest.ContactInfo, opt => opt.MapFrom(src => src.ContactInfo));

            Mapper.CreateMap<UsageSummary, CompanyUsageSummaryModel>()
                  .ForMember(dest => dest.Date,
                             opt => opt.MapFrom(src => new DateTime(src.Year, src.Month ?? 13, src.Day ?? 1)))
                  .ForMember(dest => dest.Encrypts, opt => opt.MapFrom(src => src.EncryptCount))
                  .ForMember(dest => dest.Decrypts, opt => opt.MapFrom(src => src.DecryptCount));

            Mapper.CreateMap<CompanyDomain, CompanyDomainModel>()
                  .ForMember(dest => dest.Domain, opt => opt.MapFrom(src => src.Domain))
                  .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            Mapper.CreateMap<UserAccountListItem, ActiveAccountsListItemModel>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.EntityId))
                  .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FName))
                  .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LName))
                  .ForMember(dest => dest.UserWebPortalRole, opt => opt.MapFrom(src => src.Role))
                  .ForMember(dest => dest.LicenseCreated, opt => opt.MapFrom(src => src.LicenseCreatedDate))
                  .ForMember(dest => dest.LicenseExpires, opt => opt.MapFrom(src => src.LicenseExpirationDate))
                  .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName));

            Mapper.CreateMap<PendingUserAccountListItem, PendingAccountsListItemModel>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.EntityID))
                  .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FName))
                  .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LName))
                  .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
                  .ForMember(dest => dest.IsLicenseReserved, opt => opt.MapFrom(src => src.IsLicenseReserved))
                  .ForMember(dest => dest.AccountStatus, opt => opt.MapFrom(src => src.Status))
                  .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName));

            Mapper.CreateMap<ServiceUserEncryptionCount, UserEncryptionModel>()
                  .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                  .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FName))
                  .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LName))
                  .ForMember(dest => dest.Encryptions, opt => opt.MapFrom(src => src.EncryptionCount))
                  .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName));

            Mapper.CreateMap<ServiceDLPViolation, DlpViolation>()
                  .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreateDate))
                  .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FName))
                  .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LName))
                  .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                  .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            Mapper.CreateMap<ServiceDLPRuleViolationCount, DlpRuleViolationCount>()
                  .ForMember(dest => dest.ViolationCount, opt => opt.MapFrom(src => src.ViolationCount))
                  .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            Mapper.CreateMap<ServiceDLPUserViolationCount, DlpUserViolationCount>()
                  .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                  .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FName))
                  .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LName))
                  .ForMember(dest => dest.ViolationCount, opt => opt.MapFrom(src => src.ViolationCount));

            Mapper.CreateMap<ServiceUserEncryptionCount, UserEncryptionCount>()
                  .ForMember(dest => dest.EncryptionCount, opt => opt.MapFrom(src => src.EncryptionCount))
                  .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FName))
                  .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LName))
                  .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName));

            Mapper.CreateMap<ServiceMalwareRiskOverviewItem, GwMalwareRisk>()
                  .ForMember(dest => dest.RiskName, opt => opt.MapFrom(src => src.RiskName))
                  .ForMember(dest => dest.TotalViolations, opt => opt.MapFrom(src => src.TotalViolations));

            Mapper.CreateMap<ServiceMalwarefFileTypeOverviewItem, GwMalwareFileType>()
                  .ForMember(dest => dest.FileType, opt => opt.MapFrom(src => src.FileType))
                  .ForMember(dest => dest.TotalFiles, opt => opt.MapFrom(src => src.TotalFiles));

            Mapper.CreateMap<ServiceMalwareFileTypeRiskProfileItem, GwMalwareFileTypeRisk>()
                  .ForMember(dest => dest.Risk, opt => opt.MapFrom(src => src.Risk))
                  .ForMember(dest => dest.TotalViolations, opt => opt.MapFrom(src => src.TotalViolations));

            Mapper.CreateMap<ServiceDLPViolationsRuleTypeOverviewItem, DlpRuleType>()
                  .ForMember(dest => dest.Violations, opt => opt.MapFrom(src => src.TotalViolations))
                  .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.RuleTypeTitle));

            Mapper.CreateMap<ServiceMalwareFileItem, GwMalwareFile>()
                  .ForMember(dest => dest.AuthorEmail, opt => opt.MapFrom(src => src.AuthorEmail))
                  .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                  .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                  .ForMember(dest => dest.FileType, opt => opt.MapFrom(src => src.FileType))
                  .ForMember(dest => dest.IsOutbound, opt => opt.MapFrom(src => src.IsOutbound ? "Outbound" : "Inbound"))
                  .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName));

            Mapper.CreateMap<ServiceCompanyDLPConfig, CompanyDLPSettingsModel>()
                  .ForMember(dest => dest.DefaultDRMForAlwaysEncrypt,
                             opt => opt.MapFrom(src => src.DefaultDRMForAlwaysEncrypt))
                  .ForMember(dest => dest.EnableAlwaysEncrypt, opt => opt.MapFrom(src => src.EnableAlwaysEncrypt))
                  .ForMember(dest => dest.EnableDLP, opt => opt.MapFrom(src => src.EnableDLP))
                  .ForMember(dest => dest.EnablePassiveMode, opt => opt.MapFrom(src => src.EnablePassiveMode))
                  .ForMember(dest => dest.EncryptUponViolation, opt => opt.MapFrom(src => src.EncryptUponViolation))
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.OneAndDone, opt => opt.MapFrom(src => src.OneAndDone))
                  .ForMember(dest => dest.Policies, opt => opt.MapFrom(src => src.Policies.OrderBy(p => p.Id)));

            Mapper.CreateMap<ServiceDLPPolicy, DLPPolicyModel>()
                  .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => src.IsUsedByCompany))
                  .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title));

            Mapper.CreateMap<ServiceDLPRuleType, DLPRuleTypeModel>()
                  .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => src.IsUsedByCompany))
                  .ForMember(dest => dest.PolicyId, opt => opt.MapFrom(src => (DLPPolicyId) src.PolicyId))
                  .ForMember(dest => dest.Rules, opt => opt.MapFrom(src => src.RuleList));

            Mapper.CreateMap<ServiceDLPRule, DLPRuleModel>()
                  .ForMember(dest => dest.RuleId, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.RuleTypeId, opt => opt.MapFrom(src => src.RuleTypeId))
                  .ForMember(dest => dest.EntityRuleId, opt => opt.MapFrom(src => src.EntityRuleId))
                  .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.IsUsedByCompany))
                  .ForMember(dest => dest.PolicyId, opt => opt.MapFrom(src => src.PolicyId))
                  .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                  .ForMember(dest => dest.DefaultDRM, opt => opt.MapFrom(src => src.DefDRMId))
                  .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.SeverityId))
                  .ForMember(dest => dest.BWList, opt => opt.MapFrom(src => src.BWList))
                  .ForMember(dest => dest.Terms, opt => opt.MapFrom(src => src.Terms))
                  .ForMember(dest => dest.Formats,
                             opt =>
                             opt.MapFrom(
                                 src =>
                                 src.RuleTypeId == 31
                                     ? @"Enter a domain such as mailhost.com for example."
                                     : src.Formats))
                  .ForMember(dest => dest.RegexTerm, opt => opt.MapFrom(src => src.RegexTerm));

            Mapper.CreateMap<DLPRuleModel, ServiceDLPRule>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.RuleId))
                  .ForMember(dest => dest.RuleTypeId, opt => opt.MapFrom(src => src.RuleTypeId))
                  .ForMember(dest => dest.EntityRuleId, opt => opt.MapFrom(src => src.EntityRuleId))
                  .ForMember(dest => dest.IsUsedByCompany, opt => opt.MapFrom(src => src.Enabled))
                  .ForMember(dest => dest.PolicyId, opt => opt.MapFrom(src => src.PolicyId))
                  .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                  .ForMember(dest => dest.DefDRMId, opt => opt.MapFrom(src => src.DefaultDRM))
                  .ForMember(dest => dest.SeverityId, opt => opt.MapFrom(src => src.Severity))
                  .ForMember(dest => dest.BWList, opt => opt.MapFrom(src => src.BWList))
                  .ForMember(dest => dest.Terms, opt => opt.MapFrom(src => src.Terms))
                  .ForMember(dest => dest.Formats, opt => opt.MapFrom(src => src.Formats))
                  .ForMember(dest => dest.RegexTerm, opt => opt.MapFrom(src => src.RegexTerm));


            Mapper.CreateMap<CompanyDLPSettingsModel, ServiceCompanyDLPConfig>()
                  .ForMember(dest => dest.DefaultDRMForAlwaysEncrypt,
                             opt => opt.MapFrom(src => src.DefaultDRMForAlwaysEncrypt))
                  .ForMember(dest => dest.EnableAlwaysEncrypt, opt => opt.MapFrom(src => src.EnableAlwaysEncrypt))
                  .ForMember(dest => dest.EnableDLP, opt => opt.MapFrom(src => src.EnableDLP))
                  .ForMember(dest => dest.EnablePassiveMode, opt => opt.MapFrom(src => src.EnablePassiveMode))
                  .ForMember(dest => dest.EncryptUponViolation, opt => opt.MapFrom(src => src.EncryptUponViolation))
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.OneAndDone, opt => opt.MapFrom(src => src.OneAndDone))
                  .ForMember(dest => dest.Policies, opt => opt.Ignore());

            Mapper.CreateMap<UserAdminStatus, ServiceUserAdminStatus>();

            Mapper.CreateMap<CompanyGWConfig, ZDPSettingsModel>()
                  .ForMember(dest => dest.IsEnabled, opt => opt.MapFrom(src => src.EnableAntiMalware))
                  .ForMember(dest => dest.FileTypes, opt => opt.MapFrom(src => src.FileTypes));


            Mapper.CreateMap<GlassWallFileType, ZDPFileTypeModel>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                  .ForMember(dest => dest.ConfigurationSettings, opt => opt.MapFrom(src => src.Configs));

            Mapper.CreateMap<GlassWallConfig, ZDPSettingModel>()
                  .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.FTCId))
                  .ForMember(dest => dest.Setting, opt => opt.MapFrom(src => src.Setting))
                  .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                  .ForMember(dest => dest.IsWaterMark, opt => opt.MapFrom(src => src.IsWatermark));

            Mapper.CreateMap<DLPViolation, PBPJustification>()
                  .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreateDate))
                  .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FName))
                  .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LName))
                  .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                  .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                  .ForMember(dest => dest.Justification, opt => opt.MapFrom(src => src.Justification));

            Mapper.CreateMap<PendingUserAccountListItem, UserAccountModel>()
                  .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FName))
                  .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LName))
                  .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
                  .ForMember(dest => dest.LicenseType,
                             opt =>
                             opt.MapFrom(src => src.IsLicenseReserved ? AccountType.Professional : AccountType.Free))
                  .ForMember(dest => dest.AccountStatus, opt => opt.MapFrom(src => src.Status))
                  .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName));

            Mapper.CreateMap<SoftwareRelease, CompanyProductVersionModel>()
                  .ForMember(dest => dest.VersionNumber,
                             opt => opt.MapFrom(src => new VersionInfo(src.Version)))
                  .ForMember(dest => dest.VersionId, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Platform.Id))
                  .ForMember(dest => dest.ReleaseNotes, opt => opt.MapFrom(src => src.Notes))
                  .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.DownloadURI))
                  .ForMember(dest => dest.Severity,
                             opt =>
                             opt.MapFrom(
                                 src =>
                                 src.IsMajorRelease
                                     ? VersionSeverity.Major
                                     : VersionSeverity.Minor))
                  .ForMember(dest => dest.IsGlobalMinimumVersion, opt => opt.MapFrom(src => src.IsGlobalMinimum))
                  .ForMember(dest => dest.IsCompanyVersion, opt => opt.MapFrom(src => src.IsCurrentEntityVersion))
                  .ForMember(dest => dest.IsCurrentCompanyVersion, opt => opt.MapFrom(src => src.IsCurrentEntityVersion));

            Mapper.CreateMap<SoftwareReleaseModel, SoftwareRelease>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.VersionId))
                  .ForMember(dest => dest.CreatedByID,
                             opt => opt.MapFrom(model => EncrypticsPrincipal.CurrentEncrypticsUser.UserId))
                  .ForMember(dest => dest.DownloadURI, opt => opt.MapFrom(src => src.DownloadUri))
                  .ForMember(dest => dest.IsMajorRelease, opt => opt.MapFrom(src => src.IsMajorRelease))
                  .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.ReleaseNotes))
                  .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version))
                  .ForMember(dest => dest.Platform,
                             opt => opt.MapFrom(src => new SoftwarePlatform {Id = src.ProductId}))
                //.ForMember(dest => dest.IsGlobalMinimum, opt => opt.MapFrom(src => src.IsGlobalMinimumVersion))
                  .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status == VersionStatus.Active));

            Mapper.CreateMap<SoftwareRelease, SoftwareReleaseModel>()
                  .ForMember(dest => dest.VersionId, opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.DownloadUri, opt => opt.MapFrom(src => src.DownloadURI))
                  .ForMember(dest => dest.IsMajorRelease, opt => opt.MapFrom(src => src.IsMajorRelease))
                  .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Platform.Id))
                  .ForMember(dest => dest.ReleaseNotes, opt => opt.MapFrom(src => src.Notes))
                  .ForMember(dest => dest.Version, opt => opt.MapFrom(src => new VersionInfo(src.Version)))
                  .ForMember(dest => dest.IsGlobalMinimumVersion, opt => opt.MapFrom(src => src.IsGlobalMinimum))
                  .ForMember(dest => dest.Status,
                             opt => opt.MapFrom(src => src.IsActive ? VersionStatus.Active : VersionStatus.Pending));

            Mapper.CreateMap<SoftwarePlatform, SoftwareReleaseProductModel>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Name));

            Mapper.CreateMap<CompanyEmailTemplate, EmailTemplateModel>()
                .ForMember(dest => dest.TemplateType, opt => opt.MapFrom(src => (EmailTemplateModel.EmailTemplateType)src.TemplateType))
                .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src.Header))
                .ForMember(dest => dest.Body, opt => opt.MapFrom(src => src.BaseBody));

            Mapper.CreateMap<EmailTemplateModel, CompanyEmailTemplate>()
                .ForMember(dest => dest.TemplateType, opt => opt.MapFrom(src => (int)src.TemplateType))
                .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src.Header))
                .ForMember(dest => dest.BaseBody, opt => opt.MapFrom(src => src.Body));

            Mapper.CreateMap<CompanyUsageCount, CompanyEncrypticsUsageModel>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Usage, opt => opt.MapFrom(src => src.Count))
                .ForMember(dest => dest.MonthUsed, opt => opt.MapFrom(src => src.DateOfCount));

            //Mapper.CreateMap<UserEmail, UserEmailModel>()
            //      .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.Subject))
            //      .ForMember(dest => dest.ProfileName, opt => opt.MapFrom(src => src.ProfileName))
            //      .ForMember(dest => dest.Sent, opt => opt.MapFrom(src => src.SentDate))
            //      .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            Mapper.CreateMap<AccessGroup, DistributionGroupModel>()
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.UserCount))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            Mapper.CreateMap<AccessGroupUser, DistributionGroupMemberModel>()
                .ForMember(dest => dest.GroupMemberId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

            Mapper.CreateMap<ImportUsersRequest, ImportFileModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ImportFileId))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FilePath))
                .ForMember(dest => dest.DateUploaded, opt => opt.MapFrom(src => src.CreatedDate));

            Mapper.CreateMap<ImportUsersAccount, InsertableUserAccountModel>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.ValidationError, opt => opt.MapFrom(src => src.ReasonForFailure));

            Mapper.CreateMap<UsageRightsGroup, UsageRightsGroupModel>()
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.UserCount))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            Mapper.CreateMap<UsageRightsGroupModel, UsageRightsGroup>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.GroupId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.GroupName))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            Mapper.CreateMap<UsageRightsGroup, EditableUsageRightsGroupModel>()
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.UserCount))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.EncryptCopy, opt => opt.MapFrom(src => src.EncryptCopy))
                .ForMember(dest => dest.EncryptForward, opt => opt.MapFrom(src => src.EncryptForward))
                .ForMember(dest => dest.EncryptPrint, opt => opt.MapFrom(src => src.EncryptPrint))
                .ForMember(dest => dest.EncryptSave, opt => opt.MapFrom(src => src.EncryptSave))
                .ForMember(dest => dest.EncryptSunrise, opt => opt.MapFrom(src => src.EncryptSunrise))
                .ForMember(dest => dest.EncryptSunset, opt => opt.MapFrom(src => src.EncryptSunset))
                .ForMember(dest => dest.DecryptCopy, opt => opt.MapFrom(src => src.DecryptCopy))
                .ForMember(dest => dest.DecryptForward, opt => opt.MapFrom(src => src.DecryptForward))
                .ForMember(dest => dest.DecryptPrint, opt => opt.MapFrom(src => src.DecryptPrint))
                .ForMember(dest => dest.DecryptSave, opt => opt.MapFrom(src => src.DecryptSave));

            Mapper.CreateMap<EditableUsageRightsGroupModel, UsageRightsGroup>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.GroupId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.GroupName))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.EncryptCopy, opt => opt.MapFrom(src => src.EncryptCopy))
                .ForMember(dest => dest.EncryptForward, opt => opt.MapFrom(src => src.EncryptForward))
                .ForMember(dest => dest.EncryptPrint, opt => opt.MapFrom(src => src.EncryptPrint))
                .ForMember(dest => dest.EncryptSave, opt => opt.MapFrom(src => src.EncryptSave))
                .ForMember(dest => dest.EncryptSunrise, opt => opt.MapFrom(src => src.EncryptSunrise))
                .ForMember(dest => dest.EncryptSunset, opt => opt.MapFrom(src => src.EncryptSunset))
                .ForMember(dest => dest.DecryptCopy, opt => opt.MapFrom(src => src.DecryptCopy))
                .ForMember(dest => dest.DecryptForward, opt => opt.MapFrom(src => src.DecryptForward))
                .ForMember(dest => dest.DecryptPrint, opt => opt.MapFrom(src => src.DecryptPrint))
                .ForMember(dest => dest.DecryptSave, opt => opt.MapFrom(src => src.DecryptSave));

            Mapper.CreateMap<UsageRightsGroupUser, UsageRightsGroupMemberModel>()
                .ForMember(dest => dest.GroupMemberId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));
        }
    }
}