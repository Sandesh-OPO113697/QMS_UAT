using QMS.Models;
using System.Data.SqlClient;
using System.Data;
using MimeKit;
using System.Net.Mail;
using QMS.Encription;
using System.Diagnostics;
using MailKit.Security;
using System.Net;


namespace QMS.DataBaseService
{
    public class DL_Operation
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dlcon;
        private readonly HttpResponse response;
   
        public DL_Operation(DL_Encrpt dL_Encrpt)
        {

            _enc = dL_Encrpt;
        }


        public async Task<List<ZTcaseModel>> ZtcaseShow()
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
                        cmd.Parameters.AddWithValue("@Mode", "ZT_Case_Ops_manager");
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


        public async Task<List<Calibration_ViewModel>> Participants_View()
        {
            DataTable dt = new DataTable();
            List<Calibration_ViewModel> list = new List<Calibration_ViewModel>();

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("calibration", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "CheckCalibration");
                        cmd.Parameters.AddWithValue("@Participants", UserInfo.UserName);

                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt));
                    }
                }


                foreach (DataRow row in dt.Rows)
                {
                    Calibration_ViewModel model = new Calibration_ViewModel
                    {


                        TransactionID = row["TransactionID"]?.ToString(),
                        Assigned_By = row["CreatedBy"]?.ToString(),
                        Assigned_Date = row["CreatedDate"]?.ToString()
                        
                    };

                    list.Add(model);
                }
            }
            catch (Exception ex)
            {

            }

            return list;
        }




        public async Task SubmiteOperationManagerApprove(string Comment, string TransactionID)
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
                        cmd.Parameters.AddWithValue("@OpsManagerComment", Comment);
                        cmd.Parameters.AddWithValue("@Mode", "UpdateOperationManagerApproveComment");
                        cmd.Parameters.AddWithValue("@TransactionID", TransactionID);
                        cmd.ExecuteNonQueryAsync();

                    }
                }
            }
            catch (Exception ex)
            {

            }

        }


        public async Task SubmiteOperationManagerReject(string Comment,string TransactionID)
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
                        cmd.Parameters.AddWithValue("@OpsManagerComment", Comment);
                        cmd.Parameters.AddWithValue("@Mode", "UpdateOperationManagerRejectComment");
                        cmd.Parameters.AddWithValue("@TransactionID", TransactionID);
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
