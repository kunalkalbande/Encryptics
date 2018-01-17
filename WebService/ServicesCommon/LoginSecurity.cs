﻿using System;

namespace ServicesCommon
{
    public class LoginSecurity
    {
        public static bool CheckLockout(string account_name)
        {
            if (string.IsNullOrEmpty(account_name))
                return false;

            return ServicesCommonDA.IsUserLockedOut(account_name);
        }

        public static bool CheckPassword(string account_name, string password, string new_password)
        {
            return CheckPassword(account_name, password, new_password, null, null, null);
        }

        public static bool CheckPassword(string account_name, string password, string new_password, string hwid, int? software_family, string software_version)
        {
            bool is_success = false;

            if (string.IsNullOrEmpty(account_name) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            long? userID = ServicesCommonDA.SelectIDByEmail(account_name);

            if (userID == null || userID <= 0)
            {
                InsertLoginAttempt(account_name, userID, hwid, software_family, software_version, false);
            }
            else
                is_success = CheckPassword((long)userID, password, new_password, hwid, software_family, software_version);
            
            return is_success;
        }
        
        public static bool CheckPassword(long user_id, string password, string new_password, string hwid, int? software_family, string software_version)
        {
            bool is_success;

            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    InsertLoginAttempt(null, user_id, hwid, software_family, software_version, false);
                    return false;
                }

                UserPasswordParameters userPassParams = ServicesCommonDA.GetUserPasswordParameters(user_id);

                if (userPassParams == null || string.IsNullOrEmpty(userPassParams.PasswordHash))
                {
                    InsertLoginAttempt(null, user_id, hwid, software_family, software_version, false);
                    return false;
                }

                // Uppercase the password. The portal always enters them this way, it appears to be the .NET standard. C++, etc languages use lowercase.
                string hash = password = password.ToUpper();

                // If parameters exist (salt & iterations) then uses PBKDF2
                if (string.IsNullOrEmpty(userPassParams.Salt) == false && userPassParams.Iterations != null)
                {
                    hash = PasswordHash.GetHash(password, userPassParams.Salt, (int)userPassParams.Iterations);
                }

                // Check generated/passed hash against database hash
                is_success = PasswordHash.SlowEquals(Convert.FromBase64String(hash), Convert.FromBase64String(userPassParams.PasswordHash));

                InsertLoginAttempt(null, user_id, hwid, software_family, software_version, is_success);
                
                // Check for a new password OR see if password upgrade is required
                // New password should be defined ONLY if we are logging in a user and requiring a password change simultaneously
                if (is_success &&
                    (userPassParams.DefaultIterations != null &&
                    (string.IsNullOrEmpty(new_password) == false ||
                    (userPassParams.Iterations == null || userPassParams.Iterations != userPassParams.DefaultIterations))))
                {
                    if (string.IsNullOrEmpty(new_password) == false)
                        password = new_password;

                    string newSalt;
                    string newHash = PasswordHash.CreateHash(password, (int)userPassParams.DefaultIterations, out newSalt);

                    bool result = ServicesCommonDA.UpdateUserPasswordParameters(user_id, hash, newHash, newSalt, (int)userPassParams.DefaultIterations);
                }
            }
            catch (Exception e)
            {
                ServicesCommonDA.LogAppRecord("LoginSecurity", "CheckPassword", "Exception: " + e.Message);
                return false;
            }

            return is_success;
        }
        
        // PRIVATE
        private static bool InsertLoginAttempt(string account_name, long? user_id, string hwid, int? software_family, string software_version, bool is_success)
        {
            return ServicesCommonDA.InsertLoginAttempt(account_name, user_id, hwid, software_family, software_version, is_success);
        }
        
    }
}
