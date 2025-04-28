using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using NPOI.POIFS.Crypt.Dsig;

namespace QMS.DataBaseService
{
    public class Dl_Coaching
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dcl;
        public Dl_Coaching(DL_Encrpt dL_Encrpt, DLConnection dL)
        {

            _enc = dL_Encrpt;
            _dcl = dL;
        }
        public async Task<List<object>> GetTLAndAgentList(int ProcessID, int SubProcessID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("UpdateManagement", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "GetAgentListCouchingDone");
                        cmd.Parameters.AddWithValue("@ProgramID", ProcessID);
                        cmd.Parameters.AddWithValue("@SubProcessID", SubProcessID);
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        adpt.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                string empCode = row["EmpCode"].ToString();
                
              
                
                list.Add(new
                {
                    EmpCode = empCode,
                    EmpName = row["EmpName"].ToString(),
                
                   
                });
            }

            return list;

        }
        public async Task ExtendCauching(string Program, string subprogram , string Agent)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Coaching", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "ExtendCauching");
                        cmd.Parameters.AddWithValue("@ProgramID", Program);
                        cmd.Parameters.AddWithValue("@SubProcessID", subprogram);
                        cmd.Parameters.AddWithValue("@AgentID", Agent);
                       await cmd.ExecuteNonQueryAsync();

                    }
                }
            }
            catch (Exception ex)
            {

            }

        }

        public async Task PIPCauching(string Program, string subprogram, string Agent)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Coaching", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "PIP");
                        cmd.Parameters.AddWithValue("@ProgramID", Program);
                        cmd.Parameters.AddWithValue("@SubProcessID", subprogram);
                        cmd.Parameters.AddWithValue("@AgentID", Agent);
                        await cmd.ExecuteNonQueryAsync();

                    }
                }
            }
            catch (Exception ex)
            {

            }

        }
        public async Task ClosedCauching(string Program, string subprogram, string Agent)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Coaching", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "ClosedCauching");
                        cmd.Parameters.AddWithValue("@ProgramID", Program);
                        cmd.Parameters.AddWithValue("@SubProcessID", subprogram);
                        cmd.Parameters.AddWithValue("@AgentID", Agent);
                        await cmd.ExecuteNonQueryAsync();

                    }
                }
            }
            catch (Exception ex)
            {

            }

        }


        public async Task SubmitCountingAsync(List<MatrixItem> matrixData, CoachingFormData formData)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("Coaching", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "InsertCaoutingDate");
                        cmd.Parameters.AddWithValue("@ProgramID", formData.ProgramID);
                        cmd.Parameters.AddWithValue("@SubProcessID", formData.SUBProgramID);
                        cmd.Parameters.AddWithValue("@AgentID", formData.AgentID);
                        cmd.Parameters.AddWithValue("@QAID", formData.QaManager);
                        cmd.Parameters.AddWithValue("@R_date_1", formData.Review1);
                        cmd.Parameters.AddWithValue("@R_date_2", formData.Review2);
                        cmd.Parameters.AddWithValue("@R_date_3", formData.Review3);
                        cmd.Parameters.AddWithValue("@R_date_4", formData.Review4);

                        cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Loop through matrix items
                    foreach (var data in matrixData)
                    {
                        using (SqlCommand cmd = new SqlCommand("Coaching", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Operation", "InsertMatrix");
                            cmd.Parameters.AddWithValue("@AgentID", formData.AgentID);
                            cmd.Parameters.AddWithValue("@Matrix", data.Metric);
                            cmd.Parameters.AddWithValue("@Target", data.Target);
                            cmd.Parameters.AddWithValue("@QAID", formData.QaManager);
                            cmd.Parameters.AddWithValue("@Actual_Performance", data.Actual);
                            cmd.Parameters.AddWithValue("@ReviewDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SelectedMatrix", data.Selected);
                            cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // You should ideally log this exception or rethrow it
                Console.WriteLine("Error in SubmitCountingAsync: " + ex.Message);
                throw;
            }
        }

        public async Task SubmitExtendedCountingAsync(List<MatrixItem> matrixData, CoachingFormData formData)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("Coaching", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "SubmiteExtendCouchingDate");
                        cmd.Parameters.AddWithValue("@ProgramID", formData.ProgramID);
                        cmd.Parameters.AddWithValue("@SubProcessID", formData.SUBProgramID);
                        cmd.Parameters.AddWithValue("@AgentID", formData.AgentID);
                        cmd.Parameters.AddWithValue("@QAID", formData.QaManager);
                        cmd.Parameters.AddWithValue("@R_date_5", formData.Review5);
                        cmd.Parameters.AddWithValue("@R_date_6", formData.Review6);
                  
                        cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    using (SqlCommand cmd = new SqlCommand("Coaching", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "UpdateMatrixMatrix");
                        cmd.Parameters.AddWithValue("@AgentID", formData.AgentID);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    foreach (var data in matrixData)
                    {
                        using (SqlCommand cmd = new SqlCommand("Coaching", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Operation", "deleteandUpdatematrix");
                            cmd.Parameters.AddWithValue("@AgentID", formData.AgentID);
                            cmd.Parameters.AddWithValue("@Matrix", data.Metric);
                            cmd.Parameters.AddWithValue("@Target", data.Target);
                            cmd.Parameters.AddWithValue("@QAID", formData.QaManager);
                            cmd.Parameters.AddWithValue("@ProgramID", formData.ProgramID);
                            cmd.Parameters.AddWithValue("@SubProcessID", formData.SUBProgramID);
                            cmd.Parameters.AddWithValue("@Actual_Performance", data.Actual);
                            cmd.Parameters.AddWithValue("@ReviewDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SelectedMatrix", data.Selected);
                            cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // You should ideally log this exception or rethrow it
                Console.WriteLine("Error in SubmitCountingAsync: " + ex.Message);
                throw;
            }
        }



        public async Task<List<object>> GetQaManagerList(int ProcessID, int SubProcessID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Coaching", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "GetQaManager");
                        cmd.Parameters.AddWithValue("@ProgramID", ProcessID);
                        cmd.Parameters.AddWithValue("@SubProcessID", SubProcessID);
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        adpt.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
               
                list.Add(new
                {

                    User_Id = row["User_Id"].ToString(),

                    UserName = row["UserName"].ToString(),
                   
                });
            }

            return list;

        }

        public async Task<List<object>> GetQaManagerAndteamLeaderList(int ProcessID, int SubProcessID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Coaching", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "GetQaManagerAndTeamLeader");
                        cmd.Parameters.AddWithValue("@ProgramID", ProcessID);
                        cmd.Parameters.AddWithValue("@SubProcessID", SubProcessID);
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        adpt.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {

                list.Add(new
                {

                    User_Id = row["User_Id"].ToString(),

                    UserName = row["UserName"].ToString(),

                });
            }

            return list;

        }

        public async Task<DataTable> GetActualPerformanceList(string AgentID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                  await  con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Coaching", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "ActualPerformance");
                        cmd.Parameters.AddWithValue("@AgentID", AgentID);
                      
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        adpt.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return dt;
        }

        public async Task<DataTable> GetSelectedMatrixList(string AgentID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Coaching", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "Selectedmatrix");
                        cmd.Parameters.AddWithValue("@AgentID", AgentID);

                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        adpt.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return dt;
        }


        public async Task<DataTable> CheckCouchingExtendOrnot(string AgentID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                   await  con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Coaching", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "CheckCouchingExtend");
                        cmd.Parameters.AddWithValue("@AgentID", AgentID);

                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        adpt.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return dt;
        }

        public async Task<DataTable> GetCoutingPlanDetailsList(string AgentID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                     await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Coaching", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "ActualCochingDetailds");
                        cmd.Parameters.AddWithValue("@AgentID", AgentID);

                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        adpt.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return dt;
        }
        public async Task<List<object>> GetMatrixList(int ProcessID, int SubProcessID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Coaching", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "GetMatrixByProcess");
                        cmd.Parameters.AddWithValue("@ProgramID", ProcessID);
                        cmd.Parameters.AddWithValue("@SubProcessID", SubProcessID);
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        adpt.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {

                list.Add(new
                {

                    MATRIX = row["MATRIX"].ToString(),

                    TARGET = row["TARGET"].ToString(),

                });
            }

            return list;

        }


        public async Task<List<object>> GetMatrixByProcessList(string ProcessID, string SubProcessID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Coaching", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "GetMatrixByProcessandSUb");
                        cmd.Parameters.AddWithValue("@ProgramID", ProcessID);
                        cmd.Parameters.AddWithValue("@SubProcessID", SubProcessID);
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        adpt.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {

                list.Add(new
                {

                    MATRIX = row["MATRIX"].ToString(),

                    TARGET = row["TARGET"].ToString(),

                });
            }

            return list;

        }
    }

}
