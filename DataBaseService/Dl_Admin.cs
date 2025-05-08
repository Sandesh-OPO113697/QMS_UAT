using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.Util;
using OfficeOpenXml;
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


        public async Task UpdateSubProcesssBT(string SubProcessID, string SubProcess, string ProgramID22, int? Number_Of_Pause, IFormFile file)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    string query = "EditFormvalue";
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "UpdateSubProcess");
                        cmd.Parameters.AddWithValue("@SubprocessID", SubProcessID);
                        cmd.Parameters.AddWithValue("@SubprocessName", SubProcess);
                        cmd.Parameters.AddWithValue("@Pausecontt", Number_Of_Pause);

                        object result = await cmd.ExecuteScalarAsync();

                        int ProcessIDTest = result != null ? Convert.ToInt32(result) : 0;
                       await UpdateAgent(SubProcessID, ProcessIDTest.ToString() , file);
                    }
                }
            }
            catch(Exception ex)
            {

            }
            
        }

        public async Task UpdateAgent(string SubProgramID, string ProgramID, IFormFile file)
        {
            int successCount = 0, duplicateCount = 0, invalidCount = 0;
            string extension = Path.GetExtension(file.FileName);
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;
                if (extension == ".xlsx")  // Handle modern Excel files
                {
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {

                            string EmpName = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                            string EmpCode = worksheet.Cells[row, 2].Value?.ToString()?.Trim()?.ToUpper();
                            string TL_Name = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                            string TL_Code = worksheet.Cells[row, 4].Value?.ToString()?.Trim();

                            string QA_Name = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                            string Batch_ID = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                            string EmailID = worksheet.Cells[row, 7].Value?.ToString()?.Trim();
                            string Password = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
                            string Phone = worksheet.Cells[row, 9].Value?.ToString()?.Trim();
                            await InsertAgenttList(EmpName, EmpCode, TL_Name, TL_Code, ProgramID, SubProgramID.ToString(), QA_Name, Batch_ID, EmailID, Password, Phone);
                            successCount++;
                        }
                    }

                }
                else if (extension == ".xls")
                {
                    HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                    ISheet sheet = hssfwb.GetSheetAt(0);
                    int rowCount = sheet.PhysicalNumberOfRows;

                    for (int row = 1; row < rowCount; row++)
                    {
                        IRow currentRow = sheet.GetRow(row);

                        string EmpName = currentRow.GetCell(0)?.ToString()?.Trim();
                        string EmpCode = currentRow.GetCell(1)?.ToString()?.Trim()?.ToUpper();
                        string TL_Name = currentRow.GetCell(2)?.ToString()?.Trim();
                        string TL_Code = currentRow.GetCell(3)?.ToString()?.Trim();

                        string QA_Name = currentRow.GetCell(4)?.ToString()?.Trim();
                        string Batch_ID = currentRow.GetCell(5)?.ToString()?.Trim();
                        string EmailID = currentRow.GetCell(6)?.ToString()?.Trim();
                        string Password = currentRow.GetCell(7)?.ToString()?.Trim();
                        string Phone = currentRow.GetCell(8)?.ToString()?.Trim();
                        await InsertAgenttList(EmpName, EmpCode, TL_Name, TL_Code, ProgramID, SubProgramID.ToString(), QA_Name, Batch_ID, EmailID, Password, Phone);
                        successCount++;
                    }
                }
                else
                {

                }

            }
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
                    cmd.CommandTimeout = 0;
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
                    cmd.CommandTimeout = 0;
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
                    cmd.CommandTimeout = 0;
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
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@Mode", "Update_SubProcessName");
                    cmd.Parameters.AddWithValue("@status", ProcessName);
                    cmd.Parameters.AddWithValue("@id", id);
                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

        }
        public async Task InsertSubProcessDetailsAsync(string Location_ID, string ProgramID, string SubProcess, int Number_Of_Pause, IFormFile file, string TypeProcess, IFormFile files)
        {
            int SubProgramID = 0;
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("CreateSubProcess", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@Location", Location_ID);
                    cmd.Parameters.AddWithValue("@Procesname", ProgramID);
                    cmd.Parameters.AddWithValue("@Subprogram", SubProcess.ToUpper());
                    cmd.Parameters.AddWithValue("@Number_Of_Pause", Number_Of_Pause);
                    cmd.Parameters.AddWithValue("@TypeProcess", TypeProcess);
                    cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName);

                    SqlParameter outpouparameter = new SqlParameter("@InserTedID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outpouparameter);
                    await cmd.ExecuteNonQueryAsync();
                    SubProgramID = (outpouparameter.Value != DBNull.Value) ? Convert.ToInt32(outpouparameter.Value) : 0;
                }
            }

            if (SubProgramID != 0)
            {

                int successCount = 0, duplicateCount = 0, invalidCount = 0;
                string extension = Path.GetExtension(file.FileName);
                string Getextension = Path.GetExtension(files.FileName);

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    if (extension == ".xlsx")  // Handle modern Excel files
                    {
                        using (var package = new ExcelPackage(stream))
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                            int rowCount = worksheet.Dimension.Rows;

                            for (int row = 2; row <= rowCount; row++)
                            {

                                string EmpName = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                string EmpCode = worksheet.Cells[row, 2].Value?.ToString()?.Trim()?.ToUpper();
                                string TL_Name = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                                string TL_Code = worksheet.Cells[row, 4].Value?.ToString()?.Trim();

                                string QA_Name = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                                string Batch_ID = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                                string EmailID = worksheet.Cells[row, 7].Value?.ToString()?.Trim();
                                string Password = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
                                string Phone = worksheet.Cells[row, 9].Value?.ToString()?.Trim();
                                await InsertAgenttList(EmpName, EmpCode, TL_Name, TL_Code, ProgramID, SubProgramID.ToString(), QA_Name, Batch_ID, EmailID, Password, Phone);
                                successCount++;
                            }
                        }

                    }
                    else if (extension == ".xls")
                    {
                        HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                        ISheet sheet = hssfwb.GetSheetAt(0);
                        int rowCount = sheet.PhysicalNumberOfRows;

                        for (int row = 1; row < rowCount; row++)
                        {
                            IRow currentRow = sheet.GetRow(row);

                            string EmpName = currentRow.GetCell(0)?.ToString()?.Trim();
                            string EmpCode = currentRow.GetCell(1)?.ToString()?.Trim()?.ToUpper();
                            string TL_Name = currentRow.GetCell(2)?.ToString()?.Trim();
                            string TL_Code = currentRow.GetCell(3)?.ToString()?.Trim();

                            string QA_Name = currentRow.GetCell(4)?.ToString()?.Trim();
                            string Batch_ID = currentRow.GetCell(5)?.ToString()?.Trim();
                            string EmailID = currentRow.GetCell(6)?.ToString()?.Trim();
                            string Password = currentRow.GetCell(7)?.ToString()?.Trim();
                            string Phone = currentRow.GetCell(8)?.ToString()?.Trim();
                            await InsertAgenttList(EmpName, EmpCode, TL_Name, TL_Code, ProgramID, SubProgramID.ToString(), QA_Name, Batch_ID, EmailID, Password, Phone);
                            successCount++;
                        }
                    }
                    else
                    {

                    }


                    using (var streams = new MemoryStream())
                    {
                        await files.CopyToAsync(streams);
                        streams.Position = 0;
                        if (Getextension == ".xlsx")  // Handle modern Excel files
                        {
                            using (var packages = new ExcelPackage(streams))
                            {
                                ExcelWorksheet worksheet = packages.Workbook.Worksheets[0];
                                int rowCount = worksheet.Dimension.Rows;

                                for (int row = 2; row <= rowCount; row++)
                                {

                                    string MATRIX = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                    string TARGET = worksheet.Cells[row, 2].Value?.ToString()?.Trim()?.ToUpper();

                                    await InsertMatrixList(ProgramID, SubProgramID.ToString(), MATRIX, TARGET);
                                    successCount++;
                                }
                            }

                        }
                        else if (Getextension == ".xls")
                        {
                            HSSFWorkbook hssfwb = new HSSFWorkbook(streams);
                            ISheet sheet = hssfwb.GetSheetAt(0);
                            int rowCount = sheet.PhysicalNumberOfRows;

                            for (int row = 1; row < rowCount; row++)
                            {
                                IRow currentRow = sheet.GetRow(row);

                                string MATRIX = currentRow.GetCell(0)?.ToString()?.Trim();
                                string TARGET = currentRow.GetCell(1)?.ToString()?.Trim()?.ToUpper();

                                await InsertMatrixList(ProgramID, SubProgramID.ToString(), MATRIX, TARGET);
                                successCount++;
                            }
                        }
                        else
                        {

                        }

                    }

                }


            }
        }
        public async Task InsertMatrixList(string ProgramID, string SubProgramID, string MATRIX, string TARGET)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("InsertmatrixMaster", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@ProgramID", ProgramID);
                        cmd.Parameters.AddWithValue("@SubProgramID", SubProgramID);
                        cmd.Parameters.AddWithValue("@MATRIX", MATRIX);
                        cmd.Parameters.AddWithValue("@TARGET", TARGET);
                        cmd.Parameters.AddWithValue("@CreateBy", UserInfo.UserName);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task InsertAgenttList(string EmpName, string EmpCode, string TL_Name, string TL_Code, string processID, string SubProcessID, string QA_Name, string Batch_ID, string EmailID, string Password, string Phone)
        {
            string UserName = UserInfo.UserName.Substring(0, 3);

            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("usp_InsertAgentList", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@EmpName", UserName + "_" + EmpName);
                    cmd.Parameters.AddWithValue("@EmpCode", UserName + "_" + EmpCode);
                    cmd.Parameters.AddWithValue("@TL_Name", TL_Name);
                    cmd.Parameters.AddWithValue("@TL_Code", UserName + "_" + TL_Code);
                    cmd.Parameters.AddWithValue("@ProcessID", processID);
                    cmd.Parameters.AddWithValue("@SubProcessID", SubProcessID);
                    cmd.Parameters.AddWithValue("@QA_Name", QA_Name);
                    cmd.Parameters.AddWithValue("@Batch_ID", Batch_ID);
                    cmd.Parameters.AddWithValue("@EmailID", EmailID);
                    cmd.Parameters.AddWithValue("@Password", Password);
                    cmd.Parameters.AddWithValue("@Phone", Phone);
                    await cmd.ExecuteNonQueryAsync();
                }


                using (SqlCommand cmd = new SqlCommand("CreateUser", conn))
                {



                    string UserNameENC = await _enc.EncryptAsync(UserName + "_" + EmpCode);
                    string NameENC = await _enc.EncryptAsync(UserName + "_" + EmpName);
                    string PassENC = await _enc.EncryptAsync(Password);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Location", UserInfo.LocationID);
                    cmd.Parameters.AddWithValue("@UserName", UserNameENC);
                    cmd.Parameters.AddWithValue("@Program", processID);
                    cmd.Parameters.AddWithValue("@Password", PassENC);
                    cmd.Parameters.AddWithValue("@SubProcesname", SubProcessID);
                    cmd.Parameters.AddWithValue("@Role", 11);
                    cmd.Parameters.AddWithValue("@AccountID", UserInfo.AccountID);
                    cmd.Parameters.AddWithValue("@UserNamedrp", UserName + "_" + EmpCode);
                    cmd.Parameters.AddWithValue("@Name", NameENC);
                    cmd.Parameters.AddWithValue("@Phone", Phone);
                    cmd.Parameters.AddWithValue("@Procesname", processID);
                    cmd.Parameters.AddWithValue("@CreateBy", UserInfo.UserName);
                    cmd.Parameters.AddWithValue("@email", EmailID);
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
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@Location", Location_ID);
                    cmd.Parameters.AddWithValue("@Procesname", Process.ToUpper());
                    cmd.Parameters.AddWithValue("@dataRetaintion", DataRetention);
                    cmd.Parameters.AddWithValue("@CreateBy", UserInfo.UserName);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }


        public async Task<DataTable> GetProcessListByLocationAccountAdmin(string Location)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("sp_admin", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);
                        cmd.Parameters.AddWithValue("@Mode", "GetProcessByAccountAdmin");
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt));
                    }
                }

            }
            catch (Exception ex)
            {

            }


            return dt;
        }
        public async Task<DataTable> GetProcessListByLocation(string Location)
        {
            string userIdsName = string.Empty;

            List<string> users = new List<string>();
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("sp_admin", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@status", UserInfo.UserName);

                    cmd.Parameters.AddWithValue("@Mode", "GetUserIdList");
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(await _enc.DecryptAsync(reader.GetString(0)));
                        }
                    }
                }
            }
            users.Add(UserInfo.UserName);
            if (users.Count > 0)
            {
                userIdsName = string.Join(",", users.Select(user => $"{user}"));
            }

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("sp_admin", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@status", userIdsName);
                        cmd.Parameters.AddWithValue("@Location_ID", Location);
                        cmd.Parameters.AddWithValue("@Mode", "Get_locationWise_list");
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt));
                    }
                }

            }
            catch (Exception ex)
            {

            }


            return dt;
        }

        public async Task<DataTable> GetLastTransactionListAsync()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("MonitoringDetails", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@Operations", "GetLastTranaction");
                   

                    SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                    await Task.Run(() => adpt.Fill(dt));
                }
            }
            return dt;
        }

        public async Task<DataTable> GetProcessListAsync()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("sp_admin", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@status", UserInfo.UserName);
                    if (UserInfo.UserType == "Admin")
                    {
                        cmd.Parameters.AddWithValue("@Mode", "Get_Process_list");
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Location_ID", UserInfo.LocationID);
                        cmd.Parameters.AddWithValue("@Mode", "Get_locationWise_list");
                    }

                    SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                    await Task.Run(() => adpt.Fill(dt));
                }
            }
            return dt;
        }

        public async Task<string> GetUserNameByID(string id)
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

                    UserName = result?.ToString() ?? string.Empty;
                }
            }
            string Dnc = await _enc.DecryptAsync(UserName);
            return Dnc;
        }


        public async Task<string> GetRoleNameByID(string id)
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
                    command.Parameters.AddWithValue("@Mode", "Get_RoleNameByUser");
                    var result = await command.ExecuteScalarAsync();

                    UserName = result?.ToString() ?? string.Empty;
                }
            }

            return UserName;
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
                        command.CommandTimeout = 0;
                        command.Parameters.AddWithValue("@status", RoleID);
                        command.Parameters.AddWithValue("@Mode", "GetFeatureOnRole");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string displayText = reader["SubFeatureName"].ToString();
                                string value = reader["SubFeature_ID"].ToString();

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

        public async Task AssignFeature(string User, string UserName, List<string> Feature, List<string> SubFeature)
        {
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                await con.OpenAsync();
                string FeatureNameQuery = "sp_admin";
                using (SqlCommand cmd = new SqlCommand(FeatureNameQuery, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@Mode", "RemoveFeaturesByRole");
                    cmd.Parameters.AddWithValue("@ID", User);
                    await cmd.ExecuteNonQueryAsync();

                }
                foreach (string selectedSubFeature in SubFeature)
                {
                    string[] iddd = selectedSubFeature.Split('-');
                    string SubFeatureID = iddd[0];

                    string FeatureID = string.Empty;
                    string FeatureName = string.Empty;
                    using (SqlCommand cmd = new SqlCommand(FeatureNameQuery, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Mode", "GetFeatureID");
                        cmd.Parameters.AddWithValue("@ID", SubFeatureID);
                        var result = await cmd.ExecuteScalarAsync();
                        FeatureID = result?.ToString();
                    }
                    using (SqlCommand cmd = new SqlCommand(FeatureNameQuery, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Mode", "AssignFeature");
                        cmd.Parameters.AddWithValue("@ID", FeatureID);
                        var result = await cmd.ExecuteScalarAsync();
                        FeatureName = result?.ToString();
                    }

                    string checkExistenceQuery = "sp_admin";
                    int existingCount = 0;
                    using (SqlCommand cmd = new SqlCommand(checkExistenceQuery, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@status", User);
                        cmd.Parameters.AddWithValue("@id", FeatureID);
                        cmd.Parameters.AddWithValue("@SubFeatureIDvalue", SubFeatureID);
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
                            cmd.Parameters.AddWithValue("@Feature_id", FeatureID);
                            cmd.Parameters.AddWithValue("@FeatureName", FeatureName);
                            cmd.Parameters.AddWithValue("@Role_id", User);
                            cmd.Parameters.AddWithValue("@RoleName", UserName);
                            cmd.Parameters.AddWithValue("@SubFetureid", SubFeatureID);
                            cmd.Parameters.AddWithValue("@CreateDate", DateTime.Now);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }
        public async Task UpdateFeatureModule(string checkboxValue, string message)
        {

            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                string query = "sp_admin";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", "UpdateFeatureModule");
                    cmd.Parameters.AddWithValue("@id", checkboxValue);
                    cmd.Parameters.AddWithValue("@status", message);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

        }
        public async Task<List<SelectListItem>> GetFeatureModule(string RoleID)
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
                        command.Parameters.AddWithValue("@id", RoleID);
                        command.Parameters.AddWithValue("@Mode", "GetFeatureModule");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string displayText = reader["Module_Name"].ToString();
                                string value = reader["Module_id"].ToString();
                                bool isActive = Convert.ToInt32(reader["Active"]) == 1;

                                processes.Add(new SelectListItem
                                {
                                    Text = displayText,
                                    Value = value,
                                    Selected = isActive
                                }); ;
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

        public async Task<List<FeatureWithSubFeatures>> GetFeature()
        {
            var processes = new List<FeatureWithSubFeatures>();
            string query = "sp_admin";
            string mode = "GetFeatureAndSubFeture";
            DataTable dt = await GetDataAsyncStoredProcedure(query, mode);

            FeatureWithSubFeatures currentFeature = null;

            foreach (DataRow row in dt.Rows)
            {
                var FetureValue = row["fetureID"].ToString();
                var FetureName = row["FeatureName"].ToString();
                var SubFetureValue = row["SubFeatureId"].ToString();
                var SubFetureName = row["SubFeaturename"].ToString();

                if (currentFeature == null || currentFeature.FeatureValue != FetureValue)
                {
                    if (currentFeature != null)
                        processes.Add(currentFeature);

                    currentFeature = new FeatureWithSubFeatures
                    {
                        FeatureValue = FetureValue,
                        FeatureName = FetureName,
                        SubFeatures = new List<SubFeature>()
                    };
                }
                currentFeature.SubFeatures.Add(new SubFeature
                {
                    SubFeatureValue = SubFetureValue,
                    SubFeatureName = SubFetureName
                });
            }

            if (currentFeature != null)
                processes.Add(currentFeature);

            return processes;
        }


        private async Task<DataTable> GetSUBFeatureNameByFeture(string query, int usernameParam, string mode)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {


                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", mode);
                    cmd.Parameters.AddWithValue("@id", usernameParam);

                    await con.OpenAsync();


                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        await Task.Run(() => da.Fill(dt));
                    }
                }
            }
            return dt;
        }
        public async Task<List<SelectListItem>> GetSubFeatureByDeture(int FetureID)
        {
            var processes = new List<SelectListItem>();
            string query = "sp_admin";
            string mode = "SubFeture";
            DataTable dt = await GetSUBFeatureNameByFeture(query, FetureID, mode);

            foreach (DataRow row in dt.Rows)
            {
                string displayText = $"{row["SubFeaturename"]}";
                string value = $"{row["SubFeatureId"]}";


                processes.Add(new SelectListItem
                {
                    Text = displayText,
                    Value = value,

                });
            }
            return processes;

        }
        public async Task RemoveRoleAccess(string User, string UserName)
        {
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                string processNameQuery = "sp_admin";
                using (SqlCommand cmd = new SqlCommand(processNameQuery, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", User);
                    cmd.Parameters.AddWithValue("@UserName", UserName);
                    cmd.Parameters.AddWithValue("@Mode", "RemoveRoleAcess");

                    await con.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();

                }
            }
        }

        public async Task AssignRole(string User, string UserName, List<string> Selectedroled)
        {
            await RemoveRoleAccess(User, UserName);
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


        public async Task<List<SelectListItem>> GetFetureByRoleAsync(string username)
        {
            string query = "sp_admin";
            string mode = "GetFeatureByRoleBasis";
            var processes = new List<SelectListItem>();
            DataTable dt = await GetFeatureNameByRole(query, username, mode);

            foreach (DataRow row in dt.Rows)
            {
                string displayText = $"{row["RoleName"]}";
                string value = $"{row["Role_id"]}";


                processes.Add(new SelectListItem
                {
                    Text = displayText,
                    Value = value,

                });
            }
            return processes;
        }
        public async Task<List<SelectListItem>> GetRoleAndSubAsync(string username)
        {
            string query = "sp_admin";
          
            var processes = new List<SelectListItem>();
            DataTable dt = new DataTable();
            if (UserInfo.UserType=="Admin")
            {
                string mode = "GetRoleAndSub";
                dt = await GetDataProcessSUBAsyncStoredProcedure(query, username, mode);

            }
            else 
            {
                string mode = "GetRoleHiraricy";
                dt = await GetDataProcessSUBAsyncStoredProcedure(query, username, mode);

            }
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

        public async Task<List<SelectListItem>> GetRoleOnBasicName(string username)
        {
            string query = "sp_admin";
            string mode = "Get_RolesByUserName";
            var processes = new List<SelectListItem>();
            DataTable dt = await GetDataProcessSUBAsyncStoredProcedure(query, username, mode);

            foreach (DataRow row in dt.Rows)
            {
                string displayText = $"{row["Role_Name"]}";
                string value = $"{row["Role_id"]}";


                processes.Add(new SelectListItem
                {
                    Text = displayText,
                    Value = value,

                });
            }
            return processes;
        }

        public async Task<List<SelectListItem>> GetUsersAsync()
        {
            string query = "sp_admin";
           
            var users = new List<SelectListItem>();
            DataTable dt = new DataTable();
            if (UserInfo.UserType == "Admin")
            {
                string mode = "GET_User";
                dt = await DecryptDataTableAsyncNamwe(await GetDataAsyncStoredProcedure(query, mode));
            }
            else if (UserInfo.UserType == "SiteAdmin")
            {
                string mode = "GET_UserByLocation";
                dt = await DecryptDataTableAsyncNamwe(await GetDataAsyncWithLocation(query, mode, UserInfo.LocationID));
            }

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

        public async Task RemoveProcessAccess(string User, string UserName)
        {
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                string processNameQuery = "sp_admin";
                using (SqlCommand cmd = new SqlCommand(processNameQuery, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", User);
                    cmd.Parameters.AddWithValue("@UserName", UserName);
                    cmd.Parameters.AddWithValue("@Mode", "RemoveProcessAcess");

                    await con.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();

                }
            }
        }
        public async Task AssignProcess(string User, string UserName, List<string> SelectedProcesses)
        {
            await RemoveProcessAccess(User, UserName);
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
                            cmd.Parameters.AddWithValue("@id", User);
                            cmd.Parameters.AddWithValue("@Mode", "AssignProcessCount");

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


            var processes = new List<SelectListItem>();
            DataTable dt = new DataTable();
            if (UserInfo.UserType == "Admin")
            {
                string mode = "Get_Processes";
                dt = await GetDataProcessSUBAsyncStoredProcedure(query, username, mode);
            }
            else if (UserInfo.UserType == "SiteAdmin")
            {
                string mode = "Get_ProcessesbyLocation";
                dt = await GetDataProcessSUBAsyncBySiteadmin(query, username, mode, UserInfo.LocationID);


            }

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


        private async Task<DataTable> GetFeatureNameByRole(string query, string usernameParam, string mode)
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

        private async Task<DataTable> GetDataProcessSUBAsyncBySiteadmin(string query, string usernameParam, string mode, string Location)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {


                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", mode);
                    cmd.Parameters.AddWithValue("@status", usernameParam);
                    cmd.Parameters.AddWithValue("@Location_ID", Location);


                    await con.OpenAsync();


                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        await Task.Run(() => da.Fill(dt));
                    }
                }
            }
            return dt;
        }





        private async Task<DataTable> GetDataProcessSUBAsyncStoredProcedure(string query, string usernameParam, string mode)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {


                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Mode", SqlDbType.NVarChar).Value = mode;
                    cmd.Parameters.Add("@status", SqlDbType.NVarChar).Value = usernameParam;


                    await con.OpenAsync();


                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt); // Sync fill
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
        private async Task<DataTable> GetDataAsyncWithLocation(string query, string mode, string Location)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mode", mode);
                    cmd.Parameters.AddWithValue("@Location_ID", Location);
                    await con.OpenAsync();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        await Task.Run(() => da.Fill(dt));
                    }
                }
            }
            return dt;
        }

        private async Task<DataTable> GetDataAsyncStoredProcedure(string query, string mode)
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


        public async Task<int> InsertUserBulkUploadAsync(string Location_ID, string ProgramID, string SUBProgramID, IFormFile file)
        {
            int successCount = 0, duplicateCount = 0, invalidCount = 0;
            string extension = Path.GetExtension(file.FileName);


            string User = UserInfo.UserName.Substring(0, 3);


            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    if (extension == ".xlsx")  // Handle modern Excel files
                    {
                        using (var package = new ExcelPackage(stream))
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                            int rowCount = worksheet.Dimension.Rows;

                            for (int row = 2; row <= rowCount; row++)
                            {

                                string USerName    = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                string Password    = worksheet.Cells[row, 2].Value?.ToString()?.Trim()?.ToUpper();
                                string Role        = worksheet.Cells[row, 3].Value?.ToString()?.Trim()?.ToUpper();
                                string USerID      = worksheet.Cells[row, 4].Value?.ToString()?.Trim()?.ToUpper();
                                string Name        = worksheet.Cells[row, 5].Value?.ToString()?.Trim()?.ToUpper();
                                string PhoneNumber = worksheet.Cells[row, 6].Value?.ToString()?.Trim()?.ToUpper();
                                string email       = worksheet.Cells[row, 7].Value?.ToString()?.Trim()?.ToUpper();

                                string UserNameENC = await _enc.EncryptAsync(User + "_" + USerID);
                                string NameENC     = await _enc.EncryptAsync(User + "_" + Name);
                                string PassENC     = await _enc.EncryptAsync(Password);

                                if(Role != "Account Level Admin" || Role != "QA Manager" || Role != "Monitor Supervsior" || Role != "Monitor" || Role != "Account Head" || Role != "HR" || Role != "MIS" || Role != "Leadership" || Role != "Agent" || Role != "Operation Manager" || Role != "SiteAdmin" )
                                {
                                    return 0;
                                }
                                else
                                {
                                    using (SqlCommand cmd = new SqlCommand("BulkUserCreate", conn))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;


                                        cmd.Parameters.AddWithValue("@Location", Location_ID);
                                        cmd.Parameters.AddWithValue("@UserName", UserNameENC);
                                        cmd.Parameters.AddWithValue("@Program", ProgramID);
                                        cmd.Parameters.AddWithValue("@Password", PassENC);
                                        cmd.Parameters.AddWithValue("@SubProcesname", SUBProgramID);
                                        cmd.Parameters.AddWithValue("@Role", Role);
                                        cmd.Parameters.AddWithValue("@AccountID", UserInfo.AccountID);
                                        cmd.Parameters.AddWithValue("@UserNamedrp", USerID);
                                        cmd.Parameters.AddWithValue("@Name", NameENC);
                                        cmd.Parameters.AddWithValue("@Phone", PhoneNumber);
                                        cmd.Parameters.AddWithValue("@Procesname", SUBProgramID);
                                        cmd.Parameters.AddWithValue("@CreateBy", UserInfo.UserName);
                                        cmd.Parameters.AddWithValue("@email", email);
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                    successCount++;
                                    
                                }
                                    
                            }


                        }
                    }
                    else
                    {
                        try
                        {
                            HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                            ISheet sheet = hssfwb.GetSheetAt(0);
                            int rowCount = sheet.PhysicalNumberOfRows;

                            for (int row = 1; row < rowCount; row++)
                            {
                                IRow currentRow = sheet.GetRow(row);

                                string USerName = currentRow.GetCell(0)?.ToString()?.Trim();
                                string Password = currentRow.GetCell(1)?.ToString()?.Trim()?.ToUpper();
                                string Role = currentRow.GetCell(2)?.ToString()?.Trim();
                                string USerID = currentRow.GetCell(3)?.ToString()?.Trim();

                                string Name = currentRow.GetCell(4)?.ToString()?.Trim();
                                string PhoneNumber = currentRow.GetCell(5)?.ToString()?.Trim();
                                string email = currentRow.GetCell(6)?.ToString()?.Trim();

                                string UserNameENC = await _enc.EncryptAsync(User + "_" + USerID);
                                string NameENC = await _enc.EncryptAsync(User + "_" + Name);
                                string PassENC = await _enc.EncryptAsync(Password);
                                if (Role != "Account Level Admin" && Role != "QA Manager" && Role != "Monitor Supervsior" && Role != "Monitor" &&
     Role != "Account Head" && Role != "HR" && Role != "MIS" && Role != "Leadership" &&
     Role != "Agent" && Role != "Operation Manager" && Role != "SiteAdmin")
                                {
                                    return 0;
                                }
                                else
                                {
                                    using (SqlCommand cmd = new SqlCommand("BulkUserCreate", conn))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.AddWithValue("@Location", Location_ID);
                                        cmd.Parameters.AddWithValue("@UserName", UserNameENC);
                                        cmd.Parameters.AddWithValue("@Program", ProgramID);
                                        cmd.Parameters.AddWithValue("@Password", PassENC);
                                        cmd.Parameters.AddWithValue("@SubProcesname", SUBProgramID);
                                        cmd.Parameters.AddWithValue("@Role", Role);
                                        cmd.Parameters.AddWithValue("@AccountID", UserInfo.AccountID);
                                        cmd.Parameters.AddWithValue("@UserNamedrp", User + "_" + USerID);
                                        cmd.Parameters.AddWithValue("@Name", NameENC);
                                        cmd.Parameters.AddWithValue("@Phone", PhoneNumber);
                                        cmd.Parameters.AddWithValue("@CreateBy", UserInfo.UserName);
                                        cmd.Parameters.AddWithValue("@email", email);
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                }
                                successCount++;
                            }
                        }
                        catch(Exception ex)
                        {

                        }

                    }


                }

             
            }
            return 1;
        }


        public async Task InsertUserDetailsAsync(string Location_ID, string ProgramID, string SUBProgramID, string Role_ID, string UserID, string Password, string UserName, string PhoneNumber , string email)
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
                    cmd.Parameters.AddWithValue("@UserNamedrp", UserID);
                    cmd.Parameters.AddWithValue("@Name", NameENC);
                    cmd.Parameters.AddWithValue("@Phone", PhoneNumber);
                    cmd.Parameters.AddWithValue("@Procesname", SUBProgramID);
                    cmd.Parameters.AddWithValue("@CreateBy", UserInfo.UserName);
                    cmd.Parameters.AddWithValue("@email", email);
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
                                Text = reader["SubProcessName"].ToString() + "," + reader["IsActive"].ToString()
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
            string StrCon = await _enc.DecryptAsync(_con);
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
                            string encryptedPrefix = reader["AccountPrefix"].ToString();
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
            string userIdsName = string.Empty;

            List<string> users = new List<string>();
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("sp_admin", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@status", UserInfo.UserName);
                    cmd.Parameters.AddWithValue("@Mode", "GetUserIdList");
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(await _enc.DecryptAsync(reader.GetString(0)));
                        }
                    }
                }
            }
            users.Add(UserInfo.UserName);
            if (users.Count > 0)
            {
                userIdsName = string.Join(",", users.Select(user => $"{user}"));
            }
            string query = "sp_admin";
            string mode = "GetUserList";
            DataTable dt = await GetDataProcessSUBAsyncStoredProcedure(query, userIdsName, mode);
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
