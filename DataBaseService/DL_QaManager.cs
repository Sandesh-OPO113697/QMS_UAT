using QMS.Models;
using System.Data.SqlClient;
using System.Data;
using MimeKit;
using System.Net.Mail;
using QMS.Encription;
using System.Diagnostics;
using MailKit.Security;
using System.Net;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using OfficeOpenXml;
using Org.BouncyCastle.Asn1.X509;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Tsp;
using NPOI.POIFS.Crypt.Dsig;
using System.Transactions;

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
        public async Task SubmiteAgentSurvey(PerformanceData data)
        {


            var AgentID = data.AgentID;
            var ProcessID = data.ProgramID;
            var subProgram = data.SUBProgramID;
            
            try
            {
                foreach (var perfId in data.SelectedPerformance)
                {
                    using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                    {
                        await con.OpenAsync();
                        using (SqlCommand cmd = new SqlCommand("AgentSurveyDetails", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Operation", "SubmiteAgentTrasnacation");
                            cmd.Parameters.AddWithValue("@AgentID", AgentID);
                            cmd.Parameters.AddWithValue("@ProcessID", ProcessID);
                            cmd.Parameters.AddWithValue("@subProgram", subProgram);
                            cmd.Parameters.AddWithValue("@TrasnsactionID", perfId);
                            cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName);
                            await cmd.ExecuteNonQueryAsync();


                        }
                    }
                }
               
            }
            catch (Exception ex)
            {

            }

          
        }
        public async Task<DataTable> GetTransactionIDByUser(string AgentID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("AgentSurveyDetails", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "GetTransactionID");
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

        public async Task<int> UpdateAgentacknowledements()
        {
            string UserNameENC = await _enc.EncryptAsync(UserInfo.UserName);
            int rowsAffected = 0;
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                using (SqlCommand cmd = new SqlCommand("usp_UpdateZtTriggerSignoff", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Agent", UserNameENC);
                    await conn.OpenAsync();
                    object result = await cmd.ExecuteNonQueryAsync();
                    rowsAffected += Convert.ToInt32(result);
                }
            }
            return rowsAffected;
        }

        public async Task<int> InsertZtSignOff(List<ZtSignOffSectionData> sections)
        {
            int rowsAffected = 0;

            if (sections == null || sections.Count == 0)
                return 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    var first = sections[0];
                    string ProgramID = first.ProgramID;
                    string SUBProgramID = first.SUBProgramID;
                    string ExpiryDate = first.ExpiryDate.ToString();
                    int insertedId;

                    using (SqlCommand cmd = new SqlCommand("usp_ZtTriggerSignoff", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ProgramID", first.ProgramID);
                        cmd.Parameters.AddWithValue("@SubProgramID", first.SUBProgramID);
                        cmd.Parameters.AddWithValue("@ExpiryDate", first.ExpiryDate);
                        cmd.Parameters.AddWithValue("@CreateBy", UserInfo.UserName);

                        var outputIdParam = new SqlParameter("@InsertedId", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputIdParam);

                        await cmd.ExecuteNonQueryAsync();

                        insertedId = (int)outputIdParam.Value;
                    }

                    DataTable dt2 = new DataTable();
                    using (SqlCommand cmd3 = new SqlCommand("usp_GetZtuserMaapingAgent", conn))
                    {
                        cmd3.CommandType = CommandType.StoredProcedure;
                        cmd3.CommandTimeout = 0;
                        cmd3.Parameters.AddWithValue("@ProgramID", first.ProgramID);
                        cmd3.Parameters.AddWithValue("@SubProgramID", first.SUBProgramID);
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd3);
                        await Task.Run(() => adpt.Fill(dt2));
                    }

                    foreach (DataRow row in dt2.Rows)
                    {
                        using (SqlCommand insertCmd = new SqlCommand("usp_ZTP_ZtSignOffAgentMapping", conn))
                        {
                            String agentId = row["EMPID"].ToString();
                            insertCmd.CommandType = CommandType.StoredProcedure;
                            insertCmd.Parameters.AddWithValue("@AgentID", agentId);
                            insertCmd.Parameters.AddWithValue("@ZtsignOffID", insertedId);
                            insertCmd.Parameters.AddWithValue("@createdby", UserInfo.UserName);
                            await insertCmd.ExecuteNonQueryAsync();
                        }
                    }

                    foreach (var section in sections)
                    {
                        using (SqlCommand cmd2 = new SqlCommand("usp_ZTP_SignOffTrigger_Details", conn))
                        {
                            cmd2.CommandType = CommandType.StoredProcedure;
                            cmd2.Parameters.AddWithValue("@ZtSignOffID", insertedId);
                            cmd2.Parameters.AddWithValue("@ProgramID", section.ProgramID);
                            cmd2.Parameters.AddWithValue("@SubProgramID", section.SUBProgramID);
                            cmd2.Parameters.AddWithValue("@Parameter", section.Parameter ?? "");
                            cmd2.Parameters.AddWithValue("@Process_1", section.Procedure1 ?? "");
                            cmd2.Parameters.AddWithValue("@Process_2", section.Procedure2 ?? "");
                            cmd2.Parameters.AddWithValue("@Process_3", section.Procedure3 ?? "");
                            cmd2.Parameters.AddWithValue("@Process_4", section.Procedure4 ?? "");
                            cmd2.Parameters.AddWithValue("@Process_5", section.Procedure5 ?? "");
                            cmd2.Parameters.AddWithValue("@Process_6", section.Procedure6 ?? "");
                            cmd2.Parameters.AddWithValue("@Process_7", section.Procedure7 ?? "");
                            cmd2.Parameters.AddWithValue("@Process_8", section.Procedure8 ?? "");
                            cmd2.Parameters.AddWithValue("@Process_9", section.Procedure9 ?? "");
                            cmd2.Parameters.AddWithValue("@Process_10", section.Procedure10 ?? "");
                            cmd2.Parameters.AddWithValue("@createdby", UserInfo.UserName);
                            cmd2.CommandTimeout = 0;
                            object result = await cmd2.ExecuteNonQueryAsync();
                            rowsAffected += Convert.ToInt32(result);

                            // rowsAffected += await cmd2.ExecuteNonQueryAsync();

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting dynamic fields: " + ex.Message);
            }

            return rowsAffected;
        }


        public async Task UploadAPR(string ProcessID, string Subprocess, IFormFile file)
        {
            int successCount = 0, duplicateCount = 0, invalidCount = 0;
            string extension = Path.GetExtension(file.FileName);


            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;
                if (extension == ".xlsx")  // Handle modern Excel files
                {
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {

                            string AgentID = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                            string C_SAT = worksheet.Cells[row, 2].Value?.ToString()?.Trim()?.ToUpper();
                            string NPS = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                            string FCR = worksheet.Cells[row, 4].Value?.ToString()?.Trim();

                            string Repeat = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                            string AHT = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                            string Sales_Conversion = worksheet.Cells[row, 7].Value?.ToString()?.Trim();
                            string Resolution = worksheet.Cells[row, 8].Value?.ToString()?.Trim();

                            string C_SAT_Q = worksheet.Cells[row, 9].Value?.ToString()?.Trim()?.ToUpper();
                            string NPS_Q = worksheet.Cells[row, 10].Value?.ToString()?.Trim();
                            string FCR_Q = worksheet.Cells[row, 11].Value?.ToString()?.Trim();

                            string Repeat_Q = worksheet.Cells[row, 12].Value?.ToString()?.Trim();
                            string AHT_Q = worksheet.Cells[row, 13].Value?.ToString()?.Trim();
                            string Sales_Conversion_Q = worksheet.Cells[row, 14].Value?.ToString()?.Trim();
                            string Resolution_Q = worksheet.Cells[row, 15].Value?.ToString()?.Trim();

                            await InsertMatrixList(AgentID, C_SAT, NPS, FCR, ProcessID, Subprocess.ToString(), Repeat, AHT, Sales_Conversion, Resolution, C_SAT_Q, NPS_Q, FCR_Q, Repeat_Q, AHT_Q, Sales_Conversion_Q, Resolution_Q);
                            successCount++;
                        }
                    }

                }
                else if (extension == ".xls")
                {
                    HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                    ISheet sheet = hssfwb.GetSheetAt(0);
                    int rowCount = sheet.PhysicalNumberOfRows;

                    for (int row = 1; row < rowCount; row++)
                    {
                        IRow currentRow = sheet.GetRow(row);

                        string AgentID = currentRow.GetCell(0)?.ToString()?.Trim();
                        string C_SAT = currentRow.GetCell(1)?.ToString()?.Trim()?.ToUpper();
                        string NPS = currentRow.GetCell(2)?.ToString()?.Trim();
                        string FCR = currentRow.GetCell(3)?.ToString()?.Trim();

                        string Repeat = currentRow.GetCell(4)?.ToString()?.Trim();
                        string AHT = currentRow.GetCell(5)?.ToString()?.Trim();
                        string Sales_Conversion = currentRow.GetCell(6)?.ToString()?.Trim();
                        string Resolution = currentRow.GetCell(7)?.ToString()?.Trim();

                        string C_SAT_Q = currentRow.GetCell(8)?.ToString()?.Trim()?.ToUpper();
                        string NPS_Q = currentRow.GetCell(9)?.ToString()?.Trim();
                        string FCR_Q = currentRow.GetCell(10)?.ToString()?.Trim();

                        string Repeat_Q = currentRow.GetCell(11)?.ToString()?.Trim();
                        string AHT_Q = currentRow.GetCell(12)?.ToString()?.Trim();
                        string Sales_Conversion_Q = currentRow.GetCell(12)?.ToString()?.Trim();
                        string Resolution_Q = currentRow.GetCell(14)?.ToString()?.Trim();


                        await InsertMatrixList(AgentID, C_SAT, NPS, FCR, ProcessID, Subprocess.ToString(), Repeat, AHT, Sales_Conversion, Resolution, C_SAT_Q, NPS_Q, FCR_Q, Repeat_Q, AHT_Q, Sales_Conversion_Q, Resolution_Q);
                        successCount++;
                    }
                }
                else
                {
                }
            }
        }
        public async Task InsertMatrixList(string AgentID, string C_SAT, string NPS, string FCR, string ProgramID, string SubProgramID, string Repeat, string AHT, string Sales_Conversion, string ResolutionT, string C_SAT_Q, string NPS_Q, string FCR_Q, string Repeat_Q, string AHT_Q, string Sales_Conversion_Q, string Resolution_Q)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("UploadAPR", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@ProgramID", ProgramID);
                        cmd.Parameters.AddWithValue("@SubProgramID", SubProgramID);
                        cmd.Parameters.AddWithValue("@AgentID", AgentID);
                        cmd.Parameters.AddWithValue("@C_SAT", C_SAT);
                        cmd.Parameters.AddWithValue("@NPS", NPS);
                        cmd.Parameters.AddWithValue("@FCR", FCR);
                        cmd.Parameters.AddWithValue("@Repeat", Repeat);
                        cmd.Parameters.AddWithValue("@AHT", AHT);
                        cmd.Parameters.AddWithValue("@Sales_Conversion", Sales_Conversion);
                        cmd.Parameters.AddWithValue("@ResolutionT", ResolutionT);
                        cmd.Parameters.AddWithValue("@C_SAT_Q", C_SAT_Q);
                        cmd.Parameters.AddWithValue("@NPS_Q", NPS_Q);
                        cmd.Parameters.AddWithValue("@FCR_Q", FCR_Q);
                        cmd.Parameters.AddWithValue("@Repeat_Q", Repeat_Q);
                        cmd.Parameters.AddWithValue("@AHT_Q", AHT_Q);
                        cmd.Parameters.AddWithValue("@Sales_Conversion_Q", Sales_Conversion_Q);
                        cmd.Parameters.AddWithValue("@ResolutionT_Q", Resolution_Q);
                        cmd.Parameters.AddWithValue("@CreateBy", UserInfo.UserName);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task SubmiteCochingComment(string AgentID, string ReviewDate, string Comment, string NumberOFReview)
        {
            string EncUser = await _enc.EncryptAsync(UserInfo.UserName);
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Coaching", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "CommentsSubmit");
                        cmd.Parameters.AddWithValue("@AgentID", AgentID);
                        cmd.Parameters.AddWithValue("@ReviewDate", ReviewDate);
                        cmd.Parameters.AddWithValue("@Comment", Comment);
                        cmd.Parameters.AddWithValue("@NumberOFReview", NumberOFReview);
                        cmd.Parameters.AddWithValue("@UserName", EncUser);
                        await cmd.ExecuteNonQueryAsync();

                    }
                }
            }
            catch (Exception ex)
            {

            }
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
                        await cmd.ExecuteNonQueryAsync();

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
            await SendEmailAsync(Email, "Your Score Is Recalculated", TransactionID);
        }
        public async Task<bool> SendEmailAsync(string recipientEmails, string Massage,string TransactionID)
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
            string TLname = "";
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

                // Add recipient
                emailMessage.To.Add(new MailboxAddress("", recipientEmails));

                emailMessage.Subject = "Dispute calibrated reply";
                var bodyBuilder = new BodyBuilder
                {
                    TextBody = $@"
                                    Dear {Agent_Name},
                                    
                                    The Super-Auditor replies the dispute.
                                    
                                    Details:-
                                    Transaction ID       :  {TransactionID}
                                    Agent Name           :  {Agent_Name}
                                    Process              :  {Process}
                                    Auditor              :  {Audit_Type}
                                    Audit Score          :  {CQ_Score}
                                    Overall Comments     :  {Remarks}
                                    
                              
                                    
                                    Regards,
                                    Your  Quality Team"
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

        public async Task<DataTable> GetMonitporedSectionGriedAsync(int processID, int SubprocessID, string TransactionID)
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
                            cmd.Parameters.AddWithValue("@Fatal", section.fatal);
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


        public async Task<List<AgentToQASurveyModel>> AgentToQASurveylist()
        {
            DataTable dt = new DataTable();
            List<AgentToQASurveyModel> list = new List<AgentToQASurveyModel>();

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("GetAgentSurveyResponses", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                       
                        cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);

                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt));
                    }
                }


                foreach (DataRow row in dt.Rows)
                {
                    AgentToQASurveyModel model = new AgentToQASurveyModel
                    {


                        Transaction_ID = row["Transaction_ID"]?.ToString(),
                        AgentID = row["AgentID"]?.ToString(),
                      
                      
                    };

                    list.Add(model);
                }
            }
            catch (Exception ex)
            {

            }

            return list;
        }



        public async Task<List<AgentToQASurveyModel>> AgentToQASurveylistView(string TransactionID)
        {
            DataTable dt = new DataTable();
            List<AgentToQASurveyModel> list = new List<AgentToQASurveyModel>();

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("GetAgentSurveyResponsesview", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;

                        cmd.Parameters.AddWithValue("@TransactionID", TransactionID);

                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt));
                    }
                }

              

                foreach (DataRow row in dt.Rows)
                {
                    AgentToQASurveyModel model = new AgentToQASurveyModel
                    {


                        Transaction_ID = row["Transaction_ID"]?.ToString(),
                        AgentID = row["AgentID"]?.ToString(),
                        QuestionText = row["QuestionText"]?.ToString(),
                        Rating = row["Rating"]?.ToString(),
                        AgentComment = row["AgentComment"]?.ToString(),
                        AgentReponseDate = row["AgentReponseDate"]?.ToString()

                       

                    };

                    list.Add(model);
                }
            }
            catch (Exception ex)
            {

            }

            return list;
        }


        public async Task<List<ReviewDataModel>> GetCaoutingList()
        {
            DataTable dt = new DataTable();
            List<ReviewDataModel> list = new List<ReviewDataModel>();
            string endcuserName = await _enc.EncryptAsync(UserInfo.UserName);
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("Coaching", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "GetCouching");
                        cmd.Parameters.AddWithValue("@UserName", endcuserName);

                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt));
                    }
                }


                foreach (DataRow row in dt.Rows)
                {
                    ReviewDataModel model = new ReviewDataModel
                    {


                        AgentID = row["AgentID"]?.ToString(),
                        ProcessName = row["ProcessName"]?.ToString(),
                        SubProcess = row["SubProcessName"]?.ToString(),
                        FirstReview = row["1st Review"]?.ToString(),
                        Comment1 = row["Comment 1"]?.ToString(),
                        SecondReview = row["2nd Review"]?.ToString(),
                        Comment2 = row["Comment 2"]?.ToString(),
                        ThirdReview = row["3rd Review"]?.ToString(),
                        Comment3 = row["Comment 3"]?.ToString(),
                        FourthReview = row["4th Review"]?.ToString(),
                        Comment4 = row["Comment 4"]?.ToString(),
                        FifthReview = row["5th Review"]?.ToString(),
                        Comment5 = row["Comment 5"]?.ToString(),
                        SixReview = row["6th Review"]?.ToString(),
                        Comment6 = row["Comment 6"]?.ToString(),
                        CoachingStatus = row["CoachingStatus"]?.ToString()
                    };

                    list.Add(model);
                }
            }
            catch (Exception ex)
            {

            }

            return list;
        }

        public async Task<DataTable> CallibrationBypaticipates(string transactionId)
        {
            DataTable dt = new DataTable();


            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("GetCalibrationDetailsByUser", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;

                        cmd.Parameters.AddWithValue("@username", transactionId);

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

        public async Task<DataTable> CallibrationBypaticipatesByUserID()
        {
            DataTable dt = new DataTable();


            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("calibration", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "GetCallibrationByUser");
                        cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName);

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


        public async Task<List<ReviewDataModel>> GetCaoutingExtendedList()
        {
            DataTable dt = new DataTable();
            List<ReviewDataModel> list = new List<ReviewDataModel>();
            string endcuserName = await _enc.EncryptAsync(UserInfo.UserName);
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("Coaching", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "GetCouchingExtended");
                        cmd.Parameters.AddWithValue("@UserName", endcuserName);

                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt));
                    }
                }


                foreach (DataRow row in dt.Rows)
                {
                    ReviewDataModel model = new ReviewDataModel
                    {


                        AgentID = row["AgentID"]?.ToString(),
                        ProcessName = row["ProcessName"]?.ToString(),
                        SubProcess = row["SubProcessName"]?.ToString(),
                        FirstReview = row["1st Review"]?.ToString(),
                        Comment1 = row["Comment 1"]?.ToString(),
                        SecondReview = row["2nd Review"]?.ToString(),
                        Comment2 = row["Comment 2"]?.ToString(),
                        ThirdReview = row["3rd Review"]?.ToString(),
                        Comment3 = row["Comment 3"]?.ToString(),
                        FourthReview = row["4th Review"]?.ToString(),
                        Comment4 = row["Comment 4"]?.ToString(),
                        FifthReview = row["5th Review"]?.ToString(),
                        Comment5 = row["Comment 5"]?.ToString(),
                        SixReview = row["6th Review"]?.ToString(),
                        Comment6 = row["Comment 6"]?.ToString(),
                        CoachingStatus = row["CoachingStatus"]?.ToString()
                    };

                    list.Add(model);
                }
            }
            catch (Exception ex)
            {

            }

            return list;
        }



        public async Task<List<MatrixAllDetails>> GetMatrixList(string AgentID)
        {
            DataTable dt = new DataTable();
            List<MatrixAllDetails> list = new List<MatrixAllDetails>();
            string endcuserName = await _enc.EncryptAsync(UserInfo.UserName);
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("Coaching", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "GeMatrixListAll");
                        cmd.Parameters.AddWithValue("@UserName", endcuserName);
                        cmd.Parameters.AddWithValue("@AgentID", AgentID);
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt));
                    }
                }


                foreach (DataRow row in dt.Rows)
                {
                    MatrixAllDetails model = new MatrixAllDetails
                    {


                        AgentID = row["AgentID"]?.ToString(),
                        Metrics = row["Matrix"]?.ToString(),
                        Target = row["Target"]?.ToString(),
                        Actual_Performance = row["Actual_Performance"]?.ToString(),
                        ReviewDate = row["ReviewDate"]?.ToString(),
                        CreatedBy = row["CreatedBy"]?.ToString(),

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
