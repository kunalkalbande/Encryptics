using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace ServicesCommon
{
    public class ServicesCommonDA
    {
        public enum TokenRole
        {
            NONE = 0,
            PORTAL = 1,
            DEVICE = 2
        }

        public static int AuthenticateToken(string token)
        {
            string newToken;

            return AuthenticateToken(token, null, out newToken);
        }

        public static int AuthenticateToken(string token, string hwid)
        {
            string newToken;

            return AuthenticateToken(token, hwid, out newToken);
        }

        public static int AuthenticateToken(string token, string hwid, out string newToken)
        {
            newToken = null;

            return AuthenticateToken(token, string.Empty, hwid, out newToken);
        }

        public static int AuthenticateToken(string token, string email, string hwid)
        {
            string newToken;

            return AuthenticateToken(token, email, hwid, out newToken);
        }

        public static int AuthenticateToken(string token, string email, string hwid, out string newToken)
        {
            newToken = null;

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("dbo.AuthenticateToken", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@Token", token);

            if (string.IsNullOrEmpty(hwid) == false)
                comm.Parameters.AddWithValue("@HWID", hwid);

            if (string.IsNullOrEmpty(email) == false)
                comm.Parameters.AddWithValue("@Email", email);

            comm.Parameters.AddWithValue("@IPAddress", GetUserComboIPAddress());

            SqlParameter paramStatusCode = new SqlParameter("@StatusCode", SqlDbType.Int);
            SqlParameter paramNewToken = new SqlParameter("@NewToken", SqlDbType.VarChar, 64);
            paramStatusCode.Direction = ParameterDirection.Output;
            paramNewToken.Direction = ParameterDirection.Output;

            comm.Parameters.Add(paramStatusCode);
            comm.Parameters.Add(paramNewToken);

            int statusCode = 0;

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                if (comm.Parameters["@StatusCode"].Value != DBNull.Value)
                    statusCode = (int)comm.Parameters["@StatusCode"].Value;

                if (comm.Parameters["@NewToken"].Value != DBNull.Value)
                    newToken = (string)comm.Parameters["@NewToken"].Value;

            }
            catch (Exception e)
            {
                LogAppRecord("ServicesCommonDA", "AuthenticateToken", "Exception: " + e.Message);
                statusCode = 201;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return statusCode;
        }

        public static string GenerateSessionToken(string email, string hwid, TokenRole role)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("dbo.GenerateSessionToken", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@Email", email);

            if (hwid != null && hwid != string.Empty)
                comm.Parameters.AddWithValue("@HWID", hwid);

            comm.Parameters.AddWithValue("@RoleID", (int)role);

            comm.Parameters.AddWithValue("@IPAddress", GetUserComboIPAddress());

            SqlParameter paramID = new SqlParameter("@Token", SqlDbType.VarChar, 36);
            paramID.Direction = ParameterDirection.Output;

            comm.Parameters.Add(paramID);

            string token = null;

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                if (comm.Parameters["@Token"].Value != DBNull.Value)
                    token = (string)comm.Parameters["@Token"].Value;

            }
            catch (Exception e)
            {
                LogAppRecord("ServicesCommonDA", "GenerateSessionToken", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return token;
        }
        
        public static UserPasswordParameters GetUserPasswordParameters(long user_id)
        {
            UserPasswordParameters userPassParams = null;

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("dbo.GetUserPasswordParameters", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@UserID", user_id);

            SqlParameter paramPassword = new SqlParameter("@Password", SqlDbType.VarChar, 128);
            SqlParameter paramPasswordSalt = new SqlParameter("@PasswordSalt", SqlDbType.VarChar, 64);
            SqlParameter paramPasswordIterations = new SqlParameter("@PasswordIterations", SqlDbType.Int);
            SqlParameter paramDefaultIterations = new SqlParameter("@DefaultIterations", SqlDbType.Int);
            paramPassword.Direction = ParameterDirection.Output;
            paramPasswordSalt.Direction = ParameterDirection.Output;
            paramPasswordIterations.Direction = ParameterDirection.Output;
            paramDefaultIterations.Direction = ParameterDirection.Output;

            comm.Parameters.Add(paramPassword);
            comm.Parameters.Add(paramPasswordSalt);
            comm.Parameters.Add(paramPasswordIterations);
            comm.Parameters.Add(paramDefaultIterations);

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                // DefaultIterations or PasswordIterations can be NULL, but not both
                if (comm.Parameters["@Password"].Value != DBNull.Value &&
                    (comm.Parameters["@DefaultIterations"].Value != DBNull.Value || comm.Parameters["@PasswordIterations"].Value != DBNull.Value))
                {
                    userPassParams = new UserPasswordParameters();

                    if (comm.Parameters["@Password"].Value != DBNull.Value)
                        userPassParams.PasswordHash = (string)comm.Parameters["@Password"].Value;

                    if (comm.Parameters["@DefaultIterations"].Value != DBNull.Value)
                        userPassParams.DefaultIterations = (int)comm.Parameters["@DefaultIterations"].Value;

                    if (comm.Parameters["@PasswordSalt"].Value != DBNull.Value)
                        userPassParams.Salt = (string)comm.Parameters["@PasswordSalt"].Value;

                    if (comm.Parameters["@PasswordIterations"].Value != DBNull.Value)
                        userPassParams.Iterations = (int)comm.Parameters["@PasswordIterations"].Value;
                }
            }
            catch (Exception e)
            {
                LogAppRecord("ServicesCommonDA", "GetUserPasswordParameters", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return userPassParams;
        }
        
        public static bool InsertLoginAttempt(string account_name, long? user_id, string hwid, int? software_family, string software_version, bool is_success)
        {
            bool result = false;

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("dbo.InsertLoginAttempt", conn);
            comm.CommandType = CommandType.StoredProcedure;

            if (string.IsNullOrEmpty(account_name) == false)
                comm.Parameters.AddWithValue("@AccountName", account_name);

            if (user_id != null)
                comm.Parameters.AddWithValue("@UserID", user_id);

            if (string.IsNullOrEmpty(hwid) == false)
                comm.Parameters.AddWithValue("@HWID", hwid);

            if (software_family != null)
                comm.Parameters.AddWithValue("@SoftwareFamily", software_family);

            if (string.IsNullOrEmpty(software_version) == false)
                comm.Parameters.AddWithValue("@SoftwareVersion", software_version);

            comm.Parameters.AddWithValue("@IsSuccess", is_success);

            SqlParameter parResult = new SqlParameter("@Result", SqlDbType.Bit);

            parResult.Direction = ParameterDirection.Output;

            comm.Parameters.Add(parResult);

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                if ((bool)comm.Parameters["@Result"].Value)
                    result = true;
            }
            catch (Exception e)
            {
                LogAppRecord("ServicesCommonDA", "InsertLoginAttempt", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return result;
        }

        public static bool InsertServiceRequest(string operation, string identifier, string nonce)
        {
            bool result = false;

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("dbo.InsertServiceRequest", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@Operation", operation);
            comm.Parameters.AddWithValue("@Identifier", identifier);
            comm.Parameters.AddWithValue("@Nonce", nonce);

            SqlParameter parResult = new SqlParameter("@Result", SqlDbType.Bit);

            parResult.Direction = ParameterDirection.Output;

            comm.Parameters.Add(parResult);

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                if ((bool)comm.Parameters["@Result"].Value)
                    result = true;
            }
            catch (Exception e)
            {
                LogAppRecord("ServicesCommonDA", "InsertServiceRequest", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return result;
        }
        
        public static bool IsExistingServiceRequest(string operation, string identifier, string nonce)
        {
            bool result = false;

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("dbo.IsExistingServiceRequest", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@Operation", operation);
            comm.Parameters.AddWithValue("@Identifier", identifier);
            comm.Parameters.AddWithValue("@Nonce", nonce);

            SqlParameter parResult = new SqlParameter("@Result", SqlDbType.Bit);

            parResult.Direction = ParameterDirection.Output;

            comm.Parameters.Add(parResult);

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                if ((bool)comm.Parameters["@Result"].Value)
                    result = true;
            }
            catch (Exception e)
            {
                LogAppRecord("ServicesCommonDA", "IsExistingServiceRequest", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return result;
        }

        public static bool IsUserLockedOut(string email)
        {
            bool result = false;

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("dbo.IsUserLockedOut", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@Email", email);

            SqlParameter parResult = new SqlParameter("@Result", SqlDbType.Bit);

            parResult.Direction = ParameterDirection.Output;

            comm.Parameters.Add(parResult);

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                if ((bool)comm.Parameters["@Result"].Value)
                    result = true;
            }
            catch (Exception e)
            {
                LogAppRecord("ServicesCommonDA", "IsUserLockedOut", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return result;
        }

        public static void LogAppRecord(string classInfo, string methodInfo, string logInfo)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("dbo.InsertAppLogRecord", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@Class", classInfo);
            comm.Parameters.AddWithValue("@Method", methodInfo);
            comm.Parameters.AddWithValue("@LogInfo", logInfo);

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public static long SelectIDByEmail(string email)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("dbo.SelectIDByEmail", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@Email", email);

            SqlParameter paramID = new SqlParameter("@ID", SqlDbType.BigInt);
            paramID.Direction = ParameterDirection.Output;

            comm.Parameters.Add(paramID);

            long userID = -1;

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                if (comm.Parameters["@ID"].Value != DBNull.Value)
                    userID = Convert.ToInt64(comm.Parameters["@ID"].Value);

            }
            catch (Exception e)
            {
                LogAppRecord("ServicesCommonDA", "SelectIDByEmail", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return userID;
        }

        public static bool UpdateTokenExpDate(string token, int? lifespan_mins)
        {
            bool result = false;

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("dbo.UpdateTokenExpDate", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@Token", token);

            if (lifespan_mins != null)
                comm.Parameters.AddWithValue("@Mins", lifespan_mins);

            SqlParameter paramResult = new SqlParameter("@Result", SqlDbType.Int);
            paramResult.Direction = ParameterDirection.Output;

            comm.Parameters.Add(paramResult);

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                if (comm.Parameters["@Result"].Value != DBNull.Value && Convert.ToInt32(comm.Parameters["@Result"].Value) == 1)
                    result = true;
            }
            catch (Exception e)
            {
                LogAppRecord("ServicesCommonDA", "UpdateTokenExpDate", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return result;
        }

        public static bool UpdateUserPasswordParameters(long user_id, string old_password, string new_password, string password_salt, int password_iterations)
        {
            bool isSuccess = false;

            if (string.IsNullOrEmpty(old_password) || string.IsNullOrEmpty(new_password) || string.IsNullOrEmpty(password_salt) || password_iterations <= 0)
                return false;

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("dbo.UpdateUserPasswordParameters", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@UserID", user_id);
            comm.Parameters.AddWithValue("@OldPassword", old_password);
            comm.Parameters.AddWithValue("@NewPassword", new_password);
            comm.Parameters.AddWithValue("@PasswordSalt", password_salt);
            comm.Parameters.AddWithValue("@PasswordIterations", password_iterations);

            SqlParameter paramResult = new SqlParameter("@Result", SqlDbType.Bit);
            paramResult.Direction = ParameterDirection.Output;

            comm.Parameters.Add(paramResult);

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                if (comm.Parameters["@Result"].Value != DBNull.Value)
                    isSuccess = Convert.ToBoolean(comm.Parameters["@Result"].Value);
            }
            catch (Exception e)
            {
                LogAppRecord("ServicesCommonDA", "UpdateUserPasswordParameters", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return isSuccess;
        }
        
        // PRIVATE
        private static string GetUserComboIPAddress()
        {
            return getUserIP_HTTP_X_FORWARDED_FOR() + "," + getUserIP_UserHostAddress();
        }

        private static string getUserIP_HTTP_X_FORWARDED_FOR()
        {
            if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            {
                return HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            }
            return "null";
        }

        private static string getUserIP_UserHostAddress()
        {
            if (HttpContext.Current.Request.UserHostAddress != null && HttpContext.Current.Request.UserHostAddress.Length != 0)
            {
                return HttpContext.Current.Request.UserHostAddress;
            }
            return "null";
        }

        public static string GetTenantIdByEmail(string email)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("dbo.GetTenantIdByEmail", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@Email", email);

            SqlParameter paramTenantId = new SqlParameter("@TenantId", SqlDbType.NVarChar, 120);
            paramTenantId.Direction = ParameterDirection.Output;

            comm.Parameters.Add(paramTenantId);

            string TenantId = string.Empty;

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                if (comm.Parameters["@TenantId"].Value != DBNull.Value)
                    TenantId = Convert.ToString(comm.Parameters["@TenantId"].Value);

            }
            catch (Exception e)
            {
                LogAppRecord("ServicesCommonDA", "SelectIDByEmail", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return TenantId;
        }

    }

}
