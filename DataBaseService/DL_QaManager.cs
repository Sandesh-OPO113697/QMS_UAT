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
    public class DL_QaManager
    {

        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dlcon;
        private readonly HttpResponse response;
        public DL_QaManager(DL_Encrpt dL_Encrpt)
        {

            _enc = dL_Encrpt;
        }
        public async Task SubmiteDisputeAkowedge(string QaComment, string calibration, string TransactionID, string CQscore)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("AgentSurvey", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@QAComment", QaComment);
                        cmd.Parameters.AddWithValue("@CalibrationComment", calibration);
                        cmd.Parameters.AddWithValue("@CQScore", CQscore);
                        cmd.Parameters.AddWithValue("@Mode", "SubmiteQADispute");
                        cmd.Parameters.AddWithValue("@TransactionID", TransactionID);
                        cmd.ExecuteNonQueryAsync();

                    }
                }
            }
            catch (Exception ex)
            {

            }

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("AgentSurvey", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Mode", "GetUserID");
                        cmd.Parameters.AddWithValue("@TransactionID", TransactionID);
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt));
                    }
                }



            }
            catch (Exception ex)
            {

            }

            string userid = dt.Rows[0]["AgentID"].ToString();
            string userID = await _enc.EncryptAsync(userid);

            DataTable dt2 = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("AgentSurvey", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Mode", "GetEmailID");
                        cmd.Parameters.AddWithValue("@UserName", userID);
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt2));
                    }
                }



            }
            catch (Exception ex)
            {

            }
            string Email = dt2.Rows[0]["Email"].ToString();
            await SendEmailAsync(Email, "Your Score Is Recalculated");
        }
        public async Task<bool> SendEmailAsync(string recipientEmails, string Massage)
        {
            string mailHost = "192.168.0.122";
            int mailPort = 587;
            string mailUserId = "reports@1point1.in";
            string mailPassword = "Pass@1234";
            string mailFrom = "reports@1point1.in";

            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("QMS_EMAIL", mailFrom));

                // Add recipient
                emailMessage.To.Add(new MailboxAddress("", recipientEmails));

                emailMessage.Subject = "Dispute feedBack Massage";
                var bodyBuilder = new BodyBuilder
                {
                    TextBody = $"Dear Agent,\n\nAgent Dispute is: {Massage}\n\nRegards,\nApplication Team"
                };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                await client.ConnectAsync(mailHost, mailPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(mailUserId, mailPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<DataTable> GetMonitporedSectionGriedAsync(int processID, int SubprocessID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("AgentSurvey", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Mode", "GetMotiroedSectionGried");
                        cmd.Parameters.AddWithValue("@processID", processID);
                        cmd.Parameters.AddWithValue("@SubprocessID", SubprocessID);
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
        public async Task<int> UpdateSectionByQAEvaluation(List<SectionAuditModel> model, string TransactionID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("UpdateSectionEvaluation", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "DeleteData");
                        cmd.Parameters.AddWithValue("@TransactionID", TransactionID);


                        await cmd.ExecuteNonQueryAsync();
                    }

                }

                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    foreach (var section in model)
                    {
                        using (SqlCommand cmd = new SqlCommand("UpdateSectionEvaluation", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Category", section.category);
                            cmd.Parameters.AddWithValue("@Level", section.level);
                            cmd.Parameters.AddWithValue("@SectionName", section.sectionName);
                            cmd.Parameters.AddWithValue("@QA_rating", section.qaRating);
                            cmd.Parameters.AddWithValue("@Scorable", section.scorable);
                            cmd.Parameters.AddWithValue("@Weightage", section.score);
                            cmd.Parameters.AddWithValue("@Commentssection", section.comments ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@TransactionID", TransactionID);
                            cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName);
                            cmd.Parameters.AddWithValue("@Operation", "Update");


                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                return 1; // Return 1 only after all records are inserted successfully
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 0; // Return 0 on failure
            }
        }
        public async Task<List<DisputeCallfeedbackModel>> DisputeAgentFeedback()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("AgentSurvey", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);
                        cmd.Parameters.AddWithValue("@Mode", "GetAgentList");

                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt));
                    }
                }
            }
            catch (Exception ex)
            {

            }

            List<string> UserEnc = dt.AsEnumerable().Select(row => row["UserName"].ToString()).ToList();
            List<string> UserDENC = new List<string>();

            foreach (var EncUsername in UserEnc)
            {
                UserDENC.Add(await _enc.DecryptAsync(EncUsername));
            }
            List<DisputeCallfeedbackModel> auditDetailsList = new List<DisputeCallfeedbackModel>();

            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();

                foreach (var agentId in UserDENC)
                {
                    using (SqlCommand cmd = new SqlCommand("AgentSurvey", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@UserName", agentId);
                        cmd.Parameters.AddWithValue("@Mode", "GetDisputeGrid");

                        SqlDataReader reader = await cmd.ExecuteReaderAsync();

                        while (await reader.ReadAsync())
                        {
                            auditDetailsList.Add(new DisputeCallfeedbackModel
                            {
                                TransactionID = reader["TransactionID"].ToString(),
                                AgentID = reader["AgentID"].ToString(),
                                TLName = reader["TLName"].ToString(),
                                MonitorBy = reader["MonitorBy"].ToString()
                            });
                        }

                        reader.Close();
                    }
                }
            }
            var distinctAuditDetailsList = auditDetailsList
    .GroupBy(x => new { x.TransactionID, x.AgentID, x.TLName, x.MonitorBy })
    .Select(g => g.First())
    .ToList();

            return distinctAuditDetailsList;

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

                    using (SqlCommand cmd = new SqlCommand("AgentSurvey", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Mode", "ZT_Case_Qa_manager");
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

        public async Task<DataTable> GetQaManagerZtCaseViewDetails(string TransactionID)
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


        public async Task SubmiteQaManagerApprove(string Comment, String ZTHistory, string TransactionID)
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
                        cmd.Parameters.AddWithValue("@QaManagerComment", Comment);
                        cmd.Parameters.AddWithValue("@ZTHistory", ZTHistory);
                        cmd.Parameters.AddWithValue("@Mode", "UpdateQaManagerApproveComment");
                        cmd.Parameters.AddWithValue("@TransactionID", TransactionID);
                        cmd.ExecuteNonQueryAsync();

                    }
                }
            }
            catch (Exception ex)
            {

            }

        }


        public async Task SubmiteQaManagerReject(string Comment, String ZTHistory, string TransactionID)
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
                        cmd.Parameters.AddWithValue("@QaManagerComment", Comment);
                        cmd.Parameters.AddWithValue("@ZTHistory", ZTHistory);
                        cmd.Parameters.AddWithValue("@Mode", "UpdateQaManagerRejectComment");
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
