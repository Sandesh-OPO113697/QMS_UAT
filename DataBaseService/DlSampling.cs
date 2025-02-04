using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace QMS.DataBaseService
{
    public class DlSampling
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dcl;
        public DlSampling(IConfiguration configuration, DL_Encrpt dL_Encrpt, DLConnection dL)
        {
            _con = configuration.GetConnectionString("Master_Con");
            _enc = dL_Encrpt;
            _dcl = dL;
        }
        public async Task AssignFiltersAgainProcess(string ahtMin, string ahtMax, string disposition, string Process, string SubProcess)
        {
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("sp_AssignFilyters", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ahtMin", ahtMin);
                    cmd.Parameters.AddWithValue("@ahtMax", ahtMax);
                    cmd.Parameters.AddWithValue("@disposition", disposition);
                    cmd.Parameters.AddWithValue("@Process", Process);
                    cmd.Parameters.AddWithValue("@SubProcess", SubProcess);
                    cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

        }
        public async Task<JsonResult> InsertAllocationDetails(Dictionary<string, string> formData)
        {
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("CreateAllowcation", connection))
                    {
                        
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@AuditID", formData.ContainsKey("AuditID") ? formData["AuditID"] : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@LocationID", formData.ContainsKey("LocationID") ? formData["LocationID"] : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@RoleID", formData.ContainsKey("RoleID") ? formData["RoleID"] : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ProgramID", formData.ContainsKey("ProgramID") ? formData["ProgramID"] : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@SUBProgramID", formData.ContainsKey("SUBProgramID") ? formData["SUBProgramID"] : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@SamplingSize", formData.ContainsKey("SamplingSize") ? formData["SamplingSize"] : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Agent", formData.ContainsKey("Agent") ? formData["Agent"] : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@qa", formData.ContainsKey("qa") ? formData["qa"] : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@tl", formData.ContainsKey("tl") ? formData["tl"] : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@target", formData.ContainsKey("target") ? formData["target"] : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@NoOfWorking", formData.ContainsKey("NoOfWorking") ? formData["NoOfWorking"] : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Active_Head_count", formData.ContainsKey("Audit_Active_Head_count_ALL") ? formData["Audit_Active_Head_count_ALL"] : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Audit_sampling_Month", formData.ContainsKey("Audit_Audit_sampling_Month_ALL") ? formData["Audit_Audit_sampling_Month_ALL"] : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@of_qa", formData.ContainsKey("Audit_of-qa_ALL") ? formData["Audit_of-qa_ALL"] : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Per_QA_Monthly_target", formData.ContainsKey("Audit_Per_QA_Monthly_target_ALL") ? formData["Audit_Per_QA_Monthly_target_ALL"] : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Per_QA_Daily_Target", formData.ContainsKey("Audit_Per_QA_Daily_Traget_ALL") ? formData["Audit_Per_QA_Daily_Traget_ALL"] : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Per_QA_Target_Manual", formData.ContainsKey("Audit_Target_Manual_ALL") ? formData["Audit_Target_Manual_ALL"] : (object)DBNull.Value);
                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true, message = "Data saved successfully!" });
                        }
                        else
                        {
                            return new JsonResult(new { success = false, message = "Data insertion failed" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public async Task<String> GetSamplingCountByProcessandsub(string Process , string? SubProcess)
        {
            string SamplingCount = string.Empty;
            string StoreProcedure = "ALLOCATION";
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@MODE", "GetSampling");
                        command.Parameters.AddWithValue("@Process", Process);
                        command.Parameters.AddWithValue("@SubProcess", SubProcess);
                        using (var reder = await command.ExecuteReaderAsync())
                        {
                            while (await reder.ReadAsync())
                            {
                                 SamplingCount = reder["SampleSize"].ToString();
                               
                            }
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {

            }
            return SamplingCount;
        }
        public async Task<String> GetQACountByProcessandsub(string Process, string? SubProcess)
        {
            string SamplingCount = string.Empty;
            string StoreProcedure = "ALLOCATION";
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@MODE", "GetQACOUNT");
                        command.Parameters.AddWithValue("@Process", Process);
                        command.Parameters.AddWithValue("@SubProcess", SubProcess);
                        using (var reder = await command.ExecuteReaderAsync())
                        {
                            while (await reder.ReadAsync())
                            {
                                SamplingCount = reder["QA_Count"].ToString();

                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return SamplingCount;
        }

        public async Task<String> GetTLCountByProcessandsub(string Process, string? SubProcess)
        {
            string TLCount = string.Empty;
            string StoreProcedure = "ALLOCATION";
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@MODE", "GetTlCOUNT");
                        command.Parameters.AddWithValue("@Process", Process);
                        command.Parameters.AddWithValue("@SubProcess", SubProcess);
                        using (var reder = await command.ExecuteReaderAsync())
                        {
                            while (await reder.ReadAsync())
                            {
                                TLCount = reder["TL_Count"].ToString();

                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return TLCount;
        }

        public async Task<List<SelectListItem>> GetAuditType()
        {
            var Adudit = new List<SelectListItem>();
            string StoreProcedure = "ALLOCATION";
            try
            {
                using (var connection=  new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command =  new SqlCommand(StoreProcedure , connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@MODE" , "AUDIT_TYPE");
                        using (var reder = await command.ExecuteReaderAsync())
                        {
                            while(await reder.ReadAsync())
                            {
                                string TText = reder["Audit_Type"].ToString();
                                string TValue = reder["ID"].ToString();
                                Adudit.Add(new SelectListItem
                                {
                                    Text= TText,
                                    Value= TValue
                                });

                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }
            return Adudit;

        }

        public async Task<List<SelectListItem>> GetRoleList(string roleName)
        {
            var Adudit = new List<SelectListItem>();
            string StoreProcedure = "ALLOCATION";
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@MODE", "GetRoleList");
                        command.Parameters.AddWithValue("@Role", roleName);
                        using (var reder = await command.ExecuteReaderAsync())
                        {
                            while (await reder.ReadAsync())
                            {
                                string TText = reder["RoleName"].ToString();
                                string TValue = reder["RoleID"].ToString();
                                Adudit.Add(new SelectListItem
                                {
                                    Text = TText,
                                    Value = TValue
                                });

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Adudit;

        }
        public async Task AddSamplingCount(int SamplingSize , string FetureID , string SubFetureid , string RoleName , string process , string subprocess)
        {
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("sp_Sampling", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SamplingSize", SamplingSize);
                    cmd.Parameters.AddWithValue("@RoleName", RoleName);
                    cmd.Parameters.AddWithValue("@Location", UserInfo.LocationID);
                    cmd.Parameters.AddWithValue("@Process", process);
                    cmd.Parameters.AddWithValue("@SubProcess" , subprocess);
                    cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

        }

    }
}
