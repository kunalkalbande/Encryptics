using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using PortalCommon;
using ServicesCommon;

namespace PortalServerDataAccess
{
    public class PortalServerDA
    {
        public bool ExpireTokenSession(long admin_entity_id, long admin_id, long entity_id, long user_id, long? token_id, string token_guid)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("port.ExpireTokenSession", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@AdminEntityID", admin_entity_id);
            comm.Parameters.AddWithValue("@AdminID", admin_id);
            comm.Parameters.AddWithValue("@EntityID", entity_id);
            comm.Parameters.AddWithValue("@UserID", user_id);

            if (token_id != null)
                comm.Parameters.AddWithValue("@TokenID", token_id);
            else if (string.IsNullOrEmpty(token_guid) == false)
                comm.Parameters.AddWithValue("@TokenGUID", token_guid);

            SqlParameter paramResult = new SqlParameter("@Result", SqlDbType.Bit);
            paramResult.Direction = ParameterDirection.Output;

            comm.Parameters.Add(paramResult);

            bool isSuccess = false;

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                if (comm.Parameters["@Result"].Value != DBNull.Value && (Boolean)comm.Parameters["@Result"].Value)
                    isSuccess = true;

            }
            catch (Exception e)
            {
                LogAppRecord("PortalServerDA", "ExpireTokenSession", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return isSuccess;
        }

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

        public bool GetUserAuthorizedAction(long admin_entity_id, long admin_id, long entity_id, long user_id, string action)
        {
            bool result = false;

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("port.GetUserAuthorizedAction", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@AdminEntityID", admin_entity_id);
            comm.Parameters.AddWithValue("@AdminID", admin_id);
            comm.Parameters.AddWithValue("@EntityID", entity_id);
            comm.Parameters.AddWithValue("@UserID", user_id);
            comm.Parameters.AddWithValue("@Action", action);

            SqlParameter parIsAuthorized = new SqlParameter("@IsAuthorized", SqlDbType.Bit);
            parIsAuthorized.Direction = ParameterDirection.Output;

            comm.Parameters.Add(parIsAuthorized);

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                if (!(comm.Parameters["@IsAuthorized"].Value is DBNull) && (Boolean)comm.Parameters["@IsAuthorized"].Value)
                    result = true;
            }
            catch (Exception e)
            {
                LogAppRecord("PortalServerDA", "GetUserAuthorizedAction", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return result;
        }

        public List<AuthorizedAction> GetUserAuthorizedActions(long admin_entity_id, long admin_id, long entity_id, long user_id, string[] actions)
        {
            List<AuthorizedAction> result = null;

            string actionList = string.Join(",", actions);

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("port.GetUserAuthorizedActions", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@AdminEntityID", admin_entity_id);
            comm.Parameters.AddWithValue("@AdminID", admin_id);
            comm.Parameters.AddWithValue("@EntityID", entity_id);
            comm.Parameters.AddWithValue("@UserID", user_id);
            comm.Parameters.AddWithValue("@ActionList", actionList);

            SqlParameter parResult = new SqlParameter("@Result", SqlDbType.Int);
            parResult.Direction = ParameterDirection.Output;

            comm.Parameters.Add(parResult);

            SqlDataAdapter dataAdapter = new SqlDataAdapter(comm);
            DataTable dt = new DataTable();

            try
            {
                dataAdapter.Fill(dt);

                if (!(comm.Parameters["@Result"].Value is DBNull) && (int)comm.Parameters["@Result"].Value == 1)
                {
                    result = new List<AuthorizedAction>();

                    foreach (DataRow dr in dt.Rows)
                        result.Add(new AuthorizedAction { Action = dr["URI"].ToString(), IsAuthorized = (Boolean)dr["IsAuthorized"] });

                }
            }
            catch (Exception e)
            {
                LogAppRecord("PortalServerDA", "GetUserAuthorizedActions", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return result;
        }

        public List<CompanyListItem> GetUserCompanies(long admin_entity_id, long admin_id, long user_id, CompanyStatus company_status)
        {
            List<CompanyListItem> userCompanies = new List<CompanyListItem>();

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("port.GetUserCompanies", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@AdminEntityID", admin_entity_id);
            comm.Parameters.AddWithValue("@AdminID", admin_id);
            comm.Parameters.AddWithValue("@UserID", user_id);
            comm.Parameters.AddWithValue("@CompanyStatus", (int)company_status);

            SqlParameter parResult = new SqlParameter("@Result", SqlDbType.Int);
            parResult.Direction = ParameterDirection.Output;

            comm.Parameters.Add(parResult);

            SqlDataAdapter dataAdapter = new SqlDataAdapter(comm);
            DataTable dt = new DataTable();

            try
            {
                dataAdapter.Fill(dt);

                if (!(comm.Parameters["@Result"].Value is DBNull) && (int)comm.Parameters["@Result"].Value == 1 && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        CompanyListItem companyItem = new CompanyListItem { Id = (long)dr["ID"], Name = (string)dr["Name"], Role = (RoleType)(int)dr["UserRole"], IsActive = (Boolean)dr["IsActive"] };

                        companyItem.LicensingInfo = new LicensingInfo
                        {
                            AvailableLicenses = (int)dr["AvailableLicenses"],
                            ActiveLicenses = (int)dr["ActiveLicenses"],
                            UsedLicenses = (int)dr["UsedLicenses"]
                        };

                        userCompanies.Add(companyItem);
                    }
                }
            }
            catch (Exception e)
            {
                LogAppRecord("PortalServerDA", "GetUserCompanies", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return userCompanies;
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

        public UserIdentifiers GetUserIdentifiers(string token)
        {
            UserIdentifiers userIds = null;

            if (string.IsNullOrEmpty(token))
                return null;

            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("port.GetUserIdentifiersByToken", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@Token", token);

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
                        Id = (long)comm.Parameters["@UserID"].Value,
                        EntityId = (long)comm.Parameters["@EntityID"].Value
                    };
                }
            }
            catch (Exception e)
            {
                LogAppRecord("PortalServerDA", "GetUserIdentifiers 2", "Exception: " + e.Message);
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

        public bool UpdateUserCompany(long admin_entity_id, long admin_id, long entity_id, long user_id, long new_entity_id, bool transfer_licenses)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("port.UpdateUserCompany", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@AdminEntityID", admin_entity_id);
            comm.Parameters.AddWithValue("@AdminID", admin_id);
            comm.Parameters.AddWithValue("@EntityID", entity_id);
            comm.Parameters.AddWithValue("@UserID", user_id);
            comm.Parameters.AddWithValue("@NewEntityID", new_entity_id);
            comm.Parameters.AddWithValue("@TransferLicenses", transfer_licenses);

            SqlParameter paramResult = new SqlParameter("@Result", SqlDbType.Int);
            paramResult.Direction = ParameterDirection.Output;

            comm.Parameters.Add(paramResult);

            bool isSuccess = false;

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                if (comm.Parameters["@Result"].Value != DBNull.Value)
                    isSuccess = Convert.ToBoolean(comm.Parameters["@Result"].Value);
            }
            catch (Exception e)
            {
                LogAppRecord("PortalServerDA", "UpdateUserCompany", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return isSuccess;
        }

        public bool UpdateUserContactInfo(long user_id, string first_name, string last_name, ContactInfo user_contact_info)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand comm = new SqlCommand("port.UpdateUserContactInfo", conn);
            comm.CommandType = CommandType.StoredProcedure;

            comm.Parameters.AddWithValue("@UserID", user_id);
            comm.Parameters.AddWithValue("@FirstName", first_name);
            comm.Parameters.AddWithValue("@LastName", last_name);

            if (user_contact_info != null)
            {
                comm.Parameters.AddWithValue("@Email", user_contact_info.Email == null ? (object)DBNull.Value : user_contact_info.Email);
                comm.Parameters.AddWithValue("@Phone", user_contact_info.Phone == null ? (object)DBNull.Value : user_contact_info.Phone);
                comm.Parameters.AddWithValue("@Mobile", user_contact_info.Mobile == null ? (object)DBNull.Value : user_contact_info.Mobile);
                comm.Parameters.AddWithValue("@Fax", user_contact_info.Fax == null ? (object)DBNull.Value : user_contact_info.Fax);
                comm.Parameters.AddWithValue("@Address1", user_contact_info.Address1 == null ? (object)DBNull.Value : user_contact_info.Address1);
                comm.Parameters.AddWithValue("@Address2", user_contact_info.Address2 == null ? (object)DBNull.Value : user_contact_info.Address2);
                comm.Parameters.AddWithValue("@City", user_contact_info.City == null ? (object)DBNull.Value : user_contact_info.City);
                comm.Parameters.AddWithValue("@State", user_contact_info.State == null ? (object)DBNull.Value : user_contact_info.State);
                comm.Parameters.AddWithValue("@ZipCode", user_contact_info.ZipCode == null ? (object)DBNull.Value : user_contact_info.ZipCode);
                comm.Parameters.AddWithValue("@Region", user_contact_info.Region == null ? (object)DBNull.Value : user_contact_info.Region);
                comm.Parameters.AddWithValue("@Country", user_contact_info.Country == null ? (object)DBNull.Value : user_contact_info.Country);
            }

            SqlParameter paramResult = new SqlParameter("@Result", SqlDbType.Bit);
            paramResult.Direction = ParameterDirection.Output;

            comm.Parameters.Add(paramResult);

            bool isSuccess = false;

            try
            {
                conn.Open();
                comm.ExecuteNonQuery();

                if (comm.Parameters["@Result"].Value != DBNull.Value)
                    isSuccess = Convert.ToBoolean(comm.Parameters["@Result"].Value);
            }
            catch (Exception e)
            {
                LogAppRecord("PortalServerDA", "UpdateUserContactInfo", "Exception: " + e.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return isSuccess;
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
