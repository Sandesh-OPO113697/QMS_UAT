using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using OfficeOpenXml;

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

        public async Task UpdateNatrix(string Subprocess, IFormFile matrix)
        {
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("RemoveMatroix", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;

                    cmd.Parameters.AddWithValue("@SubProgramID", Subprocess);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            int successCount = 0, duplicateCount = 0, invalidCount = 0;
            string extension = Path.GetExtension(matrix.FileName);
            using (var stream = new MemoryStream())
            {
                await matrix.CopyToAsync(stream);
                stream.Position = 0;
                if (extension == ".xlsx")
                {
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {

                            string Matrix = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                            string Performace = worksheet.Cells[row, 2].Value?.ToString()?.Trim()?.ToUpper();




                            await InsertSubDisposition(Matrix, Performace, Subprocess);
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

                        string Matrix = currentRow.GetCell(0)?.ToString()?.Trim();
                        string Performace = currentRow.GetCell(1)?.ToString()?.Trim()?.ToUpper();






                        await InsertSubDisposition(Matrix, Performace, Subprocess);
                        successCount++;

                    }
                }
                else
                {

                }




            }
        }


        public async Task InsertSubDisposition(string Matrix, string Perform, string SubProcessID)
        {

            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("UpdatematrixMaster", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@MATRIX", Matrix);
                    cmd.Parameters.AddWithValue("@TARGET", Perform);

                    cmd.Parameters.AddWithValue("@SubProgramID", SubProcessID);
                    cmd.Parameters.AddWithValue("@CreateBy", UserInfo.UserName);

                    await cmd.ExecuteNonQueryAsync();
                }
            }


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
