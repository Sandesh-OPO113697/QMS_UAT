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

namespace QMS.DataBaseService
{
    public class dl_Monitoring
    {

        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dcl;
        public dl_Monitoring(IConfiguration configuration, DL_Encrpt dL_Encrpt, DLConnection dL)
        {
            _con = configuration.GetConnectionString("Master_Con");
            _enc = dL_Encrpt;
            _dcl = dL;
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
                            cmd.Parameters.AddWithValue("@Weightage", section.score);
                            cmd.Parameters.AddWithValue("@Commentssection", section.comments ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@TransactionID", section.Transaction_ID);
                            cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName);
                            cmd.Parameters.AddWithValue("@ProgramID", Convert.ToInt32(section.ProgramID));
                            cmd.Parameters.AddWithValue("@SubProgramID", Convert.ToInt32(section.SUBProgramID));

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
                        cmd.Parameters.AddWithValue("@MonitorID",0);
                        cmd.Parameters.AddWithValue("@CQ_Score", model.CQ_Scrore);
                        cmd.Parameters.AddWithValue("@Agent_Name", model.AgentID);
                        cmd.Parameters.AddWithValue("@AgentID", Convert.ToInt32(model.AgentID));
                        cmd.Parameters.AddWithValue("@TLName", model.TL_id);
                        cmd.Parameters.AddWithValue("@TL_ID", model.TL_id);
                        cmd.Parameters.AddWithValue("@MonitorDate", Convert.ToDateTime( model.Monitored_date));
                        cmd.Parameters.AddWithValue("@TransactionDate", Convert.ToDateTime(model.Transaction_Date));
                        cmd.Parameters.AddWithValue("@Year", model.Year);
                        cmd.Parameters.AddWithValue("@month", model.Month);
                        cmd.Parameters.AddWithValue("@ZTClassification", model.ZTClassification);
                        cmd.Parameters.AddWithValue("@ZeroToleranceBehaviour", model.ZeroTolerance);
                        cmd.Parameters.AddWithValue("@week", model.Week);
                        cmd.Parameters.AddWithValue("@InsertDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName);
                        await    cmd.ExecuteNonQueryAsync();
                        return 1;

                    }
                }

            }
            catch (Exception ex)
            {
                return 0;
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
