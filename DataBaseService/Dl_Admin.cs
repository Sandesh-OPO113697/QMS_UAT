using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.Encription;
using QMS.Models;
using System.Data;
using System.Data.SqlClient;

namespace QMS.DataBaseService
{
    public class Dl_Admin
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dcl;
        public Dl_Admin(IConfiguration configuration, DL_Encrpt dL_Encrpt, DLConnection dL)
        {
            _con = configuration.GetConnectionString("Master_Con");
            _enc = dL_Encrpt;
            _dcl = dL;
        }
        public async Task<List<SelectListItem>> GetProcessName(int LocationID)
        {
            var Process = new List<SelectListItem>();

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("USP_Fill_Dropdown", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", "Select_Program_Master_locationWise");
                    cmd.Parameters.AddWithValue("@p_username", LocationID);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Process.Add(new SelectListItem
                            {
                                Value = reader["ID"].ToString(),
                                Text = reader["Process"].ToString()
                            });
                        }
                    }
                }
            }
            return Process;
        }

        public async Task<List<SelectListItem>> GetLocationAsync()
        {
            var locations = new List<SelectListItem>();

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("USP_Fill_Dropdown", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", "Select_Location_Master");

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            locations.Add(new SelectListItem
                            {
                                Value = reader["ID"].ToString(),
                                Text = reader["Location"].ToString()
                            });
                        }
                    }
                }
            }
            return locations;
        }

        public async Task<string> GetPrefixAsync()
        {
            string accountPrefix = string.Empty;
            string accountId = UserInfo.AccountID;
            string StrCon = await _enc.DecryptAsync( _con);
            using (SqlConnection conn = new SqlConnection(StrCon))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("Get_AccountDetails", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", "GetAccountPrifix");
                    cmd.Parameters.AddWithValue("@AccountID", accountId);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string encryptedPrefix =  reader["AccountPrefix"].ToString();
                            accountPrefix = await _enc.DecryptAsync(encryptedPrefix) + "_";
                        }
                    }
                }
            }

            return accountPrefix;
        }

        public async Task<List<SelectListItem>> GetRoleAsync()
        {
            var Role = new List<SelectListItem>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("USP_Fill_Dropdown", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", "BindRoleDetails");

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {

                            Role.Add(new SelectListItem
                            {
                                Value = reader["Roleid"].ToString(),
                                Text = reader["Role_name"].ToString()
                            });
                        }
                    }
                }
            }
            return Role;
        }

        public async Task DeactiveActiveUser(int id, bool isActive)
        {
            string status = isActive ? "1" : "0";
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                string query = "UPDATE User_Master SET isactive = @status WHERE ID = @id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@id", id);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

        }

        public async Task<DataTable> GetUserListAsync()
        {
            string query = "SELECT UM.ID,UM.username,UM.usertype  as Role,(SELECT Process FROM Eval_Process WHERE ID = UM.Program_id) AS Process,LM.Location, UM.Created_Date, UM.isactive AS Active_Status FROM User_Master UM LEFT JOIN LocationMaster LM ON UM.Location = LM.ID where UM.CreateBy='" + UserInfo.UserName + "' ;";

            DataTable dt = await GetDataAsync(query);
            if (dt.Rows.Count > 0)
            {
                DataTable decryptedData2 = await DecryptDataTableAsync(dt);
                return decryptedData2;
            }

            return dt;
        }

        private async Task<DataTable> DecryptDataTableAsync(DataTable dt)
        {
            DL_Encrpt encrypter = new DL_Encrpt();

            foreach (DataRow row in dt.Rows)
            {
                if (row["username"] != DBNull.Value)
                {
                    row["username"] = await _enc.DecryptAsync(row["username"].ToString());
                }
            }

            return dt;
        }

        private async Task<DataTable> GetDataAsync(string query)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    await con.OpenAsync();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        await Task.Run(() => da.Fill(dt));
                    }
                }
            }
            return dt;
        }

    }
}
