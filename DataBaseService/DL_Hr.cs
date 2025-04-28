using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Data;

namespace QMS.DataBaseService
{
    public class DL_Hr
    {

        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dlcon;
        private readonly HttpResponse response;

        public DL_Hr(DL_Encrpt dL_Encrpt)
        {

            _enc = dL_Encrpt;
        }

        public async Task<List<ZTcaseModel>> ZtCasePanel()
        {
            DataTable dt = new DataTable();
            List<ZTcaseModel> list = new List<ZTcaseModel>();

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("ZtViewDetails", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Mode", "ZT_Case_Panel");
                        cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);

                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt));
                    }
                }


                foreach (DataRow row in dt.Rows)
                {
                    ZTcaseModel model = new ZTcaseModel
                    {
                        ProgramID = row["ProgramID"]?.ToString(),
                        SubProgramID = row["SubProgramID"]?.ToString(),
                        AgentName = row["AgentName"]?.ToString(),
                        EmployeeID = row["EmployeeID"]?.ToString(),
                        AgentSupervsor = row["AgentSupervsor"]?.ToString(),
                        ZTRaisedBy = row["ZTRaisedBy"]?.ToString(),
                        ZTRaisedDate = row["ZTRaisedDate"]?.ToString(),
                        TransactionDate = row["TransactionDate"]?.ToString(),
                        ZTClassification = row["ZTClassification"]?.ToString(),
                        TransactionID = row["TransactionID"]?.ToString(),
                    };

                    list.Add(model);
                }
            }
            catch (Exception ex)
            {

            }

            return list;
        }


        public async Task<List<ZtHrCase>> ZtCaseHr()
        {
            DataTable dt = new DataTable();
            List<ZtHrCase> list = new List<ZtHrCase>();

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("ZtViewDetails", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Mode", "ZT_Case_HR");
                        cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);

                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt));
                    }
                }


                foreach (DataRow row in dt.Rows)
                {
                    ZtHrCase model = new ZtHrCase
                    {


                        ProgramID = row["ProgramID"]?.ToString(),
                        SubProgramID = row["SubProgramID"]?.ToString(),
                        AgentName = row["AgentName"]?.ToString(),
                        EmployeeID = row["EmployeeID"]?.ToString(),
                        AgentSupervsor = row["AgentSupervsor"]?.ToString(),
                        ZTRaisedBy = row["ZTRaisedBy"]?.ToString(),
                        ZTRaisedDate = row["ZTRaisedDate"]?.ToString(),
                        TransactionDate = row["TransactionDate"]?.ToString(),
                        ZTClassification = row["ZTClassification"]?.ToString(),
                        TransactionID = row["TransactionID"]?.ToString(),
                    };

                    list.Add(model);
                }
            }
            catch (Exception ex)
            {

            }

            return list;
        }


        public async Task<DataTable> GetPanelZtCaseViewDetails(string TransactionID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("ZtViewDetails", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Mode", "GetManagerView");
                        cmd.Parameters.AddWithValue("@TransactionID", TransactionID);
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



        public async Task SubmitePanelApprove(string Comment, string TransactionID)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("ZtViewDetails", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Panel_Comments", Comment);
                        cmd.Parameters.AddWithValue("@Mode", "UpdatePanelApproveComment");
                        cmd.Parameters.AddWithValue("@TransactionID", TransactionID);
                        cmd.ExecuteNonQueryAsync();

                    }
                }
            }
            catch (Exception ex)
            {

            }

        }


        public async Task SubmitePanelReject(string Comment, string TransactionID)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("ZtViewDetails", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Panel_Comments", Comment);
                        cmd.Parameters.AddWithValue("@Mode", "UpdatePanelRejectComment");
                        cmd.Parameters.AddWithValue("@TransactionID", TransactionID);
                        cmd.ExecuteNonQueryAsync();

                    }
                }
            }
            catch (Exception ex)
            {

            }

        }


        public async Task SubmiteHRApprove(string Comment, string TransactionID)
        {
            string AgentID = string.Empty;
            try
            {
                DataTable dt = new DataTable();
              
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("ZtViewDetails", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                       
                        cmd.Parameters.AddWithValue("@Mode", "UpdateHR");
                        cmd.Parameters.AddWithValue("@TransactionID", TransactionID);

                        using (SqlDataAdapter aspt = new SqlDataAdapter(cmd))
                        {
                            aspt.Fill(dt);
                        }

                        if (dt.Rows.Count > 0 && dt.Rows[0]["Agent_name"] != DBNull.Value)
                        {
                            AgentID = dt.Rows[0]["Agent_name"].ToString();
                        }


                    }
                }

                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    string Agentdec = await _enc.EncryptAsync(AgentID.ToString());
                    using (SqlCommand cmd = new SqlCommand("ZtViewDetails", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        
                        cmd.Parameters.AddWithValue("@Mode", "UpdateAgent");
                        cmd.Parameters.AddWithValue("@HR_Comments", Comment);
                        cmd.Parameters.AddWithValue("@TransactionID", TransactionID);
                        cmd.Parameters.AddWithValue("@UserName", Agentdec);
                          cmd.ExecuteNonQueryAsync();
                      

                    }
                }
            }
            catch (Exception ex)
            {

            }

        }

    }
}
