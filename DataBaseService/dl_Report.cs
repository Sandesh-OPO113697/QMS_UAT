using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Data;
using NPOI.SS.Formula;
using Formula = QMS.Models.Formula;

namespace QMS.DataBaseService
{
    public class dl_Report
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dlcon;
        private readonly HttpResponse response;
        public dl_Report(DL_Encrpt dL_Encrpt)
        {

            _enc = dL_Encrpt;
        }

        public async Task<DataTable> GetFieldsByFormula(string formulaName, string programId, string subprogram, string agentname)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                await con.OpenAsync(); // Await the connection open

                using (SqlCommand cmd = new SqlCommand("GetFormulaFieldsByName", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FormulaName", formulaName);
                    cmd.Parameters.AddWithValue("@ProgramID", programId);
                    cmd.Parameters.AddWithValue("@SubprogramID", subprogram);
                    cmd.Parameters.AddWithValue("@AgentID", agentname);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(reader); // Load async reader directly into DataTable
                    }
                }
            }

            return dt;
        }


        public async Task<string> checkTemplate(string program, string subprogram, string formula)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("checktemplate", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@formulaname", formula);
                        cmd.Parameters.AddWithValue("@programID", program);
                        cmd.Parameters.AddWithValue("@SubprogramID", subprogram);

                        DataTable dt = new DataTable();
                        SqlDataAdapter aspt = new SqlDataAdapter(cmd);
                        aspt.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            return "created";
                        }
                        else
                        {
                            return "notcreated";
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return "";
        }
        public async Task<List<Formula>> GetTemplate(int pid, int sbis)
        {
            List<Formula> formulas = new List<Formula>();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("gettemplate", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);
                        cmd.Parameters.AddWithValue("@programID", pid);
                        cmd.Parameters.AddWithValue("@SubprogramID", sbis);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                formulas.Add(new Formula
                                {
                                    FormulaName = reader["FormulaName"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception or handle error
            }
            return formulas;
        }


        public async Task CreateFormula(FormulaModel model)
        {


            string connectionString = UserInfo.Dnycon;
            string fieldsString = string.Join(",", model.fields);

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_SaveFormula", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FormulaName", model.formulaName);
                cmd.Parameters.AddWithValue("@ProgramId", Convert.ToInt32(model.programId));
                cmd.Parameters.AddWithValue("@SubProgramId", Convert.ToInt32(model.subprogram));
                cmd.Parameters.AddWithValue("@Fields", fieldsString);
                cmd.Parameters.AddWithValue("@Username", UserInfo.UserName);

                try
                {
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();

                }
                catch (Exception ex)
                {

                }
            }


        }
    }
}