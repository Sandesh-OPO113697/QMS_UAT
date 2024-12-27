using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.Encription;
using QMS.Models;
using System.Data;
using System.Data.SqlClient;
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
                string query = "UPDATE Eval_SubProcess SET Isactive = @status WHERE ID = @id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
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
                string query = "UPDATE Eval_Process SET Active_Status = @status WHERE ID = @id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
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
                string query = "UPDATE Eval_Process SET Process = @ProcessName WHERE ID = @id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ProcessName", ProcessName);
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
                string query = "UPDATE Eval_SubProcess SET SubProcessName = @ProcessName WHERE ID = @id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ProcessName", ProcessName);
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
            string query = "SELECT UM.ID,LM.Location as LocationName,UM.Process as ProcessName,UM.Active_Status, UM.Created_Date FROM Eval_Process UM LEFT JOIN LocationMaster LM ON UM.Location_ID = LM.ID where UM.CreateBy='" + UserInfo.UserName + "'and   UM.Location_ID="+ Location + " ;";

            DataTable dt = await GetDataAsync(query);

            return dt;
        }
        public async Task<DataTable> GetProcessListAsync()
        {
            string query = "SELECT UM.ID,LM.Location as LocationName,UM.Process as ProcessName,UM.Active_Status, UM.Created_Date FROM Eval_Process UM LEFT JOIN LocationMaster LM ON UM.Location_ID = LM.ID where UM.CreateBy='" + UserInfo.UserName + "' ;";

            DataTable dt = await GetDataAsync(query);

            return dt;
        }

        public async Task <string> GetUserNameByID(string id)
        {
            string query = "SELECT EMPID FROM User_Master WHERE id = @id";
            string UserName = string.Empty;
            using (var connection = new SqlConnection(UserInfo.Dnycon))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
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
            string query = "select Feature_id , FeatureName from User_Feature_Mapping where Role_id=" + RoleID;
            DataTable dt = await GetDataAsync(query);

            foreach (DataRow row in dt.Rows)
            {
                string displayText = $"{row["FeatureName"]}";
                string value = $"{row["Feature_id"]}";


                processes.Add(new SelectListItem
                {
                    Text = displayText,
                    Value = value,

                });
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


                    string processNameQuery = "SELECT FeatureName FROM [dbo].[Feature_Master] WHERE ID = @ProcessID";
                    string processName = string.Empty;
                    using (SqlCommand cmd = new SqlCommand(processNameQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@ProcessID", processID);
                        var result = await cmd.ExecuteScalarAsync();
                        processName = result?.ToString();
                    }

                    if (!string.IsNullOrEmpty(processName))
                    {
                        string checkExistenceQuery = "SELECT COUNT(*) FROM User_Feature_Mapping WHERE feature_id = @Role_id AND Role_id = @User_id";
                        int existingCount = 0;
                        using (SqlCommand cmd = new SqlCommand(checkExistenceQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@User_id", User);
                            cmd.Parameters.AddWithValue("@Role_id", processID);

                            var countResult = await cmd.ExecuteScalarAsync();
                            existingCount = Convert.ToInt32(countResult);
                        }

                        if (existingCount == 0)
                        {
                            string insertQuery = "INSERT INTO User_Feature_Mapping (Role_id, RoleName, feature_id, FeatureName, CreateDate)VALUES (@User_id, @UserName, @Role_id, @Role_Name, @CreateDate)";
                            using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                            {
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
            string query = "SELECT Id, FeatureName FROM [dbo].[Feature_Master] where Active = 1";
            DataTable dt = await GetDataAsync(query);

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
                    

                    string processNameQuery = "SELECT Role_name FROM [dbo].[Role_Master] WHERE Roleid = @ProcessID";
                    string processName = string.Empty;
                    using (SqlCommand cmd = new SqlCommand(processNameQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@ProcessID", processID);
                        var result = await cmd.ExecuteScalarAsync();
                        processName = result?.ToString();
                    }

                    if (!string.IsNullOrEmpty(processName))
                    {
                        string checkExistenceQuery = "SELECT COUNT(*) FROM User_Role_Mapping WHERE User_id = @UserID AND Role_id = @Role_id ";
                        int existingCount = 0;
                        using (SqlCommand cmd = new SqlCommand(checkExistenceQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@UserID", User);
                            cmd.Parameters.AddWithValue("@Role_id", processID);
                            
                            var countResult = await cmd.ExecuteScalarAsync();
                            existingCount = Convert.ToInt32(countResult);
                        }

                        if (existingCount == 0)
                        {
                            string insertQuery = "INSERT INTO User_Role_Mapping (Role_id, Role_Name, User_id, UserName, CreateDate) VALUES (@Role_id, @Role_Name, @User_id, @UserName, @CreateDate)";
                            using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                            {
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
            string query = @"
                                SELECT 
                                    RM.Roleid,
                                    RM.Role_name,
                                    CASE 
                                        WHEN URM.Role_id IS NOT NULL THEN 1 
                                        ELSE 0 
                                    END AS isActive
                                FROM 
                                    [dbo].[Role_Master] RM
                                LEFT JOIN 
                                    [dbo].[User_Role_Mapping] URM 
                                ON 
                                    RM.Roleid = URM.Role_id 
                                    AND URM.UserName = @Username
                                WHERE 
                                    RM.Active = 1;

                                ";

            var processes = new List<SelectListItem>();
            DataTable dt = await GetDataProcessSUBAsync(query, new SqlParameter("@Username", username));

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
            string query = "SELECT ID, Name FROM [dbo].[User_Master] WHERE isactive = 1";
            var users = new List<SelectListItem>();
            DataTable dt = await DecryptDataTableAsyncNamwe(await GetDataAsync(query));
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

                    string processNameQuery = "SELECT Process FROM [dbo].[Eval_Process] WHERE ID = @ProcessID";
                    string processName = string.Empty;
                    using (SqlCommand cmd = new SqlCommand(processNameQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@ProcessID", processID);
                        var result = await cmd.ExecuteScalarAsync(); 
                        processName = result?.ToString(); 
                    }

                    if (!string.IsNullOrEmpty(processName))
                    {
                        string checkExistenceQuery = "SELECT COUNT(*) FROM User_Program_Mapping WHERE Userid = @UserID AND Proram_id = @ProcessID AND Sub_ProgramId = @SubProcessID";
                        int existingCount = 0;
                        using (SqlCommand cmd = new SqlCommand(checkExistenceQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@UserID", User);
                            cmd.Parameters.AddWithValue("@ProcessID", processID);
                            cmd.Parameters.AddWithValue("@SubProcessID", subProcessID);
                            var countResult = await cmd.ExecuteScalarAsync(); 
                            existingCount = Convert.ToInt32(countResult);
                        }

                        if (existingCount == 0)
                        {
                            string insertQuery = "INSERT INTO User_Program_Mapping (Proram_id, ProgramName, Sub_ProgramId, Userid, UserName, CreateDate) VALUES (@ProcessID, @ProgramName, @SubProcessID, @UserID, @UserName, @CreateDate)";
                            using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                            {
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
                string query = @"
                                    SELECT 
                                    p.ID AS ProcessID, 
                                    sp.id AS SubProcessID, 
                                    p.Process AS ProcessName, 
                                    sp.SubProcessName,
                                    CASE 
                                        WHEN p.ID = upm.ProcessID AND sp.id = upm.SubProcessID THEN 1
                                        WHEN p.ID = upm.ProcessID THEN 1
                                        ELSE 0
                                    END AS isActive
                                FROM Eval_Process p
                                LEFT JOIN Eval_SubProcess sp 
                                    ON p.ID = sp.processid
                                LEFT JOIN (
                                    SELECT 
                                        Proram_id AS ProcessID, 
                                        Sub_ProgramId AS SubProcessID, 
                                        UserName 
                                    FROM User_Program_Mapping 
                                    WHERE UserName = @Username
                                ) upm
                                    ON p.ID = upm.ProcessID
                                    AND (sp.id = upm.SubProcessID OR sp.id IS NULL)
                                WHERE p.Active_Status = 1;
                                ";

                var processes = new List<SelectListItem>();
                DataTable dt = await GetDataProcessSUBAsync(query, new SqlParameter("@Username", username));

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


    }
}
