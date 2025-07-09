using QMS.Encription;
using QMS.Models;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Collections.Generic;
using MailKit.Security;
using MimeKit;

namespace QMS.DataBaseService
{
    public class Dl_UpdateManagement
    {

        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dcl;
        public Dl_UpdateManagement( DL_Encrpt dL_Encrpt, DLConnection dL)
        {
          
            _enc = dL_Encrpt;
            _dcl = dL;
        }
        public async Task InsertEmailNotificationAsync(string userCode, string subject, string body, string attachmentFileName,string attachmentBase64 ,  bool notified)
        {
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            using (SqlCommand cmd = new SqlCommand("UpdateManagement", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Operation", "Notification");
                cmd.Parameters.AddWithValue("@UserCode", userCode);
                cmd.Parameters.AddWithValue("@Subject", subject);
                cmd.Parameters.AddWithValue("@Body", body);
                cmd.Parameters.AddWithValue("@AttachmentFileName", (object)attachmentFileName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Notified", notified);
                cmd.Parameters.AddWithValue("@AttachmentBase64", (object)attachmentBase64 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }


        public async Task<bool> SendMail(string to, string subject, string body, string base64Attachment, string attachmentFileName)
        {
            string mailHost = "192.168.0.122";
            int mailPort = 587;
            string mailUserId = "reports@1point1.in";
            string mailPassword = "Pass@1234";
            string mailFrom = "reports@1point1.in";
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse("Notification Mail")); // Sender
                foreach (var address in to.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    email.To.Add(MailboxAddress.Parse(address.Trim()));
                }

                email.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = body
                };
                if (!string.IsNullOrEmpty(base64Attachment) && !string.IsNullOrEmpty(attachmentFileName))
                {
                    byte[] fileBytes = Convert.FromBase64String(base64Attachment);
                    builder.Attachments.Add(attachmentFileName, fileBytes);
                }

                email.Body = builder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                client.Connect(mailHost, mailPort, SecureSocketOptions.StartTls);
                client.Authenticate(mailUserId, mailPassword);
                client.Send(email);
                client.Disconnect(true);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public async Task<List<object>> GetTLAndAgentList(int ProcessID ,  int SubProcessID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("UpdateManagement", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "GetTlAndAgentList");
                        cmd.Parameters.AddWithValue("@ProgramID", ProcessID);
                        cmd.Parameters.AddWithValue("@SubProcessID", SubProcessID);
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        adpt.Fill(dt);
                    }
                }
            }
            catch( Exception ex)
            {

            }
           
            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                string empCode = row["EmpCode"].ToString();
                string tlCode = row["TL_Code"].ToString();

                // Fetch emails using a new method (assuming it returns a tuple or dictionary)
                var emails = await GetEmailsByEmpAndTl(empCode, tlCode);

                list.Add(new
                {
                    EmpCode = empCode,
                    EmpName = row["EmpName"].ToString(),
                    TL_Code = tlCode,
                    TL_Name = row["TL_Name"].ToString(),
                    empemails = emails.EmpEmail,
                    tlemail = emails.TLEmail
                });
            }

            return list;

        }



        public async Task<List<object>> GetTLList(int ProcessID, int SubProcessID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("UpdateManagement", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "GetTlList");
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
                string empCode = row["TL_Code"].ToString();
                string tlCode = row["TL_Code"].ToString();

                // Fetch emails using a new method (assuming it returns a tuple or dictionary)
                var emails = await GetEmailsByEmpAndTl(empCode, tlCode);

                list.Add(new
                {
                    EmpCode = empCode,
                    EmpName = row["TL_Code"].ToString(),
                    TL_Code = tlCode,
                    //TL_Name = row["TL_Name"].ToString(),
                    //empemails = emails.EmpEmail,
                    //tlemail = emails.TLEmail
                });
            }

            return list;

        }




        public async Task<List<object>> GetAgentList(AgentRequest request)
        {
            DataTable dt = new DataTable();
            var resultList = new List<object>();

            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();

                    foreach (string agentId in request.agentTlList)
                    {
                        using (SqlCommand cmd = new SqlCommand("UpdateManagement", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Operation", "GetAgentList");
                            cmd.Parameters.AddWithValue("@EmpCode", agentId); // Assuming this param is expected

                            SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                            DataTable tempDt = new DataTable();
                            adpt.Fill(tempDt);

                            foreach (DataRow row in tempDt.Rows)
                            {
                                string empCode = row["EmpName"]?.ToString();
                                string tlCode = row["EmpName"]?.ToString();

                                // Optional: Fetch emails
                                // var emails = await GetEmailsByEmpAndTl(empCode, tlCode);

                                resultList.Add(new
                                {
                                    EmpCode = empCode,
                                    EmpName = row["EmpName"]?.ToString(),
                                    TL_Code = tlCode,
                                    TL_Name = row["EmpName"]?.ToString()

                                    // Uncomment if email support is ready
                                    // EmpEmail = emails.EmpEmail,
                                    // TLEmail = emails.TLEmail
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle/log error (optionally throw)
                Console.WriteLine("Error in GetAgentList: " + ex.Message);
            }

            return resultList;
        }



        public async Task<(string EmpEmail, string TLEmail)> GetEmailsByEmpAndTl(string empCode, string tlCode)
        {
            string empEmail = "", tlEmail = "";

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    using (SqlCommand cmd = new SqlCommand("UpdateManagement", conn))
                    {
                        string Emp = await _enc.EncryptAsync(empCode);
                        string tl = await _enc.EncryptAsync(tlCode);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@EmpCode", Emp);
                        cmd.Parameters.AddWithValue("@TLCode", tl);
                        cmd.Parameters.AddWithValue("@Operation", "GetTandAgentMail");

                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                empEmail = reader["EmpEmail"].ToString();
                                tlEmail = reader["TLEmail"].ToString();
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }
           

            return (empEmail, tlEmail);
        }
    }
}
