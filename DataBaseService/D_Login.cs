using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QMS.Encription;
using QMS.Models;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Net.Mail;
using System.Net;
using System.Text;
using MimeKit;
using MailKit.Security;
using System.Security.Authentication;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using AuthenticationException = MailKit.Security.AuthenticationException;
using System.Drawing.Imaging;


namespace QMS.DataBaseService
{
    public class D_Login
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dlcon;
        private readonly HttpResponse response;
        public D_Login(IConfiguration configuration, DL_Encrpt dL_Encrpt, DLConnection conn , IHttpContextAccessor httpContextAccessor)
        {
            _con = configuration.GetConnectionString("Master_Con");
            _enc = dL_Encrpt;
            _dlcon = conn;
            this.response = httpContextAccessor.HttpContext?.Response;

        }
        public async Task<int> SaveEmailPhoneOTP(string emailOTP, string phoneOTP, string email, string phone, string username)
        {
            string Dycon = await _dlcon.GetDynStrByUserIDAsync(username);
            int isValid = 0;
            using (SqlConnection connection = new SqlConnection(Dycon))
            {
                await connection.OpenAsync(); 

                using (SqlCommand command = new SqlCommand("SaveOTPAndValidate", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@Operation", "CheckEmailValid");

                    command.Parameters.AddWithValue("@EmailOTP", emailOTP);
                    command.Parameters.AddWithValue("@PhoneOTP", phoneOTP);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Phone", phone);
                    command.Parameters.AddWithValue("@Username", await _enc.EncryptAsync(username));
                    object result = await command.ExecuteScalarAsync();

                    isValid = result != null ? Convert.ToInt32(result) : 0;
                }
            }

            return isValid ;
        }


        public async Task<int> varifyEmailPhoneOTP(string emailOTP, string phoneOTP, string username)
        {
            string Dycon = await _dlcon.GetDynStrByUserIDAsync(username);
            int isValid = 0;

            string usernametest = await _enc.EncryptAsync(username);
            using (SqlConnection connection = new SqlConnection(Dycon))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("SaveOTPAndValidate", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@Operation", "VarifyOTP");

                    command.Parameters.AddWithValue("@EmailOTP", emailOTP);
                    command.Parameters.AddWithValue("@PhoneOTP", phoneOTP);
           
                    command.Parameters.AddWithValue("@Username", usernametest);
                    object result = await command.ExecuteScalarAsync();

                    isValid = result != null ? Convert.ToInt32(result) : 0;
                }
            }

            return isValid;
        }

        public async Task SendEmail(string recipientEmail, string OTP)
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
                emailMessage.To.Add(new MailboxAddress("", recipientEmail));
                emailMessage.Subject = $"OTP: {OTP}";
                var bodyBuilder = new BodyBuilder
                {
                    TextBody = $"Dear User,\n\nYour OTP is: {OTP}\n\nRegards,\nApplication Team"
                };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                client.Connect(mailHost, mailPort, SecureSocketOptions.StartTls); 
                client.Authenticate(mailUserId, mailPassword);
                client.Send(emailMessage);
                client.Disconnect(true);

                Console.WriteLine("OTP email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send OTP email: {ex.Message}");
            }
        }
        public async Task<string> SendOTPAsync(string OTP, string Mobile)
        {
            string Message = $"Yours OTP is {OTP}. Thank you for contacting OPO";

            string sourceAddress = "OPOSMS";
            string messageType = "SERVICE_IMPLICIT";
            string dltTemplateId = "1607100000000131768";
            string entityId = "1601100000000007805";
            string isEnabled = "true";

            string json = $"{{\"customerId\": \"{"ONE_POINT__Hsxp4Bd95x41AyX0iYU6"}\"," +
                          $"\"destinationAddress\":[\"{Mobile}\"]," +
                          $"\"message\": \"{Message}\"," +
                          $"\"sourceAddress\": \"{sourceAddress}\"," +
                          $"\"messageType\": \"{messageType}\"," +
                          $"\"dltTemplateId\": \"{dltTemplateId}\"," +
                          $"\"urlShortenerParams\":{{\"isEnabled\":\"{isEnabled}\"}}," +
                          $"\"entityId\":\"{entityId}\"" +
                          "}}";

            string APIurl = "https://openapi.airtel.in/gateway/airtel-iq-sms-utility/sendSms";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Basic T05FX1BPSU5UX19Ic3hwNEJkOTV4NDFBeVgwaVlVNjpGbUE4RkhWM0BCd0R1JjZr");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                try
                {
                    HttpResponseMessage response = await client.PostAsync(APIurl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Message = "Success";
                    }
                    else
                    {
                        Message = $"Failed...! {response.StatusCode}";
                    }
                }
                catch (Exception ex)
                {
                    Message = $"Failed...! {ex.Message}";
                }
            }

            return Message;
        }
        public async Task<int> CheckUserIsValidAsync(string UserID, string Password)
        {
            string Dycon = await _dlcon.GetDynStrByUserIDAsync(UserID);
            string encUser = await _enc.EncryptAsync(UserID);
            string encPassword = await _enc.EncryptAsync(Password);
            int isValid = 0;
            using (SqlConnection connection = new SqlConnection(Dycon))
            {
                using (SqlCommand command = new SqlCommand("CheckUserValid", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@UserName", encUser);
                    command.Parameters.AddWithValue("@Operation", "CheckUserValiesOrNot");

                    SqlParameter outputParam = new SqlParameter
                    {
                        ParameterName = "@IsValid",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputParam);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    isValid = (int)outputParam.Value;
                }
            }

            if (isValid == 1)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(Dycon))
                    {
                        using (SqlCommand command = new SqlCommand("CheckUserValid", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@UserName", encUser);
                            command.Parameters.AddWithValue("@Password", encPassword);
                            command.Parameters.AddWithValue("@Operation", "ResetPassword");

                            SqlParameter outputParam2 = new SqlParameter
                            {
                                ParameterName = "@IsValid",
                                SqlDbType = SqlDbType.Int,
                                Direction = ParameterDirection.Output
                            };
                            command.Parameters.Add(outputParam2);

                            await connection.OpenAsync();
                            await command.ExecuteNonQueryAsync();


                            int resetResult = (int)outputParam2.Value;

                            if (resetResult == 1)
                            {
                                return 1;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return 0;
                }

            }
            else
            {
                return 0;
            }
        }

        public async Task AssignRoleToUser(string UserID, HttpContext httpContext)
        {
            string Dycon = await _dlcon.GetDynStrByUserIDAsync(UserID);
            string query = "sp_Superadmin";
            DataTable dt = new DataTable();
            var roleFeatureDictionary = new Dictionary<int, Dictionary<int, Dictionary<int, string>>>();
            var roleModules = new Dictionary<int, List<object>>();

            try
            {
                using (SqlConnection connection = new SqlConnection(Dycon))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@mode", "GetFetureAndSubFeature");
                        command.Parameters.AddWithValue("@EmpID", UserID);
                        await connection.OpenAsync();
                        SqlDataAdapter adpt = new SqlDataAdapter(command);
                        await Task.Run(() => adpt.Fill(dt));
                    }
                }

                var roleNames = new Dictionary<int, string>();
                var featureNames = new Dictionary<int, string>();

             
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        int roleId = row["Role_id"] == DBNull.Value ? 0 : Convert.ToInt32(row["Role_id"]);
                        int featureId = row["Feature_id"] == DBNull.Value ? 0 : Convert.ToInt32(row["Feature_id"]);
                        int subFeatureId = row["SubFeature_ID"] == DBNull.Value ? 0 : Convert.ToInt32(row["SubFeature_ID"]);
                        string subFeatureName = row["SubFeatureName"] == DBNull.Value ? string.Empty : row["SubFeatureName"].ToString();

                        if (!roleNames.ContainsKey(roleId))
                            roleNames[roleId] = row["RoleName"] == DBNull.Value ? string.Empty : row["RoleName"].ToString();

                        if (!featureNames.ContainsKey(featureId))
                            featureNames[featureId] = row["FeatureName"] == DBNull.Value ? string.Empty : row["FeatureName"].ToString();

                        if (!roleFeatureDictionary.ContainsKey(roleId))
                            roleFeatureDictionary[roleId] = new Dictionary<int, Dictionary<int, string>>();

                        if (!roleFeatureDictionary[roleId].ContainsKey(featureId))
                            roleFeatureDictionary[roleId][featureId] = new Dictionary<int, string>();

                        roleFeatureDictionary[roleId][featureId][subFeatureId] = subFeatureName;

                        int moduleId = row["Module_id"] == DBNull.Value ? 0 : Convert.ToInt32(row["Module_id"]);
                        string moduleName = row["Module_Name"] == DBNull.Value ? string.Empty : row["Module_Name"].ToString();

                        if (!roleModules.ContainsKey(roleId))
                            roleModules[roleId] = new List<object>();

                        roleModules[roleId].Add(new { ModuleId = moduleId, ModuleName = moduleName });
                    }

                }
                else
                {
                    using (SqlConnection connection = new SqlConnection(Dycon))
                    {
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@mode", "GetMauduleDefault");
                            command.Parameters.AddWithValue("@EmpID", UserID);
                            await connection.OpenAsync();
                            SqlDataAdapter adpt = new SqlDataAdapter(command);
                            await Task.Run(() => adpt.Fill(dt));
                        }
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        int roleId = row["Role_id"] == DBNull.Value ? 0 : Convert.ToInt32(row["Role_id"]);
                        int featureId = row["Feature_id"] == DBNull.Value ? 0 : Convert.ToInt32(row["Feature_id"]);
                        int subFeatureId = row["SubFeature_ID"] == DBNull.Value ? 0 : Convert.ToInt32(row["SubFeature_ID"]);
                        string subFeatureName = row["SubFeatureName"] == DBNull.Value ? string.Empty : row["SubFeatureName"].ToString();

                        if (!roleNames.ContainsKey(roleId))
                            roleNames[roleId] = row["RoleName"] == DBNull.Value ? string.Empty : row["RoleName"].ToString();

                        if (!featureNames.ContainsKey(featureId))
                            featureNames[featureId] = row["FeatureName"] == DBNull.Value ? string.Empty : row["FeatureName"].ToString();

                        if (!roleFeatureDictionary.ContainsKey(roleId))
                            roleFeatureDictionary[roleId] = new Dictionary<int, Dictionary<int, string>>();

                        if (!roleFeatureDictionary[roleId].ContainsKey(featureId))
                            roleFeatureDictionary[roleId][featureId] = new Dictionary<int, string>();

                        roleFeatureDictionary[roleId][featureId][subFeatureId] = subFeatureName;

                        int moduleId = row["Module_id"] == DBNull.Value ? 0 : Convert.ToInt32(row["Module_id"]);
                        string moduleName = row["Module_Name"] == DBNull.Value ? string.Empty : row["Module_Name"].ToString();

                        if (!roleModules.ContainsKey(roleId))
                            roleModules[roleId] = new List<object>();

                        roleModules[roleId].Add(new { ModuleId = moduleId, ModuleName = moduleName });
                    }


                }

                var finalStructure = new List<object>();

                foreach (var role in roleFeatureDictionary)
                {
                    var roleInfo = new
                    {
                        RoleId = role.Key,
                        RoleName = roleNames.ContainsKey(role.Key) ? roleNames[role.Key] : "Unknown Role",
                        Modules = roleModules.ContainsKey(role.Key) ? roleModules[role.Key] : new List<object>(),
                        Features = new List<object>()
                    };

                    foreach (var feature in role.Value)
                    {
                        var featureInfo = new
                        {
                            FeatureId = feature.Key,
                            FeatureName = featureNames.ContainsKey(feature.Key) ? featureNames[feature.Key] : "Unknown Feature",
                            SubFeatures = feature.Value.Select(subFeature => new
                            {
                                SubFeatureId = subFeature.Key,
                                SubFeatureName = subFeature.Value
                            }).ToList()
                        };

                        ((List<object>)roleInfo.Features).Add(featureInfo);
                    }

                    finalStructure.Add(roleInfo);
                }

                string serializedData = JsonConvert.SerializeObject(finalStructure);
                var roles = JsonConvert.DeserializeObject<List<Role>>(serializedData);
                var distinctModules = roles
                    .SelectMany(r => r.Modules.Select(m => new
                    {
                        ModuleId = m.ContainsKey("ModuleId") ? m["ModuleId"] : null,
                        ModuleName = m.ContainsKey("ModuleName") ? m["ModuleName"] : null,
                        RoleId = r.RoleId,
                        RoleName = r.RoleName
                    }))
                    .Distinct()
                    .ToList();
                string serialiModule = JsonConvert.SerializeObject(distinctModules);
                httpContext.Session.SetString("ModuleFeatures", serialiModule);

                httpContext.Session.SetString("RoleFeatures", serializedData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public async Task<int> CheckAccountUserAsync(string UserID, string Password)
        {
            string reset = "";
            string Dycon = await _dlcon.GetDynStrByUserIDAsync(UserID);
            try
            {
                string User = await _enc.EncryptAsync(UserID);
                string passeord = await _enc.EncryptAsync(Password);
                using (SqlConnection cc = new SqlConnection(Dycon))
                {
                    await cc.OpenAsync();
                    DataTable dt = new DataTable();
                    SqlCommand cmd = new SqlCommand("Sp_Check_LogIn", cc);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Empcode", User);
                    cmd.Parameters.AddWithValue("@Password", passeord);
                    SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                    await Task.Run(() => adpt.Fill(dt));

                    if (dt.Rows.Count > 0)
                    {
                        UserInfo.UserName = await _enc.DecryptAsync(dt.Rows[0]["Name"].ToString());
                        UserInfo.UserType = dt.Rows[0]["usertype"].ToString();
                        UserInfo.IsActive = dt.Rows[0]["isactive"].ToString();
                        UserInfo.LocationID = dt.Rows[0]["Location"].ToString();
                        UserInfo.AccountID = dt.Rows[0]["Account_id"].ToString();
                        reset = dt.Rows[0]["flag"].ToString();
                        //string token = JWTHelper.CreateJWTToken(UserID, UserInfo.UserType);
                        //if (token != null)
                        //{


                        //    response.Cookies.Append("Token", token, new CookieOptions
                        //    {
                        //        HttpOnly = true, 
                        //        Secure = true,
                        //        SameSite = SameSiteMode.Strict, 
                        //        Expires = DateTimeOffset.UtcNow.AddHours(1) 
                        //    });


                        //}

                        if (reset == "0")
                        {

                            return 2;
                        }

                        {
                            return 1;
                        }
                    }
                    else
                    {
                        UserInfo.UserType = "failed";
                        return 0;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<int> CheckSuperAdminIsValid(string UserID, string Password)
        {
            try
            {
                string encryptedUserID = await _enc.EncryptAsync(UserID);
                string encryptedPassword = await _enc.EncryptAsync(Password);
                string Conn = await _enc.DecryptAsync(_con);
                using (SqlConnection con = new SqlConnection(Conn))
                {
                    await con.OpenAsync();
                    DataTable dt = new DataTable();
                    SqlCommand cmd = new SqlCommand("Sp_Check_LogIn", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Empcode", encryptedUserID);
                    cmd.Parameters.AddWithValue("@Password", encryptedPassword);
                    SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                    adpt.Fill(dt);
                    await con.CloseAsync();
                    if (dt.Rows.Count > 0)
                    {
                        UserInfo.UserName = await _enc.DecryptAsync(dt.Rows[0]["Name"].ToString());
                        UserInfo.UserType = await _enc.DecryptAsync(dt.Rows[0]["usertype"].ToString());
                      

                        return 1;
                    }


                }

            }
            catch (Exception ex)
            {

            }

            return 0;
        }

        public async Task<int> CheckUserLogInAsync(string UserID, string Password)
        {
            string encryptedUserID = await _enc.EncryptAsync(UserID);
            string encryptedPassword = await _enc.EncryptAsync(Password);
            string Conn = await _enc.DecryptAsync(_con);
            try
            {
                using (SqlConnection cc = new SqlConnection(Conn))
                {
                    await cc.OpenAsync();
                    DataTable dt = new DataTable();
                    SqlCommand cmd = new SqlCommand("Sp_Check_LogIn", cc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Empcode", encryptedUserID);
                    cmd.Parameters.AddWithValue("@Password", encryptedPassword);
                    SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                    adpt.Fill(dt);
                    await cc.CloseAsync();
                    if (dt.Rows.Count > 0)
                    {
                        UserInfo.UserType = await _enc.DecryptAsync(dt.Rows[0]["usertype"].ToString());
                        return 1;
                    }
                    else
                    {
                        UserInfo.UserType = "Account_User";
                        return 0;
                    }

                }
            }
            catch (SqlException sqlEx)
            {
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }


    }
}
