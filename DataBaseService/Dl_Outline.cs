using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Data;

namespace QMS.DataBaseService
{
    public class Dl_Outline
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dlcon;
        private readonly HttpResponse response;

        public Dl_Outline(DL_Encrpt dL_Encrpt)
        {

            _enc = dL_Encrpt;
        }
        public async Task<DataTable> DahsboardCallQuality(int ProcessID, int SubProcessID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Get_LowScoring_CallAudits_ByProgram", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                       
                        cmd.Parameters.AddWithValue("@ProgramID", ProcessID);
                        cmd.Parameters.AddWithValue("@SubProgramID", SubProcessID);
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

        public async Task<DataTable> GetQuatile(int ProcessID, int SubProcessID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("SP_Get_CQScore_For_Q4_Agents", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ProgramID", ProcessID);
                        cmd.Parameters.AddWithValue("@SubProgramID", SubProcessID);
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

        public async Task<DataTable> AgentPerformence(int ProcessID, int SubProcessID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Get_Agentperformanceonmetrics", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ProgramID", ProcessID);
                        cmd.Parameters.AddWithValue("@SubProgramID", SubProcessID);
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

        public async Task<DataTable> AgentPerformence_LQ(int ProcessID, int SubProcessID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("GetAgentPerformanceWithTargets", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ProgramID", ProcessID);
                        cmd.Parameters.AddWithValue("@SubProgramID", SubProcessID);
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


        public async Task<DataTable> TransactionAudit(int ProcessID, int SubProcessID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("SP_Get_CallAudit_Defects", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ProgramID", ProcessID);
                        cmd.Parameters.AddWithValue("@SubProgramID", SubProcessID);
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




        public async Task<DataTable> TransactionAuditIdentification(int ProcessID, int SubProcessID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("SP_Get_Highest_Defect_count", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ProgramID", ProcessID);
                        cmd.Parameters.AddWithValue("@SubProgramID", SubProcessID);
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








    }
}
