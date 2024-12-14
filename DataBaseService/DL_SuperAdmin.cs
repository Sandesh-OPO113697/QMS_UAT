using QMS.Encription;
using System.Data.SqlClient;
using System.Data;
using QMS.Models;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

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
                    string query = "UPDATE User_Master SET isactive = @isactive WHERE Name = @Name";

                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@isactive", user.IsActive);
                        cmd.Parameters.AddWithValue("@Name", UID);
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
                string query = "SELECT AccountID, AccountName, Authantication_Type, Isactive, Create_date FROM AccountDetails";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    await con.OpenAsync();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        await Task.Run(() => da.Fill(dt)); // Running synchronous Fill in a Task
                    }
                }
            }

            return await DecryptDataTable(dt);
        }
        private async Task<DataTable> DecryptDataTable(DataTable dt)
        {
            // Iterate over each row in the DataTable
            foreach (DataRow row in dt.Rows)
            {
                // Await the decryption of each value asynchronously
                row["AccountName"] = await _enc.DecryptAsync(row["AccountName"].ToString());
                row["Authantication_Type"] = await _enc.DecryptAsync(row["Authantication_Type"].ToString());
            }


            return dt;
        }

        public async Task UpdateAccountStatusAsy(string accountId, int isActive)
        {
            string connectionString = await _dcl.GetDynStrByAccountAsync(accountId);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "UPDATE AccountDetails SET Isactive = @IsActive WHERE AccountID = @AccountID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@AccountID", accountId);
                cmd.Parameters.AddWithValue("@IsActive", isActive);

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

                string query2 = "SELECT Name, Username, EMPID, Account_id, usertype, isactive FROM User_Master WHERE Account_id = @AccountId";
                SqlCommand cmd = new SqlCommand(query2, conn);
                cmd.Parameters.AddWithValue("@AccountId", accountId);

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
