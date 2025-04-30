using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using NPOI.HSSF.Record;
using NPOI.POIFS.Crypt.Dsig;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace QMS.DataBaseService
{
    public class dl_Calibration
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dlcon;
        private readonly HttpResponse response;
  

        public dl_Calibration(DL_Encrpt dL_Encrpt , IConfiguration configuration)
        {
            _con = configuration.GetConnectionString("Master_Con");
            _enc = dL_Encrpt;
        }
        public async Task<List<SelectListItem>> GetRecListByAPi(string fromdate, string todate, string AgentID)
        {
            string responseBody = string.Empty;
            string Account = UserInfo.AccountID;
            string con = await _enc.DecryptAsync(_con);
            List<SelectListItem> processList = new List<SelectListItem>(); // ✅ Defined here


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
                        string data = responseBody;
                        var callRecords = JsonConvert.DeserializeObject<List<CallRecord>>(data);

                        if (callRecords != null)
                        {
                            using (var connection = new SqlConnection(UserInfo.Dnycon))
                            {
                                await connection.OpenAsync();

                                foreach (var record in callRecords)
                                {
                                    using (var cmd = new SqlCommand("CheckConnIdExists", connection))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.AddWithValue("@ConnId", record.CONNID);

                                        var outputParam = new SqlParameter("@Exists", SqlDbType.Bit)
                                        {
                                            Direction = ParameterDirection.Output
                                        };
                                        cmd.Parameters.Add(outputParam);

                                        await cmd.ExecuteNonQueryAsync();

                                        bool exists = (bool)outputParam.Value;
                                        if (!exists)
                                        {
                                            processList.Add(new SelectListItem
                                            {
                                                Value = record.CONNID,
                                                Text = record.CONNID
                                            });
                                        }
                                    }
                                }
                            }
                        }


                    }


                }
                else
                {
                    Console.WriteLine("API URL is empty!");
                }

                return processList;
               

            }
            catch (Exception ex)
            {
                return null;

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
                        using (SqlCommand cmd = new SqlCommand("InsertSectionCouching_Details", conn))
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
                return 0; 
            }
        }

        public async Task SubmiteCalibrationDetails(string programID, string SubProgramID , string TransactionID ,string Comment ,  List<string> Participants)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    string FeatureNameQuery = "InsertCalibrationDetails";

                    foreach (string participant in Participants)
                    {
                        using (SqlCommand cmd = new SqlCommand(FeatureNameQuery, con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                      
                            cmd.Parameters.AddWithValue("@ProgramID ", programID);
                            cmd.Parameters.AddWithValue("@SubProgramID ", SubProgramID);
                            cmd.Parameters.AddWithValue("@Comment ", Comment);

                            cmd.Parameters.AddWithValue("@TransactionID ", TransactionID);
                            cmd.Parameters.AddWithValue("@Participants ", participant);
                            cmd.Parameters.AddWithValue("@CreatedBy ", UserInfo.UserName);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }
           
        }

    }
}
