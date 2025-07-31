using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection.Emit;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Drawing;
using Org.BouncyCastle.Utilities;
using MailKit.Security;
using MimeKit;
using System.Net;
using System.Diagnostics;
using System.Transactions;
using NPOI.SS.Formula.Functions;

namespace QMS.DataBaseService
{
    public class dl_Monitoring
    {

        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly Dl_Admin _admin;
        private readonly DLConnection _dcl;
        public dl_Monitoring(IConfiguration configuration, DL_Encrpt dL_Encrpt, DLConnection dL , Dl_Admin adam)
        {
            _con = configuration.GetConnectionString("Master_Con");
            _enc = dL_Encrpt;
            _dcl = dL;
            _admin = adam;
        }


        public async Task<DataTable> GetMonitorDashboard(SearchDashboard id)
        {
            string query = "usp_GetCallAuditSummary";
            var dataTable = new DataTable();

            using (var connection = new SqlConnection(UserInfo.Dnycon))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ViewType", id.Filter ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ProgramID", id.Program ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@SubProgramID", id.SubProgram ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Select", id.selectedDate ?? (object)DBNull.Value);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        dataTable.Load(reader);
                    }
                }
            }

            return dataTable;
        }
        public async Task<DataSet> GetTransactiondetails(string transactionID)
        {
            DataSet dt = new DataSet();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("GetSectionTransaction", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@transaction", transactionID);

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

        public async Task<DataTable> TestviewDetails(int TestID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("GetAssessmentResultByTestID", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@testID", TestID);

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

        public async Task<DataTable> PendingTestviewDetails(int TestID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("GetPendingAssessmentResultByTestID", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@testID", TestID);

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
        public async Task<DataTable> GetAssesment()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("SP_GetUserAssessmentTests", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);
                        
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
        public async Task<string> SaveVoiceMessage(VoiceMessageModel model)
        {
            try
            {
                // Convert Base64 string to byte array
                byte[] audioBytes = model.AudioData != null ? Convert.FromBase64String(model.AudioData) : null;

                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SaveVoicemassage", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;

                        cmd.Parameters.AddWithValue("@Operations", "CheckvoiceMassage");
                        cmd.Parameters.AddWithValue("@TransactionId", model.TransactionId);
                        cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName);

                        // Ensure audio data is not null before inserting
                        if (audioBytes != null && audioBytes.Length > 0)
                        {
                            cmd.Parameters.Add("@AudioData", SqlDbType.VarBinary, -1).Value = audioBytes;
                        }
                        else
                        {
                            cmd.Parameters.Add("@AudioData", SqlDbType.VarBinary, -1).Value = DBNull.Value;
                        }

                        SqlParameter outputParam = new SqlParameter("@OutputTransactionId", SqlDbType.NVarChar, 50)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputParam);

                        await cmd.ExecuteNonQueryAsync();

                        // Get returned transaction ID
                        string transactionIdFromDB = outputParam.Value?.ToString();

                        return transactionIdFromDB;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        private List<AuditRecord> ProcessLogs(List<AuditEntry> logs)
        {
            List<AuditRecord> records = new List<AuditRecord>();

            for (int i = 0; i < logs.Count; i++)
            {
                if (logs[i].Type == "Start" || logs[i].Type == "Resume")
                {
                    var startTime = logs[i].Time;
                    var pauseEntry = logs.FirstOrDefault(l => l.Type == "Pause" && l.Time > startTime);
                    var endEntry = logs.FirstOrDefault(l => (l.Type == "Pause" || l.Type == "End") && l.Time > startTime);

                    records.Add(new AuditRecord
                    {
                        StartTime = startTime,
                        PauseTime = pauseEntry?.Time ?? startTime,
                        EndTime = endEntry?.Time ?? startTime,
                        IsPaused = endEntry?.Type == "End" ? 0 : 1 
                    });
                }
            }
            return records;
        }
        public async Task<int> InsertAuditPauseLog(AuditPauseLog audit)
        {
            var Transaction_ID = audit.Transaction_ID;
            var ProgramID = audit.ProgramID;
            var SUBProgramID = audit.SUBProgramID;

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync(); // Ensure the connection is opened asynchronously

                   
                    foreach (var entry in ProcessLogs(audit.Logs))
                    {
                       
                            using (SqlCommand cmd = new SqlCommand("InsertPauseLog", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure; // Specify stored procedure

                                cmd.Parameters.AddWithValue("@TransactionID", Transaction_ID);
                                cmd.Parameters.AddWithValue("@ProgramID", Convert.ToInt32(ProgramID));
                                cmd.Parameters.AddWithValue("@SubProgramID", Convert.ToInt32(SUBProgramID));
                                cmd.Parameters.AddWithValue("@Starttime", entry.StartTime) ;
                                cmd.Parameters.AddWithValue("@Endtime", entry.EndTime);
                                cmd.Parameters.AddWithValue("@ActualPauseTime", entry.PauseTime);

                                cmd.Parameters.AddWithValue("@IsPause", Convert.ToInt32(entry.IsPaused));
                                cmd.Parameters.AddWithValue("@createdBy", UserInfo.UserName);

                                await cmd.ExecuteNonQueryAsync();
                            }
                        
                    }

                   
                }

                return 1; // Success
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting audit log: {ex.Message}");
                return 0; // Failure
            }
        }


        public async Task<int> SubmitePridictiveEvaluation(List<PredictiveEvaluationModel> model)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    foreach (var section in model)
                    {
                        using (SqlCommand cmd = new SqlCommand("InsertPridiciveEvaluation", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Transaction_Id", section.Transaction_ID);
                            cmd.Parameters.AddWithValue("@ProgramID", Convert.ToInt32(section.ProgramID));
                            cmd.Parameters.AddWithValue("@SubProgramID", Convert.ToInt32(section.SUBProgramID));

                            cmd.Parameters.AddWithValue("@Predictive_CSAT", section.PredictiveCSAT);
                            cmd.Parameters.AddWithValue("@Predictive_NPS", section.PredictiveNPS);
                            cmd.Parameters.AddWithValue("@Predictive_FCR", section.PredictiveFCR);
                            cmd.Parameters.AddWithValue("@Predictive_Repeat", section.PredictiveRepeat ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Predictive_Sales_effort", section.PredictiveSalesEffort);
                            cmd.Parameters.AddWithValue("@Predictive_Collectioneffort", section.PredictiveCollectionEffort);
                            cmd.Parameters.AddWithValue("@PredictiveProbableescalation", section.PredictiveEscalation);
                            cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName);


                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 0; // Return 0 on failure
            }
        }


        public async Task<int> SubmiteRouteCauseEvaluation(List<RootCauseAnalysisModel> model)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    foreach (var section in model)
                    {
                        using (SqlCommand cmd = new SqlCommand("InsertrouteEvaluation", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Transaction_Id", section.Transaction_ID);
                            cmd.Parameters.AddWithValue("@ProgramID", Convert.ToInt32(section.ProgramID));
                            cmd.Parameters.AddWithValue("@SubProgramID", Convert.ToInt32(section.SUBProgramID));
                         
                            cmd.Parameters.AddWithValue("@MetricRCA", section.metricRCA);
                            cmd.Parameters.AddWithValue("@Controllable", section.controllable);
                            cmd.Parameters.AddWithValue("@RCA1", section.rca1);
                            cmd.Parameters.AddWithValue("@RCA2", section.rca2 ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@RCA3", section.rca3);
                            cmd.Parameters.AddWithValue("@CommentsSection", section.comments);
                            cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName);
                    

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 0; // Return 0 on failure
            }
        }
        public async Task<int> SubmiteSectionEvaluation(List<SectionAuditModel> model)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    foreach (var section in model)
                    {
                        using (SqlCommand cmd = new SqlCommand("InsertSectionEvaluation", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Category", section.category);
                            cmd.Parameters.AddWithValue("@Level", section.level);
                            cmd.Parameters.AddWithValue("@SectionName", section.sectionName);
                            cmd.Parameters.AddWithValue("@QA_rating", section.qaRating);
                            cmd.Parameters.AddWithValue("@Scorable", section.scorable);
                            cmd.Parameters.AddWithValue("@parameters", section.parameters);
                            cmd.Parameters.AddWithValue("@subparameters", section.subparameters);
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
                return 1; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 0; // Return 0 on failure
            }
        }

        public async Task<int> SubmiteFormEvaluation(MonitorFormModel model)
        {
           
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SubmiteMonitoring", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operations", "CallAudit_Master");
                        cmd.Parameters.AddWithValue("@Audit_Type", model.AuditID);
                        cmd.Parameters.AddWithValue("@Program_id", Convert.ToInt32( model.ProgramID));
                        cmd.Parameters.AddWithValue("@SubProgram_id", Convert.ToInt32( model.SUBProgramID));
                        cmd.Parameters.AddWithValue("@TransactionID", model.Transaction_ID);
                        cmd.Parameters.AddWithValue("@Category", model.dispositionId);
                        cmd.Parameters.AddWithValue("@Subcategory", model.SubDispositionID);
                        cmd.Parameters.AddWithValue("@Cat1", model.Cat1);
                        cmd.Parameters.AddWithValue("@Cat2", model.Cat2);
                        cmd.Parameters.AddWithValue("@Cat3", model.Cat3);
                        cmd.Parameters.AddWithValue("@Cat4", model.Cat4);
                        cmd.Parameters.AddWithValue("@Cat5", model.Cat5);
                        cmd.Parameters.AddWithValue("@MonitorID",0);
                        cmd.Parameters.AddWithValue("@CQ_Score", model.CQ_Scrore);
                        cmd.Parameters.AddWithValue("@Agent_Name", model.AgentID);
                        cmd.Parameters.AddWithValue("@AgentID",model.AgentID);
                        cmd.Parameters.AddWithValue("@TLName", model.TL_id);
                        cmd.Parameters.AddWithValue("@TL_ID", model.TL_id);
                        cmd.Parameters.AddWithValue("@MonitorDate", Convert.ToDateTime( model.Monitored_date));
                        cmd.Parameters.AddWithValue("@TransactionDate", Convert.ToDateTime(model.Transaction_Date));
                        cmd.Parameters.AddWithValue("@Year", model.Year);
                        cmd.Parameters.AddWithValue("@month", model.Month);
                        cmd.Parameters.AddWithValue("@ZTClassification", model.ZTClassification);
                        cmd.Parameters.AddWithValue("@ZeroToleranceBehaviour", model.ZeroTolerance);
                        cmd.Parameters.AddWithValue("@week", model.Week);
                        cmd.Parameters.AddWithValue("@Remarks", model.Remarks);
                        cmd.Parameters.AddWithValue("@InsertDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName);
                        await    cmd.ExecuteNonQueryAsync();


                       

                       

                    }

                    using (SqlCommand cmd = new SqlCommand("AgentSurvey", conn))
                    {
                        DataTable dt2 = new DataTable();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Mode", "GetAgentmasterEmailID");
                        cmd.Parameters.AddWithValue("@UserName", model.AgentID);
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt2));

                        string Email = dt2.Rows[0]["EmailID"].ToString();

                        string AgentName  = dt2.Rows[0]["EmpCode"].ToString();

                        await SendEmailAsync(Email,AgentName, model.Transaction_ID, model.AuditID, model.ProgramID, UserInfo.UserName, model.CQ_Scrore,model.Remarks);

                        return 1;
                    }
                }

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<bool> SendEmailAsync(string recipientEmails, string AgentName, string Transaction_ID, string AuditID, string ProgramID, string UserName, string CQ_Scrore, string Remarks)
        {
            String Audit_Type = "";
            string Process = "";
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("GetMailDetails", conn))
                {
                    DataTable dt2 = new DataTable();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@Operation", "GetAuditIDType");
                    cmd.Parameters.AddWithValue("@ID", AuditID);
                    SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                    await Task.Run(() => adpt.Fill(dt2));

                    Audit_Type = dt2.Rows[0]["Audit_Type"].ToString();
                }

              
            }

            DataTable dt = await _admin.GetProcessListAsync();


            var processList = dt.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            var processNameItem = processList
      .FirstOrDefault(x => x.Value == ProgramID.ToString());

            var processName = processNameItem?.Text;

            string mailHost = "192.168.0.122";
            int mailPort = 587;
            string mailUserId = "reports@1point1.in";
            string mailPassword = "Pass@1234";
            string mailFrom = "reports@1point1.in";
            string Massage = "One of your recent customer interactions has been audited by the Quality Team.";
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("QMS_EMAIL", mailFrom));

                // Add recipient
                emailMessage.To.Add(new MailboxAddress("", recipientEmails));

                emailMessage.Subject = "Monitor audits the transaction";
                var bodyBuilder = new BodyBuilder
                {
                     TextBody = $@"
                           Dear {AgentName},
                           
                           One of your recent customer interactions has been audited by the Quality Team.
                           Details:-
                           Audited Transaction ID:    {Transaction_ID}
                           Interaction Type:          {Audit_Type}
                           Process:                   {processName}
                           Auditor:                   {UserName}
                           Audit Score:               {CQ_Scrore}
                           
                           Overall Comments:
                           {Remarks}
                           
                           Regards,  
                           Your Quality Team"
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

        public async Task<DataSet> GetRCAVAluesDroppdawn()
        {
            DataSet dt = new DataSet();


          
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("MonitoringDetails", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operations","GetRCAValues");
                     
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
        public async Task<string> GetRecListByAPi(string fromdate, string todate, string AgentID)
        {
            string responseBody = string.Empty;
            string Account = UserInfo.AccountID;
            string con = await _enc.DecryptAsync(_con);


            string RecAPiList = string.Empty;
            string StoreProcedure = "GetRecordingApi";
            try
            {
                using (var connection = new SqlConnection(con))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Operations", "GetRecListAPi");
                        command.Parameters.AddWithValue("@Account", Account);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                RecAPiList = reader["RecApiList"].ToString();

                            }
                        }
                    }
                }


                if (!string.IsNullOrEmpty(RecAPiList))
                {

                    string formattedFromDate = DateTime.ParseExact(fromdate, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                                  .ToString("yyyyMMdd");

                    string formattedToDate = DateTime.ParseExact(todate, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                                                    .ToString("yyyyMMdd");
                    var requestBody = new
                    {
                        agentID = AgentID,
                        fromDate = formattedFromDate,
                        todate = formattedToDate
                    };

                    string jsonPayload = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    using (HttpClient httpClient = new HttpClient())
                    {
                        HttpResponseMessage response = await httpClient.PostAsync(RecAPiList, content);
                        responseBody = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                        }
                        else
                        {
                            throw new Exception($"API Error: {response.StatusCode}, Message: {responseBody}");
                        }
                    }


                }
                else
                {
                    Console.WriteLine("API URL is empty!");
                }

                return responseBody;
            }
            catch (Exception ex)
            {
                return ex.Message;

            }
        }


        public async Task<string> GetPauseLimitByProgram(string ProgramID, string SubProgramID)
        {
            string PauseLimit = string.Empty;
           
            string StoreProcedure = "MonitoringDetails";
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Operations", "GetPauseLimit");
                        command.Parameters.AddWithValue("@ProgramID", ProgramID);
                        command.Parameters.AddWithValue("@SUBProgramID", SubProgramID);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                PauseLimit = reader["Number_Of_Pause"].ToString();

                            }
                        }
                    }
                }


               

                return PauseLimit;
            }
            catch (Exception ex)
            {
                return ex.Message;

            }
        }

        public async Task<List<SelectListItem>> GetDisposition(string Process, string? SubProcess)
        {
            var Adudit = new List<SelectListItem>();
            string StoreProcedure = "MonitoringDetails";
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Operations", "GetDispositionList");
                        command.Parameters.AddWithValue("@Process", Process);
                        command.Parameters.AddWithValue("@SubProcess", SubProcess);
                        using (var reder = await command.ExecuteReaderAsync())
                        {
                            while (await reder.ReadAsync())
                            {
                                string TText = reder["Disposition"].ToString();
                                string TValue = reder["DispositionCode"].ToString();
                                Adudit.Add(new SelectListItem
                                {
                                    Text = TText,
                                    Value = TValue
                                });

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Adudit;

        }
        public async Task<List<SelectListItem>> GetCat1(string Process, string? SubProcess)
        {
            var Adudit = new List<SelectListItem>();
            string StoreProcedure = "MonitoringDetails";
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Operations", "Getcat1");
                        command.Parameters.AddWithValue("@Process", Process);
                        command.Parameters.AddWithValue("@SubProcess", SubProcess);
                        using (var reder = await command.ExecuteReaderAsync())
                        {
                            while (await reder.ReadAsync())
                            {
                                string TText = reder["Category1"].ToString();
                                string TValue = reder["Category1"].ToString();
                                Adudit.Add(new SelectListItem
                                {
                                    Text = TText,
                                    Value = TValue
                                });

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Adudit;

        }
        public async Task<List<SelectListItem>> GetCat2(string Process, string? SubProcess)
        {
            var Adudit = new List<SelectListItem>();
            string StoreProcedure = "MonitoringDetails";
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Operations", "Getcat2");
                        command.Parameters.AddWithValue("@Process", Process);
                        command.Parameters.AddWithValue("@SubProcess", SubProcess);
                        using (var reder = await command.ExecuteReaderAsync())
                        {
                            while (await reder.ReadAsync())
                            {
                                string TText = reder["Category2"].ToString();
                                string TValue = reder["Category2"].ToString();
                                Adudit.Add(new SelectListItem
                                {
                                    Text = TText,
                                    Value = TValue
                                });

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Adudit;

        }
        public async Task<List<SelectListItem>> GetCat3(string Process, string? SubProcess)
        {
            var Adudit = new List<SelectListItem>();
            string StoreProcedure = "MonitoringDetails";
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Operations", "Getcat3");
                        command.Parameters.AddWithValue("@Process", Process);
                        command.Parameters.AddWithValue("@SubProcess", SubProcess);
                        using (var reder = await command.ExecuteReaderAsync())
                        {
                            while (await reder.ReadAsync())
                            {
                                string TText = reder["Category3"].ToString();
                                string TValue = reder["Category3"].ToString();
                                Adudit.Add(new SelectListItem
                                {
                                    Text = TText,
                                    Value = TValue
                                });

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Adudit;

        }
        public async Task<List<SelectListItem>> GetCat4(string Process, string? SubProcess)
        {
            var Adudit = new List<SelectListItem>();
            string StoreProcedure = "MonitoringDetails";
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Operations", "Getcat4");
                        command.Parameters.AddWithValue("@Process", Process);
                        command.Parameters.AddWithValue("@SubProcess", SubProcess);
                        using (var reder = await command.ExecuteReaderAsync())
                        {
                            while (await reder.ReadAsync())
                            {
                                string TText = reder["Category4"].ToString();
                                string TValue = reder["Category4"].ToString();
                                Adudit.Add(new SelectListItem
                                {
                                    Text = TText,
                                    Value = TValue
                                });

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Adudit;

        }
        public async Task<List<SelectListItem>> GetCat5(string Process, string? SubProcess)
        {
            var Adudit = new List<SelectListItem>();
            string StoreProcedure = "MonitoringDetails";
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Operations", "Getcat5");
                        command.Parameters.AddWithValue("@Process", Process);
                        command.Parameters.AddWithValue("@SubProcess", SubProcess);
                        using (var reder = await command.ExecuteReaderAsync())
                        {
                            while (await reder.ReadAsync())
                            {
                                string TText = reder["Category5"].ToString();
                                string TValue = reder["Category5"].ToString();
                                Adudit.Add(new SelectListItem
                                {
                                    Text = TText,
                                    Value = TValue
                                });

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Adudit;

        }
        public async Task<string> GetcategoryorDispo(string Process, string? SubProcess)
        {
            string TText = "";
            string StoreProcedure = "MonitoringDetails";
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Operations", "GetTypeOfDispo");
                        command.Parameters.AddWithValue("@Process", Process);
                        command.Parameters.AddWithValue("@SubProcess", SubProcess);
                        using (var reder = await command.ExecuteReaderAsync())
                        {
                            while (await reder.ReadAsync())
                            {
                                TText = reder["type"].ToString();


                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return TText;
            return "";
        }

        public async Task<string> GetProvcessType(string Process, string? SubProcess)
        {
            string TText = "";
            string StoreProcedure = "MonitoringDetails";
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Operations", "GetProcesstype");
                        command.Parameters.AddWithValue("@Process", Process);
                        command.Parameters.AddWithValue("@SubProcess", SubProcess);
                        using (var reder = await command.ExecuteReaderAsync())
                        {
                            while (await reder.ReadAsync())
                            {
                                 TText = reder["type"].ToString();
                                

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return TText;

        }

        public async Task<List<SelectListItem>> GetSubDisposition(string Process, string? SubProcess, string Disposition)
        {
            var Adudit = new List<SelectListItem>();
            string StoreProcedure = "MonitoringDetails";
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Operations", "GetSubDispositionList");
                        command.Parameters.AddWithValue("@Process", Process);
                        command.Parameters.AddWithValue("@SubProcess", SubProcess);
                        command.Parameters.AddWithValue("@DispositionCode", Disposition);
                        using (var reder = await command.ExecuteReaderAsync())
                        {
                            while (await reder.ReadAsync())
                            {
                                string TText = reder["SubDisposition"].ToString();
                                string TValue = reder["SubDispositionCode"].ToString();
                                Adudit.Add(new SelectListItem
                                {
                                    Text = TText,
                                    Value = TValue
                                });

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Adudit;

        }


        public async Task<List<SelectListItem>> GetAgentName(string Process, string? SubProcess)
        {
            var Adudit = new List<SelectListItem>();
            string StoreProcedure = "MonitoringDetails";
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Operations", "GetAgentName");
                        command.Parameters.AddWithValue("@Process", Process);
                        command.Parameters.AddWithValue("@SubProcess", SubProcess);
                        using (var reder = await command.ExecuteReaderAsync())
                        {
                            while (await reder.ReadAsync())
                            {
                                string TText = reder["EmpName"].ToString();
                                string TValue = reder["EmpCode"].ToString();
                                Adudit.Add(new SelectListItem
                                {
                                    Text = TText,
                                    Value = TValue
                                });

                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return Adudit;
        }
        public async Task<string> GetTeamLeaderName(string EmpCode)
        {
            string TL_Name = string.Empty;
            string StoreProcedure = "MonitoringDetails";
            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Operations", "GetTLName");
                        command.Parameters.AddWithValue("@EmpCode", EmpCode);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                TL_Name = reader["TL_Name"].ToString();

                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return TL_Name;
        }

        public async Task<string> CheckAuditByTransactionDone(string connid)
        {
            string TreancationID = string.Empty;
            string StoreProcedure = "MonitoringDetails";

            using (var connection = new SqlConnection(UserInfo.Dnycon))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(StoreProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Operations", "CheckAuditIsDone");
                    command.Parameters.AddWithValue("@TranactionID", connid);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            TreancationID = reader["TransactionID"].ToString();

                        }
                    }
                }
            }
            return TreancationID;
        }

        public async Task<string> GetRecordingByConnID(string connid)
        {
            try
            {
                string responseBody = string.Empty;
                string Account = UserInfo.AccountID;
                string con = await _enc.DecryptAsync(_con);


                string RecAPiConnID = string.Empty;
                string StoreProcedure = "GetRecordingApi";

                using (var connection = new SqlConnection(con))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(StoreProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Operations", "GetRecConnIDAPi");
                        command.Parameters.AddWithValue("@Account", Account);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                RecAPiConnID = reader["RecApiConnID"].ToString();

                            }
                        }
                    }
                }


                if (!string.IsNullOrEmpty(RecAPiConnID))
                {
                    using (HttpClient client = new HttpClient())
                    {

                        var requestBody = new
                        {
                            transactionId = connid
                        };

                        string jsonContent = JsonConvert.SerializeObject(requestBody);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        // Make POST request
                        HttpResponseMessage response = await client.PostAsync(RecAPiConnID, content);

                        if (response.IsSuccessStatusCode)
                        {
                            responseBody = await response.Content.ReadAsStringAsync();

                        }
                        else
                        {
                            return $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
                        }
                    }
                }
                return responseBody;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }


        }
    }
}
