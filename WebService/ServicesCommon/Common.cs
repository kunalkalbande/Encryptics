using System;
using System.Collections.Generic;

namespace ServicesCommon
{    
    public enum AccountRegistrationStatus
    {
        Failed = 0,
        Success = 1,
        Exists = 2
    }

    public class AccountRegistration
    {
        public AccountRegistration()
        {
            RegistrationSite = OriginSite.UNKNOWN;
        }

        public long? Id { get; set; }
        public string ActivationCode { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public int? PasswordIterations { get; set; }
        public string HWID { get; set; }
        public char Type { get; set; }
        public string PublicKey { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public int? DeviceType { get; set; }
        public string SecretAnswerSalt { get; set; }
        public int? SecretAnswerIterations { get; set; }
        public List<SecurityQuestion> SecurityQuestions { get; set; }
        public AccountRegistrationStatus Status { get; set; }
        public bool IsSoftwarePreinstalled { get; set; }
        public long? OverrideRequestID { get; set; }
        public OriginSite RegistrationSite { get; set; }
    }

    public enum AccountType
    {
        Professional = 0,
        Free = 1,
        Trial = 2
    }

    public class ActivationIdentifiers
    {
        public ActivationIdentifiers()
        {
            RegistrationSite = OriginSite.UNKNOWN;
        }

        public long RequestId { get; set; }
        public string ActivationCode { get; set; }
        public string LinkHash { get; set; }
        public int? LinkHashKeyID { get; set; }
        public bool IsSoftwarePreinstalled { get; set; }
        public long? InsertByAdminID { get; set; }
        public string FirstName { get; set; }
        public long EntityID { get; set; }
        public string EntityName { get; set; }
        public long? OverrideRequestID { get; set; }
        public bool PasswordExists { get; set; }
        public OriginSite RegistrationSite { get; set; }
        public DateTime LastResendDate { get; set; }
    }
    
    public enum OriginSite
    {
        UNKNOWN = 0,
        PORTAL = 1,
        WEBSITE = 2
    }
    
    public class SecurityQuestion
    {
        public SecurityQuestion()
        { }

        public SecurityQuestion(string question, int questionNumber)
        {
            QuestionText = question;
            QuestionNumber = questionNumber;
        }

        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; }
        public string AnswerHash { get; set; }
    }
    
    public class UserPasswordParameters
    {
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public int? Iterations { get; set; }
        public int? DefaultIterations { get; set; }
    }
    
}
