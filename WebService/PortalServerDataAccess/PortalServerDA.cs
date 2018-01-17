using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using PortalCommon;
using ServicesCommon;

namespace PortalServerDataAccess
{
    public class PortalServerDA
    {
        public UserAccount GetUserAccount(long admin_entity_id, long admin_id, long entity_id, long user_id)
        {
            UserAccount userAcc = null;
            
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("port.GetUserAccount", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@AdminEntityID", admin_entity_id);
            comm.Parameters.AddWithValue("@AdminID", admin_id);
            comm.Parameters.AddWithValue("@EntityID", entity_id);
            comm.Parameters.AddWithValue("@UserID", user_id);

            SqlParameter parResult = new SqlParameter("@Result", SqlDbType.Int);
            parResult.Direction = ParameterDirection.Output;

            comm.Parameters.Add(parResult);
            
            SqlDataAdapter dataAdapter = new SqlDataAdapter(comm);
            DataSet ds = new DataSet();

            try
            {
                dataAdapter.Fill(ds);
                
                if (!(comm.Parameters["@Result"].Value is DBNull) && (int)comm.Parameters["@Result"].Value == 1 && ds.Tables.Count > 0)
                {
                    
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count == 1)
                    {
                        DataRow dr = dt.Rows[0];

                        userAcc = new UserAccount();
                        userAcc.Company = (string)dr["Company"];
                        userAcc.DepartmentId = (int)dr["DepartmentID"];
                        userAcc.EntityId = (long)dr["EntityID"];
                        userAcc.FName = (string)dr["FName"];
                        userAcc.Id = (long)dr["ID"];
                        userAcc.IsForcePasswordChange = (Boolean)dr["IsForcePasswordChange"];
                        userAcc.IsLockedOut = (Boolean)dr["IsLockedOut"];
                        userAcc.IsSuspended = (Boolean)dr["IsSuspended"];
                        userAcc.IsUsingTemporaryPassword = (Boolean)dr["IsUsingTemporaryPassword"];

                        if (!(dr["LastPasswordChange"] is DBNull))
                            userAcc.LastPasswordChange = ((DateTimeOffset)dr["LastPasswordChange"]).ToOffset(TimeSpan.Zero).DateTime;

                        userAcc.LName = (string)dr["LName"];
                        userAcc.Type = (AccountType)(int)dr["LicenseType"];
                        userAcc.UserName = (string)dr["Email"];
                        
                        if (!(dr["CreatedDate"] is DBNull))
                            userAcc.CreatedDate = ((DateTimeOffset)dr["CreatedDate"]).ToOffset(TimeSpan.Zero).DateTime;

                        if (!(dr["LicenseCreateDate"] is DBNull))
                            userAcc.LicenseCreatedDate = ((DateTimeOffset)dr["LicenseCreateDate"]).ToOffset(TimeSpan.Zero).DateTime;

                        if (!(dr["LicenseExpirationDate"] is DBNull))
                            userAcc.LicenseExpirationDate = ((DateTimeOffset)dr["LicenseExpirationDate"]).ToOffset(TimeSpan.Zero).DateTime;

                        userAcc.IsAwaitingApproval = dr["IsAwaitingApproval"] == DBNull.Value || Convert.ToBoolean(dr["IsAwaitingApproval"]) == false ? false : true;

                        userAcc.ContactInfo = new ContactInfo();
                        userAcc.ContactInfo.Address1 = (string)dr["Address1"];
                        userAcc.ContactInfo.Address2 = (string)dr["Address2"];
                        userAcc.ContactInfo.City = (string)dr["City"];
                        userAcc.ContactInfo.Country = (string)dr["Country"];
                        userAcc.ContactInfo.Email = (string)dr["Email"];
                        userAcc.ContactInfo.Fax = (string)dr["Fax"];
                        userAcc.ContactInfo.Mobile = (string)dr["Mobile"];
                        userAcc.ContactInfo.Phone = (string)dr["Phone"];
                        userAcc.ContactInfo.Region = (string)dr["Region"];
                        userAcc.ContactInfo.State = (string)dr["State"];
                        userAcc.ContactInfo.ZipCode = (string)dr["ZipCode"];
                        
                        userAcc.CountList = new UserAccountCountList
                        {
                            AliasCount = (int)dr["AliasCount"],
                            DeviceCount = (int)dr["DeviceCount"],
                            EncryptionCount = (int)dr["EncryptionCount"]
                        };
                        
                        DataTable dtRoles = ds.Tables[1];
                        
                        if (dtRoles != null && dtRoles.Rows.Count >= 1)
                        {
                            DataRow drRole = dtRoles.Rows[0];

                            userAcc.Role = new PortalRoleListItem { Id = (long)drRole["RoleID"], Title = (string)drRole["Title"] };
                            
                        }
                        
                    }
                    
                }
            }
            catch (Exception e)
            {
                LogAppRecord("PortalServerDA", "GetUserAccount", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return userAcc;
        }
        
        public UserIdentifiers GetUserIdentifiers(string email, string password)
        {
            UserIdentifiers userIds = null;

            if (string.IsNullOrEmpty(email))
                return null;

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("port.GetUserIdentifiers", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@Email", email);

            if (string.IsNullOrEmpty(password) == false)
                comm.Parameters.AddWithValue("@Password", password);

            SqlParameter paramUserID = new SqlParameter("@UserID", SqlDbType.BigInt);
            SqlParameter paramEntityID = new SqlParameter("@EntityID", SqlDbType.BigInt);
            paramUserID.Direction = ParameterDirection.Output;
            paramEntityID.Direction = ParameterDirection.Output;

            comm.Parameters.Add(paramUserID);
            comm.Parameters.Add(paramEntityID);

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                if (comm.Parameters["@UserID"].Value != DBNull.Value && comm.Parameters["@EntityID"].Value != DBNull.Value)
                {
                    userIds = new UserIdentifiers
                    {
                        Id = (long) comm.Parameters["@UserID"].Value,
                        EntityId = (long) comm.Parameters["@EntityID"].Value
                    };

                }
            }
            catch (Exception e)
            {
                LogAppRecord("PortalServerDA", "GetUserIdentifiers", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return userIds;
        }
        
        public void LogAppRecord(string classInfo, string methodInfo, string logInfo)
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

        public bool ValidateToken(string token, long user_id)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("port.ValidateToken", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@UserID", user_id);
            comm.Parameters.AddWithValue("@Token", token);

            SqlParameter parResult = new SqlParameter("@Result", SqlDbType.Bit);
            parResult.Direction = ParameterDirection.Output;

            comm.Parameters.Add(parResult);

            bool result = false;

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                if ((Boolean)comm.Parameters["@Result"].Value)
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                LogAppRecord("PortalServerDA", "ValidateToken", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return result;
        }
    }
}
