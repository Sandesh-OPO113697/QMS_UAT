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

        public async Task<int> SubmiteSectionEvaluation(List<OperationCallibration> model)
        {

            string processID = string.Empty;
            string SubProcess = string.Empty;
            string Comment = string.Empty;
            try
            {

                //using (SqlConnection conn2 = new SqlConnection(UserInfo.Dnycon))
                //{
                //    await conn2.OpenAsync();
                //    var TrasanctionID = model.Select(x => x.Transaction_ID).FirstOrDefault();

                //    using (SqlCommand cmd2 = new SqlCommand("calibration", conn2))
                //    {

                //        cmd2.CommandType = CommandType.StoredProcedure;
                //        cmd2.Parameters.AddWithValue("@Operation", "updateCallibrationByTLandMonitor");
                       
                
                //        cmd2.Parameters.AddWithValue("@TransactionID", TrasanctionID);

               

                //        await cmd2.ExecuteNonQueryAsync();
                //    }

                //}

                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    foreach (var section in model)
                    {
                        using (SqlCommand cmd = new SqlCommand("InsertSectionCouching_Details", conn))
                        {
                            processID = section.ProgramID.ToString();
                            SubProcess = section.SUBProgramID.ToString();
                         

                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Category", section.category);
                            cmd.Parameters.AddWithValue("@Level", section.level);
                            cmd.Parameters.AddWithValue("@SectionName", section.sectionName);
                            cmd.Parameters.AddWithValue("@parameters", section.parameters);
                            cmd.Parameters.AddWithValue("@subparameters", section.subparameters);
                            cmd.Parameters.AddWithValue("@QA_rating", section.qaRating);
                            cmd.Parameters.AddWithValue("@Scorable", section.scorable);
                            cmd.Parameters.AddWithValue("@Weightage", section.score);
                            cmd.Parameters.AddWithValue("@Commentssection", section.comments ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@TransactionID", section.Transaction_ID);
                            cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName);
                            cmd.Parameters.AddWithValue("@ProgramID", Convert.ToInt32(section.ProgramID));
                            cmd.Parameters.AddWithValue("@SubProgramID", Convert.ToInt32(section.SUBProgramID));
                            cmd.Parameters.AddWithValue("@fatal", section.fatal);
                            

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                using (SqlConnection conn2 = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn2.OpenAsync();

                     using (SqlCommand cmd2 = new SqlCommand("calibration", conn2))
                        {

                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.Parameters.AddWithValue("@Operation", "UpdateComment");
                        cmd2.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName);
                        cmd2.Parameters.AddWithValue("@ProgramID", processID);
                        cmd2.Parameters.AddWithValue("@SubProgramID", SubProcess);

                        cmd2.Parameters.AddWithValue("@Comment", Comment);

                        await cmd2.ExecuteNonQueryAsync();
                        }
                 
                }
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 0;
            }
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
                        Assigned_Date = row["CreatedDate"]?.ToString(),
                        ProgramID = row["ProgramID"]?.ToString(),
                        SubProgramID = row["SubProgramID"]?.ToString()

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
