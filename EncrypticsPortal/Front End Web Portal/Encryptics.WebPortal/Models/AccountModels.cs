using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Web.Mvc;
using MyResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Models
{
    internal struct RegularExpressionPatterns
    {
        public const string EMAIL_ADDRESS_FORMAT = @"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$";
        public const string PASSWORD_FORMAT = @"^.*(?=.{8,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).*$";
    }

    public class UsersContext : DbContext
    {
        public UsersContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
    }

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public class RegisterExternalLoginModel
    {
        [Required(ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "EmailAddressRequiredErrorMessage")]
        [Display(ResourceType = typeof(MyResources), Name = "UserNameDisplay")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    //public class LocalPasswordModelBinder : IModelBinder
    //{
    //    public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
    //    {
    //        return new LocalPasswordModelBinder();
    //    }
    //}

    //[ModelBinder(typeof(LocalPasswordModelBinder))]
    public class LocalPasswordModel
    {
        [Required(ErrorMessageResourceType = typeof(MyResources), 
            ErrorMessageResourceName = "CurrentPasswordRequiredErrorMessage")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(MyResources), Name = "CurrentPasswordDisplay")]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(MyResources), 
            ErrorMessageResourceName = "NewPasswordRequiredErrorMessage")]
        [StringLength(100, ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "PasswordMinimumLengthErrorMessage", MinimumLength = 8)]
        [RegularExpression(RegularExpressionPatterns.PASSWORD_FORMAT,
            ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "ErrorMessagePasswordFormat")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(MyResources), Name = "NewPasswordDisplay")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(MyResources), Name = "ConfirmNewPasswordDisplay")]
        [System.Web.Mvc.Compare("NewPassword", 
            ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "PasswordNotMatchingErrorMessage")]
        public string ConfirmPassword { get; set; }

        [JsonProperty("g-recaptcha-response")]
        public string CaptchaResponse { get; set; }
    }

    public class LoginModel
    {
        [Required(ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "EmailAddressRequiredErrorMessage")]
        [Display(ResourceType = typeof(MyResources), Name = "UserNameDisplay")]
        [JsonProperty("AccountName")]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "PasswordRequiredErrorMessage")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(MyResources), Name = "PasswordDisplay")]
        public string Password { get; set; }

        [Display(ResourceType = typeof(MyResources), Name = "RememberMeDisplay")]
        public bool RememberMe { get; set; }

        [JsonProperty("Result")]
        public string Tenant { get; set; }
        [JsonIgnore]
        public bool PasswordVisible { get;  set; }
    }

    public class ResendActivationModel
    {
        [Required]
        [Display(ResourceType = typeof(MyResources), Name = "UserNameDisplay")]
        public string UserName { get; set; }

        public bool Resent { get; set; }

        public bool IsCaptchaValid { get; set; }
    }

    public class RegisterModel
    {
        [Required(ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "EmailAddressRequiredErrorMessage")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(RegularExpressionPatterns.EMAIL_ADDRESS_FORMAT,
            ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "InvalidEmailAddressFormatErrorMessage")]
        [Display(ResourceType = typeof(MyResources), Name = "UserNameDisplay")]
        public string UserName { get; set; }

        //[Required]
        [Display(ResourceType = typeof(MyResources), Name = "FirstNameDisplay")]
        public string FirstName { get; set; }

        //[Required]
        [Display(ResourceType = typeof(MyResources), Name = "LastNameDisplay")]
        public string LastName { get; set; }

        [Required(ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "PasswordRequiredErrorMessage")]
        [StringLength(100, ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "PasswordMinimumLengthErrorMessage", MinimumLength = 8)]
        [RegularExpression(RegularExpressionPatterns.PASSWORD_FORMAT, 
            ErrorMessageResourceType = typeof(MyResources), 
            ErrorMessageResourceName = "ErrorMessagePasswordFormat")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(MyResources), Name = "PasswordDisplay")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(MyResources), Name = "ConfirmPasswordDisplay")]
        [System.Web.Mvc.Compare("Password", ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "PasswordNotMatchingErrorMessage")]
        public string ConfirmPassword { get; set; }

        //[Required]
        [Display(ResourceType = typeof(MyResources), Name = "SecurityQuestion1Display")]
        public string SecurityQuestion01 { get; set; }

        //[Required]
        [Display(ResourceType = typeof(MyResources), Name = "SecurityQuestion1AnswerDisplay")]
        public string SecurityAnswer01 { get; set; }

        //[Required]
        [Display(ResourceType = typeof(MyResources), Name = "SecurityQuestion2Display")]
        public string SecurityQuestion02 { get; set; }

        //[Required]
        [Display(ResourceType = typeof(MyResources), Name = "SecurityQuestion2AnswerDisplay")]
        public string SecurityAnswer02 { get; set; }

        //[Required]
        [Display(ResourceType = typeof(MyResources), Name = "SecurityQuestion3Display")]
        public string SecurityQuestion03 { get; set; }

        //[Required]
        [Display(ResourceType = typeof(MyResources), Name = "SecurityQuestion3AnswerDisplay")]
        public string SecurityAnswer03 { get; set; }

        [Required]
        [BooleanRequired(ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "AgreeToTermsErrorMessage")]
        [Display(ResourceType = typeof(MyResources), Name = "EulaAcceptanceDisplay")]
        public bool UserAcceptsEULA { get; set; }

        //[Required]
        //[Display(Name = "User name")]
        //public string UserName { get; set; }

        //[Required]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        //[DataType(DataType.Password)]
        //[Display(Name = "Password")]
        //public string Password { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Confirm password")]
        //[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        //public string ConfirmPassword { get; set; }
    }

    public class ActivationWithPasswordModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string ActivationCode { get; set; }

        [Required]
        public string Hash { get; set; }

        [Required(ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "PasswordRequiredErrorMessage")]
        [StringLength(100, ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "PasswordMinimumLengthErrorMessage", MinimumLength = 8)]
        [RegularExpression(RegularExpressionPatterns.PASSWORD_FORMAT, 
            ErrorMessageResourceType = typeof(MyResources), 
            ErrorMessageResourceName = "ErrorMessagePasswordFormat")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(MyResources), Name = "NewPasswordDisplay")]
        //[XmlIgnore]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(MyResources), Name = "ConfirmPasswordDisplay")]
        [System.ComponentModel.DataAnnotations.Compare("Password", 
            ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "PasswordNotMatchingErrorMessage")]
        //[XmlIgnore]
        public string ConfirmPassword { get; set; }
    }

    public class ForgotPasswordModel
    {
        [Required(ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "EmailAddressRequiredErrorMessage")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(RegularExpressionPatterns.EMAIL_ADDRESS_FORMAT,
            ErrorMessageResourceType = typeof(MyResources),
            ErrorMessageResourceName = "InvalidEmailAddressFormatErrorMessage")]
        [Display(ResourceType = typeof(MyResources), Name = "UserNameDisplay")]
        public string UserName { get; set; }

        /*[Display(ResourceType = typeof (Resources), Name = "SecretQuestionAnswerDisplay")]
        public string Answer { get; set; }

        [Display(ResourceType = typeof (Resources), Name = "SecretQuestionDisplay7")]
        public string SecretQuestion { get; set; }*/
    }

    public class ResetPasswordModel : ActivationWithPasswordModel
    {

    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }

    public class BooleanRequired : RequiredAttribute, IClientValidatable
    {
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata,
                                                                               ControllerContext context)
        {
            return new[] { new ModelClientValidationRule { ValidationType = "mandatory", ErrorMessage = ErrorMessage } };
        }

        public override bool IsValid(object value)
        {
            return value != null && (bool)value;
        }
    }

    public enum ResetPasswordStatusError
    {
        [Display(ResourceType = typeof(MyResources), Name = "ResetPasswordLinkStatusFailedErrorMessage")]
        Failed,
        Pending,
        [Display(ResourceType = typeof(MyResources), Name = "ResetPasswordLinkStatusBadDataErrorMessage")]
        BadData,
        [Display(ResourceType = typeof(MyResources), Name = "ResetPasswordLinkStatusExpiredErrorMessage")]
        Expired,
        [Display(ResourceType = typeof(MyResources), Name = "ResetPasswordLinkStatusUsedErrorMessage")]
        Used
    }

    public enum AccountActivationStatusError
    {
        [Display(ResourceType = typeof(MyResources), Name = "AccountActivationStatusFailedErrorMessage")]
        Pending,
        [Display(ResourceType = typeof(MyResources), Name = "AccountActivationStatusUsedErrorMessage")]
        Used,
        [Display(ResourceType = typeof(MyResources), Name = "AccountActivationStatusRemovedErrorMessage")]
        Removed,
        [Display(ResourceType = typeof(MyResources), Name = "AccountActivationStatusExpiredErrorMessage")]
        Expired,
        [Display(ResourceType = typeof(MyResources), Name = "AccountActivationStatusRemovedErrorMessage")]
        Replaced,
        [Display(ResourceType = typeof(MyResources), Name = "AccountActivationStatusFailedErrorMessage")]
        Failed,
        [Display(ResourceType = typeof(MyResources), Name = "AccountActivationStatusBadDataErrorMessage")]
        BadData
    }

}