using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System.Diagnostics;
using System;

namespace QMS.DataBaseService
{
    public class DL_Module
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dcl;
        public DL_Module(IConfiguration configuration, DL_Encrpt dL_Encrpt, DLConnection dL)
        {
            _con = configuration.GetConnectionString("Master_Con");
            _enc = dL_Encrpt;
            _dcl = dL;
        }
        public async Task<DataTable> GetAgentListUpdated(string process ,int subProcess)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("GetGentListUpdated", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@process", process);
                        cmd.Parameters.AddWithValue("@subProcess", subProcess);
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
        public async Task<bool> RemoveAgents(List<string> empCodes, string process, int subProcess)
        {
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();

                    foreach (var empCode in empCodes)
                    {
                        using (var command = new SqlCommand("DeactivateAgents", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@EmpCodes", empCode);
                            command.Parameters.AddWithValue("@process", process);
                            command.Parameters.AddWithValue("@subProcess", subProcess);

                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }
            

            return true; 
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
                    cmd.Parameters.AddWithValue("@Mode", "Select_SUB_Program_Master_locationWise_WithPauseCount");
                    cmd.Parameters.AddWithValue("@p_username", LocationID);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Process.Add(new SelectListItem
                            {
                                Value = reader["id"].ToString(),
                                Text = reader["SubProcessName"].ToString() + "," + reader["Isactive"].ToString() + "," + reader["Number_Of_Pause"].ToString(),
                              
                            });
                        }
                    }
                }
            }
            return Process;
        }
        public async Task UpdatePauseCount(int ProcessID, int SubProcess, string Pause)
        {

            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    string query = "EditFormvalue";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "UpdatePasuseCount");
                        cmd.Parameters.AddWithValue("@Pausecontt", Pause);
                        cmd.Parameters.AddWithValue("@processID", ProcessID);
                        cmd.Parameters.AddWithValue("@SubprocessID", SubProcess);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch
            {

            }

        }

    }
}
