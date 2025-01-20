using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

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
