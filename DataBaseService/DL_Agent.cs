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

        public async Task<DataTable> getCQScoreSection(string TransactionID)
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



        public async Task<DataTable> getCQScoreQADisputeSection(string TransactionID)
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
                        cmd.ExecuteNonQueryAsync();

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
            bool isSent =  SendEmail(emailids, message);
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
        public bool SendEmail(List<string> recipientEmails, string Massage)
        {
            string mailHost = "192.168.0.122";
            int mailPort = 587;
            string mailUserId = "reports@1point1.in";
            string mailPassword = "Pass@1234";
            string mailFrom = "reports@1point1.in";
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("Sender", mailFrom));

                // Add multiple recipients
                foreach (var recipientEmail in recipientEmails)
                {
                    emailMessage.To.Add(new MailboxAddress("", recipientEmail));
                }

                emailMessage.Subject = "Dispute feedBack Massage";
                var bodyBuilder = new BodyBuilder
                {
                    TextBody = $"Dear TL/Manager,\n\nAgent Dispute  is: {Massage}\n\nRegards,\nApplication Team"
                };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                client.Connect(mailHost, mailPort, SecureSocketOptions.StartTls);
                client.Authenticate(mailUserId, mailPassword);
                client.Send(emailMessage);
                client.Disconnect(true);

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
