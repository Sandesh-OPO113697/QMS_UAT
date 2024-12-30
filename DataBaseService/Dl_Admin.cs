using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.Encription;
using QMS.Models;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;

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



        public async Task DeactiveActiveSubProcess(int id, bool isActive)
        {
            string status = isActive ? "1" : "0";
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                string query = "sp_admin";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", "Eval_SubProcess");
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@id", id);
                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task DeactiveActiveProcess(int id, bool isActive)
        {
            string status = isActive ? "1" : "0";
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                string query = "sp_admin";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", "Eval_Process");
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@id", id);
                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

        }
        public async Task UpdateProcess(int id, string ProcessName)
        {

            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                string query = "sp_admin";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", "Update_ProcessName");
                    cmd.Parameters.AddWithValue("@status", ProcessName);
                    cmd.Parameters.AddWithValue("@id", id);
                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

        }
        public async Task UpdateSubProcessByName(int id, string ProcessName)
        {

            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                string query = "sp_admin";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", "Update_SubProcessName");
                    cmd.Parameters.AddWithValue("@status", ProcessName);
                    cmd.Parameters.AddWithValue("@id", id);
                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

        }
        public async Task InsertSubProcessDetailsAsync(string Location_ID, string ProgramID, string SubProcess)
        {

            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("CreateSubProcess", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Location", Location_ID);
                    cmd.Parameters.AddWithValue("@Procesname", ProgramID);
                    cmd.Parameters.AddWithValue("@Subprogram", SubProcess);
                    cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task InsertProcessDetailsAsync(string Location_ID, string Process, string DataRetention)
        {

            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("CreateProcess", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Location", Location_ID);
                    cmd.Parameters.AddWithValue("@Procesname", Process);
                    cmd.Parameters.AddWithValue("@dataRetaintion", DataRetention);
                    cmd.Parameters.AddWithValue("@CreateBy", UserInfo.UserName);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<DataTable> GetProcessListByLocation(string Location)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("sp_admin", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@status",UserInfo.UserName);
                    cmd.Parameters.AddWithValue("@id", Location);
                    cmd.Parameters.AddWithValue("@Mode", "Update_SubProcessName");
                    SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                    await Task.Run(() => adpt.Fill(dt));
                }
            }

            return dt;
        }
        public async Task<DataTable> GetProcessListAsync()
        {
            string query = "SELECT UM.ID,LM.Location as LocationName,UM.Process as ProcessName,UM.Active_Status, UM.Created_Date FROM Eval_Process UM LEFT JOIN LocationMaster LM ON UM.Location_ID = LM.ID where UM.CreateBy='" + UserInfo.UserName + "' ;";

            

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("sp_admin", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@status", UserInfo.UserName);
                    cmd.Parameters.AddWithValue("@Mode", "Get_Process_list");
                    SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                    await Task.Run(() => adpt.Fill(dt));
                }
            }

            return dt;


        }

        public async Task <string> GetUserNameByID(string id)
        {
            string query = "sp_admin";
            string UserName = string.Empty;
            using (var connection = new SqlConnection(UserInfo.Dnycon))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@Mode", "Get_Empid");
                    var result = await command.ExecuteScalarAsync();

                    UserName= result?.ToString() ?? string.Empty;
                }
            }
            string Dnc = await _enc.DecryptAsync(UserName);
            return Dnc;
        }
      

        public async Task<List<SelectListItem>> GetFeatureByRole(string RoleID)
        {
           



            var processes = new List<SelectListItem>();
            string storedProcedure = "sp_admin";

            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(storedProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@status", RoleID);
                        command.Parameters.AddWithValue("@Mode", "GetFeatureByRole");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string displayText = reader["FeatureName"].ToString();
                                string value = reader["Feature_id"].ToString();

                                processes.Add(new SelectListItem
                                {
                                    Text = displayText,
                                    Value = value
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

            return processes;

        }



        public async Task AssignFeature(string User, string UserName, List<string> Selectedroled)
        {
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                await con.OpenAsync();

                foreach (string selectedProcess in Selectedroled)
                {
                    string[] ids = selectedProcess.Split('-');
                    string processID = ids[0];


                    string processNameQuery = "sp_admin";
                    string processName = string.Empty;
                    using (SqlCommand cmd = new SqlCommand(processNameQuery, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Mode", "AssignFeature");
                        cmd.Parameters.AddWithValue("@ID", processID);
                        var result = await cmd.ExecuteScalarAsync();
                        processName = result?.ToString();
                    }

                    if (!string.IsNullOrEmpty(processName))
                    {
                        string checkExistenceQuery = "sp_admin";
                        int existingCount = 0;
                        using (SqlCommand cmd = new SqlCommand(checkExistenceQuery, con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@status", User);
                            cmd.Parameters.AddWithValue("@id", processID);
                            cmd.Parameters.AddWithValue("@Mode", "Feature_count");

                            var countResult = await cmd.ExecuteScalarAsync();
                            existingCount = Convert.ToInt32(countResult);
                        }

                        if (existingCount == 0)
                        {
                            string insertQuery = "InsertUserFeatureMapping";
                            using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@Role_id", processID);
                                cmd.Parameters.AddWithValue("@Role_Name", processName);
                                cmd.Parameters.AddWithValue("@User_id", User);
                                cmd.Parameters.AddWithValue("@UserName", UserName);
                                cmd.Parameters.AddWithValue("@CreateDate", DateTime.Now);

                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
            }
        }
        public async Task<List<SelectListItem>> GetFeature()
        {
            var processes = new List<SelectListItem>();
            string query = "sp_admin";
            string mode = "GetFeature";
            DataTable dt = await GetDataAsyncStoredProcedure(query,mode);

            foreach (DataRow row in dt.Rows)
            {
                string displayText = $"{row["FeatureName"]}";
                string value = $"{row["Id"]}";
                

                processes.Add(new SelectListItem
                {
                    Text = displayText,
                    Value = value,
                    
                });
            }
            return processes;
            
        }

        public async Task AssignRole(string User, string UserName, List<string> Selectedroled)
        {
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                await con.OpenAsync();

                foreach (string selectedProcess in Selectedroled)
                {
                    string[] ids = selectedProcess.Split('-');
                    string processID = ids[0];
                    

                    string processNameQuery = "sp_admin";
                    string processName = string.Empty;
                    using (SqlCommand cmd = new SqlCommand(processNameQuery, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id", processID);
                        cmd.Parameters.AddWithValue("@Mode", "AssignRole");
                        var result = await cmd.ExecuteScalarAsync();
                        processName = result?.ToString();
                    }

                    if (!string.IsNullOrEmpty(processName))
                    {
                        string checkExistenceQuery = "sp_admin";
                        int existingCount = 0;
                        using (SqlCommand cmd = new SqlCommand(checkExistenceQuery, con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Mode", "User_Role_Mapping_Count");
                            cmd.Parameters.AddWithValue("@id", User);
                            cmd.Parameters.AddWithValue("@status", processID);
                            
                            var countResult = await cmd.ExecuteScalarAsync();
                            existingCount = Convert.ToInt32(countResult);
                        }

                        if (existingCount == 0)
                        {
                            string insertQuery = "InsertUserRoleMapping";
                            using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@Role_id", processID);
                                cmd.Parameters.AddWithValue("@Role_Name", processName);
                                cmd.Parameters.AddWithValue("@User_id", User);
                                cmd.Parameters.AddWithValue("@UserName", UserName);
                                cmd.Parameters.AddWithValue("@CreateDate", DateTime.Now);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
            }
        }

        public async Task<List<SelectListItem>> GetRoleAndSubAsync(string username)
        {
            string query = "sp_admin";
            string mode = "GetRoleAndSub";
            var processes = new List<SelectListItem>();
            DataTable dt = await GetDataProcessSUBAsyncStoredProcedure(query, username,mode);

            foreach (DataRow row in dt.Rows)
            {
                string displayText = $"{row["Role_name"]}";
                string value = $"{row["Roleid"]}";
                bool isActive = Convert.ToInt32(row["isActive"]) == 1;

                processes.Add(new SelectListItem
                {
                    Text = displayText,
                    Value = value,
                    Selected = isActive
                });
            }
            return processes;
        }

        public async Task<List<SelectListItem>> GetUsersAsync()
        {
            string query = "sp_admin";
            string mode = "GET_User";
            var users = new List<SelectListItem>();
            DataTable dt = await DecryptDataTableAsyncNamwe(await GetDataAsyncStoredProcedure(query, mode));
            foreach (DataRow row in dt.Rows)
            {
                users.Add(new SelectListItem
                {
                    Text = row["Name"].ToString(),
                    Value = row["ID"].ToString()
                });
            }
            return users;
        }

        public async Task AssignProcess(string User, string UserName, List<string> SelectedProcesses)
        {
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                await con.OpenAsync();  

                foreach (string selectedProcess in SelectedProcesses)
                {
                    string[] ids = selectedProcess.Split('-');
                    string processID = ids[0];
                    string subProcessID = ids[1];

                    string processNameQuery = "sp_admin";
                    string processName = string.Empty;
                    using (SqlCommand cmd = new SqlCommand(processNameQuery, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@status", processID);
                        cmd.Parameters.AddWithValue("@Mode", "AssignProcess");
                        var result = await cmd.ExecuteScalarAsync(); 
                        processName = result?.ToString(); 
                    }

                    if (!string.IsNullOrEmpty(processName))
                    {
                        string checkExistenceQuery = "sp_admin";
                        int existingCount = 0;
                        using (SqlCommand cmd = new SqlCommand(checkExistenceQuery, con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@UserID", User);
                            cmd.Parameters.AddWithValue("@status", processID);
                            cmd.Parameters.AddWithValue("@Process", subProcessID);
                            var countResult = await cmd.ExecuteScalarAsync(); 
                            existingCount = Convert.ToInt32(countResult);
                        }

                        if (existingCount == 0)
                        {
                            string insertQuery = "InsertUserProgramMapping";
                            using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@ProcessID", processID);
                                cmd.Parameters.AddWithValue("@ProgramName", processName);
                                cmd.Parameters.AddWithValue("@SubProcessID", subProcessID);
                                cmd.Parameters.AddWithValue("@UserID", User);
                                cmd.Parameters.AddWithValue("@UserName", UserName);
                                cmd.Parameters.AddWithValue("@CreateDate", DateTime.Now);

                                await cmd.ExecuteNonQueryAsync();  
                            }
                        }
                    }
                }
            }
        }

            public async Task<List<SelectListItem>> GetProcessesAndSubAsync(string username)
            {
            string query = "sp_admin";
            string mode = "Get_Processes";

            var processes = new List<SelectListItem>();
                DataTable dt = await GetDataProcessSUBAsyncStoredProcedure(query, username, mode);

                foreach (DataRow row in dt.Rows)
                {
                    string displayText = $"{row["ProcessName"]} -- {row["SubProcessName"]}";
                    string value = $"{row["ProcessID"]}-{row["SubProcessID"]}";
                    bool isActive = Convert.ToInt32(row["isActive"]) == 1;

                    processes.Add(new SelectListItem
                    {
                        Text = displayText,
                        Value = value,
                        Selected = isActive
                    });
                }
                return processes;
            }
        

        private async Task<DataTable> DecryptDataTableAsyncNamwe(DataTable dt)
        {
            DL_Encrpt encrypter = new DL_Encrpt();
            foreach (DataRow row in dt.Rows)
            {
                if (row["Name"] != DBNull.Value)
                {
                    row["Name"] = await _enc.DecryptAsync(row["Name"].ToString());
                }
            }
            return dt;
        }

        private async Task<DataTable> GetDataProcessSUBAsync(string query, SqlParameter usernameParam)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))  
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    
                    cmd.Parameters.Add(usernameParam);

                    
                    await con.OpenAsync();

                    
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        await Task.Run(() => da.Fill(dt));
                    }
                }
            }
            return dt;
        }


        private async Task<DataTable> GetDataProcessSUBAsyncStoredProcedure(string query, string usernameParam,string mode)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {

                   
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", mode);
                    cmd.Parameters.AddWithValue("@status", usernameParam);

                    await con.OpenAsync();


                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        await Task.Run(() => da.Fill(dt));
                    }
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


        private async Task<DataTable> GetDataAsyncStoredProcedure(string query,string mode)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", mode);
                    await con.OpenAsync();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        await Task.Run(() => da.Fill(dt));
                    }
                }
            }
            return dt;
        }


        public async Task InsertUserDetailsAsync(string Location_ID, string ProgramID, string SUBProgramID, string Role_ID, string UserID, string Password, string UserName, string PhoneNumber)
        {
            string UserNameENC = await _enc.EncryptAsync(UserID);
            string NameENC = await _enc.EncryptAsync(UserName);
                string PassENC = await _enc.EncryptAsync(Password);
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync(); 

                using (SqlCommand cmd = new SqlCommand("CreateUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Location", Location_ID);
                    cmd.Parameters.AddWithValue("@UserName", UserNameENC);
                    cmd.Parameters.AddWithValue("@Program", ProgramID);
                    cmd.Parameters.AddWithValue("@Password", PassENC);
                    cmd.Parameters.AddWithValue("@SubProcesname", SUBProgramID);
                    cmd.Parameters.AddWithValue("@Role", Role_ID);
                    cmd.Parameters.AddWithValue("@AccountID", UserInfo.AccountID);
                    cmd.Parameters.AddWithValue("@UserNamedrp", UserInfo.UserName);
                    cmd.Parameters.AddWithValue("@Name", NameENC);
                    cmd.Parameters.AddWithValue("@Phone", PhoneNumber);
                    cmd.Parameters.AddWithValue("@Procesname", SUBProgramID);
                    cmd.Parameters.AddWithValue("@CreateBy", UserInfo.UserName);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
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

        public async Task<List<SelectListItem>> GetSUBProcessName(int LocationID)
        {
            var Process = new List<SelectListItem>();

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("USP_Fill_Dropdown", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", "Select_SUB_Program_Master_locationWise");
                    cmd.Parameters.AddWithValue("@p_username", LocationID);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Process.Add(new SelectListItem
                            {
                                Value = reader["id"].ToString(),
                                Text = reader["SubProcessName"].ToString() + ","+ reader["IsActive"].ToString()
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
                string query = "sp_admin";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", "DeactiveActiveUser");
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@id", id);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

        }

        public async Task<DataTable> GetUserListAsync()
        {
            string query = "sp_admin";
            string mode = "GetUserList";
            DataTable dt = await GetDataProcessSUBAsyncStoredProcedure(query, UserInfo.UserName,mode);
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


    }
}
