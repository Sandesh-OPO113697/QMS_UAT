using QMS.Encription;
using System.Data.SqlClient;
using System.Data;
using QMS.Models;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Drawing;

namespace QMS.DataBaseService
{
    public class DL_SuperAdmin
    {

        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dcl;
        public DL_SuperAdmin(IConfiguration configuration, DL_Encrpt dL_Encrpt, DLConnection dL)
        {
            _con = configuration.GetConnectionString("Master_Con");
            _enc = dL_Encrpt;
            _dcl = dL;
        }
        public async Task CreateAccountByScriptAsync(string accountName)
        {
            try
            {
                string connectionString = await _enc.DecryptAsync(_con);
                string createDatabaseQuery =
                    $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{accountName}') CREATE DATABASE {accountName}";
                string sqlScript = await File.ReadAllTextAsync(@"D:\Script\Account_Script_QMS.sql");

                string[] batches = sqlScript.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    await ExecuteQueryAsync(conn, createDatabaseQuery);
                    string useDatabaseQuery = $"USE {accountName};";
                    await ExecuteQueryAsync(conn, useDatabaseQuery);

                    foreach (var batch in batches)
                    {
                        string trimmedBatch = batch.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmedBatch))
                        {
                            await ExecuteQueryAsync(conn, trimmedBatch);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }




        private async Task ExecuteQueryAsync(SqlConnection connection, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));
            }

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.CommandText = query; // Ensure CommandText is set properly
                await cmd.ExecuteNonQueryAsync();
            }
        }
 
        public async Task InsertAccountAsync(string accountName, string prefix, string signOn)
        {
            string prefixEnc = await _enc.EncryptAsync(prefix);
            string connectionString = await _enc.DecryptAsync(_con);

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("Sp_CreateAccount", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AccountName", await _enc.EncryptAsync(accountName));
                cmd.Parameters.AddWithValue("@SignOn", await _enc.EncryptAsync(signOn));
                cmd.Parameters.AddWithValue("@Prefix", prefixEnc);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }


        public async Task<bool> ExecuteQueryToCheckPrefixAsync(string prefixEnc)
        {
            string PrixicENC = await _enc.EncryptAsync(prefixEnc);
            string STR = await _enc.DecryptAsync(_con);
            bool result = false;

            using (SqlConnection conn = new SqlConnection(STR))
            {
                string query = "sp_Superadmin";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@parameter", PrixicENC);
                cmd.Parameters.AddWithValue("@Mode", "CheckPrefix");

                await conn.OpenAsync();
                int count = (int)await cmd.ExecuteScalarAsync();

                if (count > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        public async Task UpdateUserStatusAsy(string UserID, int isActive)
        {
            string connectionString = await _dcl.GetDynStrByUserIDAsync(UserID);
            string UserIDENC = await _enc.EncryptAsync(UserID);
            using (SqlConnection con = new SqlConnection(connectionString))
            {
               

                string query = "sp_Superadmin";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@parameter", isActive);
                cmd.Parameters.AddWithValue("@EmpID", UserIDENC);
                cmd.Parameters.AddWithValue("@Mode", "UpdateUserStatus");

                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

            }
        }

        public async Task<List<object>> GetUserByAccountAsync(string AccountID)
        {
            var userList = new List<object>();
            try
            {
                string ConnSTR = await _dcl.GetDynStrByAccountAsync(AccountID);
                string query = "sp_Superadmin";



                using (SqlConnection conn = new SqlConnection(ConnSTR))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@parameter", AccountID);
                        cmd.Parameters.AddWithValue("@Mode", "GetUserByAccount");

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                userList.Add(new
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    name = await _enc.DecryptAsync(reader.GetString(reader.GetOrdinal("Name"))),
                                    usertype = reader.GetString(reader.GetOrdinal("usertype")),
                                    isactive = reader.GetInt32(reader.GetOrdinal("isactive"))
                                });
                            }
                        }
                    }
                }

            }
            catch (Exception ex) { }

            return userList;
        }

        public async Task DeactivateUserByAccountAsync(UserDeactivationRequest userList)
        {
            if (userList == null || userList.Users == null || userList.Users.Count == 0)
            {
                return;
            }
            var accountId = userList.Users[0].AccountId;
            string Connstr = await _dcl.GetDynStrByAccountAsync(accountId);
            using (var con = new SqlConnection(Connstr))
            {
                await con.OpenAsync();
                foreach (var user in userList.Users)
                {

                    var UID = await _enc.EncryptAsync(user.UserID); 
                    string query = "sp_Superadmin";


                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Mode", "DeactivateUserByAccoun");
                        cmd.Parameters.AddWithValue("@parameter", user.IsActive);
                        cmd.Parameters.AddWithValue("@EMPID", UID);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task<DataTable> GetAccountDetailsAsync()
        {
            string Conn = await _enc.DecryptAsync(_con);
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(Conn))
            {
                string query = "sp_Superadmin";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", "GetAccountDetails");
                    await con.OpenAsync();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {

                        await Task.Run(() => da.Fill(dt)); 

                    }
                }
            }

            return await DecryptDataTable(dt);
        }
        private async Task<DataTable> DecryptDataTable(DataTable dt)
        {

            
            foreach (DataRow row in dt.Rows)
            {
                

                row["AccountName"] = await _enc.DecryptAsync(row["AccountName"].ToString());
                row["Authantication_Type"] = await _enc.DecryptAsync(row["Authantication_Type"].ToString());
            }


            return dt;
        }

        public async Task UpdateAccountStatusAsy(string accountId, int isActive)
        {
            string connectionString = await _enc.DecryptAsync(_con);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "sp_Superadmin";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EMPID", accountId);
                cmd.Parameters.AddWithValue("@parameter", isActive);
                cmd.Parameters.AddWithValue("@Mode", "UpdateAccountStatusAsy");
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

            }
        }


        public async Task<List<object>> GetUserByAccountIDAsync(string accountId)
        {
            string Con = await _dcl.GetDynStrByAccountAsync(accountId);

            using (SqlConnection conn = new SqlConnection(Con))
            {
                await conn.OpenAsync();

             

                string query = "sp_Superadmin";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EMPID", accountId);
              
                cmd.Parameters.AddWithValue("@Mode", "GetUserByAccountIDAsync");


                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                await Task.Run(() => da.Fill(dt));

                DataTable dtt = await DecryptDataEmployeeAsync(dt);

                var users = new List<object>();

                foreach (DataRow row in dtt.Rows)
                {
                    users.Add(new
                    {
                        UserID = row["EMPID"],
                        UserName = row["Name"],
                        AccountID = row["Account_id"],
                        usertype = row["usertype"],
                        isactive = row["isactive"]
                    });
                }

                return users;
            }
        }


        public async Task<DataTable> DecryptDataEmployeeAsync(DataTable dt)
        {
            DL_Encrpt encrypter = new DL_Encrpt();

            foreach (DataRow row in dt.Rows)
            {
                if (row["EMPID"] != DBNull.Value)
                {
                    row["EMPID"] = await _enc.DecryptAsync(row["EMPID"].ToString());
                }

                if (row["Name"] != DBNull.Value)
                {

                    row["Name"] = await _enc.DecryptAsync(row["Name"].ToString());
                }
            }

            return dt;
        }


        public async Task<string> GetAccountPrifixAsync(string Account, string AccountName)
        {
            string accountId = Account;
            string accountPrefix = string.Empty;
            try
            {
                string Conn = await _enc.DecryptAsync(_con);
                using (SqlConnection cc = new SqlConnection(Conn))
                {
                    await cc.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Get_AccountDetails", cc))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Mode", "GetAccountPrifix");
                        cmd.Parameters.AddWithValue("@AccountID", accountId);

                        DataTable dt = new DataTable();
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt));

                        if (dt.Rows.Count > 0)
                        {
                            accountPrefix = await _enc.DecryptAsync(dt.Rows[0]["AccountPrefix"].ToString());
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (SqlException ex)
            {

            }
            return accountPrefix;
        }

        public async Task CreateAdminAsync(string userID, string Password, string AccountID)
        {
            string ConnID = await _dcl.GetDynStrByUserIDAsync(userID);

            try
            {
                using (SqlConnection cc = new SqlConnection(ConnID))
                {
                    await cc.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("InsertAdmin", cc))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserName", await _enc.EncryptAsync(userID));
                        if (UserInfo.UserType == "SuperAdmin")
                        {
                            cmd.Parameters.AddWithValue("@UserType", "Admin");
                        }
                        if (UserInfo.UserType == "Normal")
                        {
                            cmd.Parameters.AddWithValue("@UserType", "QE");
                        }
                        cmd.Parameters.AddWithValue("@Password", await _enc.EncryptAsync(Password));
                        cmd.Parameters.AddWithValue("@AccountID", AccountID);
                        int result = await cmd.ExecuteNonQueryAsync();

                    }
                }
            }
            catch (SqlException ex)
            {

            }
        }



    }
}
