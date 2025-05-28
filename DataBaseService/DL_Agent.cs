using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Net.Mail;
using System.Net;
using MailKit.Security;
using MimeKit;
using static System.Net.WebRequestMethods;
using System.Diagnostics;
using System.Transactions;
using System;

namespace QMS.DataBaseService
{
    public class DL_Agent
    {
        
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dcl;
        public DL_Agent(IConfiguration configuration, DL_Encrpt dL_Encrpt, DLConnection dL)
        {
            _con = configuration.GetConnectionString("Master_Con");
            _enc = dL_Encrpt;
            _dcl = dL;
        }

        public async Task SubmiteAgentSurvey(FeedbackViewModel model)
        {
            var transactionID = model.MonitoringId;

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    foreach (var question in model.Questions)
                    {
                        using (SqlCommand cmd = new SqlCommand("AgentSurveyDetails", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandTimeout = 0;

                    
                            cmd.Parameters.AddWithValue("@Operation", "SubmiteSgentSurvey");
                            cmd.Parameters.AddWithValue("@AgentComment", model.AgentComment);
                            cmd.Parameters.AddWithValue("@TrasnsactionID", model.MonitoringId);
                            cmd.Parameters.AddWithValue("@AgentId", UserInfo.UserName);
                            cmd.Parameters.AddWithValue("@QuestionId", question.QuestionId);
                            cmd.Parameters.AddWithValue("@QuestionText", question.QuestionText ?? string.Empty);
                            cmd.Parameters.AddWithValue("@Rating", question.Rating);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Optionally log the error
                throw;
            }
        }

        public async Task<DataTable> GetAssesment()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("AssesmentDetails", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);
                        cmd.Parameters.AddWithValue("@Mode", "getassesment");
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
        public async Task<DataTable> GetAgentSurveyDashboard()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("AgentSurveyDetails", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@AgentID", UserInfo.UserName);
                        cmd.Parameters.AddWithValue("@Operation", "GetAgentDashboard");
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
        public async Task<DataTable> Submiteassesment(AttemptTestViewModel model)
        {
            string userId = UserInfo.UserName;
            string connStr = UserInfo.Dnycon;
            DataTable DtScore = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();

                foreach (var question in model.Questions)
                {
               
                    if (question.SelectedOptionIds != null && question.SelectedOptionIds.Count > 0)
                    {
                        foreach (var optionId in question.SelectedOptionIds)
                        {
                            using (SqlCommand cmd = new SqlCommand("SaveUserAnswer", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@UserId", userId);
                                cmd.Parameters.AddWithValue("@TestID", model.TestID);
                                cmd.Parameters.AddWithValue("@QuestionId", question.QuestionId);
                                cmd.Parameters.AddWithValue("@SelectedOptionId", optionId);

                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                    else if (question.SelectedOptionId.HasValue)
                    {
                       
                        using (SqlCommand cmd = new SqlCommand("SaveUserAnswer", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@TestID", model.TestID);
                            cmd.Parameters.AddWithValue("@QuestionId", question.QuestionId);
                            cmd.Parameters.AddWithValue("@SelectedOptionId", question.SelectedOptionId.Value);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    else if (!string.IsNullOrEmpty(question.TextAnswer))
                    {
                        // Optional: if you want to save free-text answers, you may need a separate SP or extend your current one.
                        // For example:
                        using (SqlCommand cmd = new SqlCommand("SaveUserTextAnswer", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@QuestionId", question.QuestionId);
                            cmd.Parameters.AddWithValue("@TextAnswer", question.TextAnswer);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                // Call SP to mark test submission complete
                using (SqlCommand calcCmd = new SqlCommand("SubmitTest", conn))
                {
                    calcCmd.CommandType = CommandType.StoredProcedure;
                    calcCmd.Parameters.AddWithValue("@UserId", userId);
                    calcCmd.Parameters.AddWithValue("@TestID", model.TestID);

                    await calcCmd.ExecuteNonQueryAsync();
                }
               
                using (SqlCommand calcCmd = new SqlCommand("CheckScore", conn))
                {
                    calcCmd.CommandType = CommandType.StoredProcedure;
                    calcCmd.Parameters.AddWithValue("@UserId", userId);
                    calcCmd.Parameters.AddWithValue("@TestID", model.TestID);

                    SqlDataAdapter adpt = new SqlDataAdapter(calcCmd);
                    adpt.Fill(DtScore);
                }
            }

            return DtScore;
        }

        public async Task<AttemptTestViewModel> AttempTest(int TestID)
        {
            var model = new AttemptTestViewModel
            {
                Questions = new List<QuestionViewModel>(),
                TestID = TestID
            };

            string connStr = UserInfo.Dnycon;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("GetAttemptTestData", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TestID", TestID);

                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    var questionMap = new Dictionary<int, QuestionViewModel>();

                    while (await reader.ReadAsync())
                    {
                        int qid = (int)reader["QuestionId"];
                        if (!questionMap.ContainsKey(qid))
                        {
                            var q = new QuestionViewModel
                            {
                                QuestionId = qid,
                                QuestionText = reader["QuestionText"].ToString(),
                                AnswerType = reader["AnswerType"].ToString(),
                                Options = new List<AnswerOptionViewModel>()
                            };
                            questionMap[qid] = q;
                        }

                        questionMap[qid].Options.Add(new AnswerOptionViewModel
                        {
                            OptionId = (int)reader["OptionId"],
                            OptionText = reader["OptionText"].ToString()
                        });

                        // Set test metadata once
                        if (string.IsNullOrEmpty(model.TestName))
                        {
                            model.TestName = reader["TestName"].ToString();
                            model.TestCategory = reader["TestCategory"].ToString();
                        }
                    }

                    model.Questions = questionMap.Values.ToList();
                }
            }

            return model;
        }

        public async Task<DataTable> getMonitororIds()
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
                        cmd.Parameters.AddWithValue("@Mode", "GetMonirorDetails");
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

        public async Task<DataTable> getDisputeMonitororIds()
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
                        cmd.Parameters.AddWithValue("@Mode", "GetDisputeMonirorDetails");
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



        public async Task<DataTable> getZtSignOffData()
        {
            string UserNameENC = await _enc.EncryptAsync(UserInfo.UserName);
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Usp_GetZtSignOffDataAgentWise", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@AgentID", UserNameENC);
                       
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


        public async Task<DataTable> getAgentFeedbackSection( string TransactionID)
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
                        cmd.Parameters.AddWithValue("@Mode", "GetFeedbackSection");
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

        public async Task<DataSet> getCQScoreSection(string TransactionID)
        {
            DataSet dt = new DataSet();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("AgentSurvey", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                     
                        cmd.Parameters.AddWithValue("@Mode", "GetCQSctore");
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



        public async Task<DataSet> getCQScoreQADisputeSection(string TransactionID)
        {
            DataSet dt = new DataSet();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("AgentSurvey", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;

                        cmd.Parameters.AddWithValue("@Mode", "GetCQQADisputeSctore");
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
        public async Task SubmiteAgentAkowedge(string AgentComment , string TransactionID)
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
                        cmd.Parameters.AddWithValue("@AgentComment", AgentComment);
                        cmd.Parameters.AddWithValue("@Mode", "SubmiteAgentComment");
                        cmd.Parameters.AddWithValue("@TransactionID", TransactionID);
                        cmd.ExecuteNonQueryAsync();
                     
                    }
                }
            }
            catch (Exception ex)
            {

            }
          
        }

        public async Task SubmiteDisputeAgentAkowedge(string AgentComment, string TransactionID)
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
                        cmd.Parameters.AddWithValue("@AgentComment", AgentComment);
                        cmd.Parameters.AddWithValue("@Mode", "SubmiteIdpsuteAgentComment");
                        cmd.Parameters.AddWithValue("@TransactionID", TransactionID);
                        await cmd.ExecuteNonQueryAsync();

                    }
                }
            }
            catch (Exception ex)
            {

            }

        }


        public async Task DisputeAgentFeedback(string AgentComment, string TransactionID)
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
                        cmd.Parameters.AddWithValue("@Mode", "GetEmailIds");
                        
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt));
                    }
                }
            }
            catch (Exception ex)
            {

            }

            List<string> emailids = dt.AsEnumerable().Select(row => row["Emails"].ToString()).ToList();
            string message = "This Agent Is Dispiute this Feedback Treansaction ID : " + TransactionID +" Agent Name : " + UserInfo.UserName;
            bool isSent =  await SendEmailAsync(emailids, message, TransactionID);
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("AgentSurvey", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@AgentComment", AgentComment);
                        cmd.Parameters.AddWithValue("@Mode", "SubmiteDispute");
                        cmd.Parameters.AddWithValue("@TransactionID", TransactionID);
                        cmd.ExecuteNonQueryAsync();

                    }
                }
            }
            catch (Exception ex)
            {

            }

        }
        public async Task<bool> SendEmailAsync(List<string> recipientEmails, string Massage,string TransactionID)
        {
            string mailHost = "192.168.0.122";
            int mailPort = 587;
            string mailUserId = "reports@1point1.in";
            string mailPassword = "Pass@1234";
            string mailFrom = "reports@1point1.in";
            string Process = "";
            string Agent_Name = "";
            string Audit_Type = "";
            string Agent_Comment = "";
            string CQ_Score = "";
            string DisputerName = "";
            string Remarks = "";
            string TLname  = "";
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("usp_GetCallAuditDetailsByTransactionID", conn))
                {
                    DataTable dt2 = new DataTable();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@Mode", "GetAgentDispute");
                    cmd.Parameters.AddWithValue("@ID", TransactionID);
                    SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                    await Task.Run(() => adpt.Fill(dt2));

                    Process = dt2.Rows[0]["Process"].ToString();
                    Agent_Name = dt2.Rows[0]["Agent_Name"].ToString();
                    Audit_Type = dt2.Rows[0]["Audit_Type"].ToString();
                    Agent_Comment = dt2.Rows[0]["Agent_Comment"].ToString();
                    CQ_Score = dt2.Rows[0]["CQ_Score"].ToString();
                    DisputerName = dt2.Rows[0]["Createby"].ToString();
                    Remarks = dt2.Rows[0]["Remarks"].ToString();
                    TLname = dt2.Rows[0]["TLName"].ToString();
                }

               
            }




            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("QMS_EMAIL", mailFrom));

                // Add multiple recipients
                foreach (var recipientEmail in recipientEmails)
                {
                    emailMessage.To.Add(new MailboxAddress("", recipientEmail));
                }

                emailMessage.Subject = "Dispute feedBack Massage";
                var bodyBuilder = new BodyBuilder
                {
                    //TextBody = $"Dear TL/Manager,\n\nAgent Dispute is: {Massage}\n\nRegards,\nApplication Team"

                    TextBody = $@"
                                    Dear {TLname},
                                    
                                    You have received a dispute for the below-mentioned interaction.
                                    
                                    Details:-
                                    Transaction ID       :  {TransactionID}
                                    Agent Name           :  {Agent_Name}
                                    Process              :  {Process}
                                    Auditor              :  {Audit_Type}
                                    Audit Score          :  {CQ_Score}
                                    Overall Comments     :  {Remarks}
                                    Disputer Name        :  {DisputerName}
                                    Comments on Request for Re-evaluation:
                                    {Agent_Comment}
                                    
                                    Regards,
                                    Your QMS Team"



                };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                await client.ConnectAsync(mailHost, mailPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(mailUserId, mailPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);

                Console.WriteLine("OTP email sent successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send OTP email: {ex.Message}");
                return false;
            }
        }

        public async Task<DataTable> getPrrocessAndSubProcess(string TransactionID)
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
                        cmd.Parameters.AddWithValue("@TransactionID", TransactionID);
                        cmd.Parameters.AddWithValue("@Mode", "getProcessAndSubProcesss");
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


    }
}
