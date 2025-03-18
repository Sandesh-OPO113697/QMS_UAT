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

        public async Task<string> GetRecListByAPi( string fromdate  , string todate , string AgentID)
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
                
                        string formattedFromDate = DateTime.ParseExact(fromdate, "MM/dd/yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
                        string formattedToDate = DateTime.ParseExact(todate, "MM/dd/yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");

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

        public async Task<List<SelectListItem>> GetSubDisposition(string Process, string? SubProcess , string Disposition)
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
    }
}
