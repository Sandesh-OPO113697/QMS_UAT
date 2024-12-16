using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using QMS.Encription;
using QMS.Models;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace QMS.DataBaseService
{
    public class D_Login
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dlcon;
        public D_Login(IConfiguration configuration, DL_Encrpt dL_Encrpt, DLConnection conn)
        {
            _con = configuration.GetConnectionString("Master_Con");
            _enc = dL_Encrpt;
            _dlcon = conn;
        }
        public async Task AssignRoleToUser(string UserID , HttpContext httpContext)
        {
            string Dycon = await _dlcon.GetDynStrByUserIDAsync(UserID);
            string query = @"
                SELECT 
                    upm.Proram_id as Program_id,
                    upm.ProgramName as ProgramName,
                    upm.Sub_ProgramId as Sub_ProgramId,
                    (SELECT SubProcessName FROM [dbo].[Eval_SubProcess] WHERE id = upm.Sub_ProgramId) AS SubProcessName,
                    upm.Userid as Userid,
                    urm.Role_Name AS UserRoleName,
                    ufm.FeatureName as FeatureName
                FROM 
                    [dbo].[User_Program_Mapping] upm
                JOIN 
                    [dbo].[User_Role_Mapping] urm 
                    ON upm.Userid = urm.User_Id
                LEFT JOIN 
                    [dbo].[User_Feature_Mapping] ufm
                    ON urm.Role_Name = ufm.RoleName 
                WHERE 
                    urm.UserName = @UserName
                GROUP BY 
                    upm.Proram_id, 
                    upm.ProgramName, 
                    upm.Userid, 
                    urm.Role_Name, 
                    upm.Sub_ProgramId, 
                    ufm.FeatureName";
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(Dycon))
                {

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserName", UserID);
                       await connection.OpenAsync();
                        SqlDataAdapter adpt = new SqlDataAdapter(command);
                        await Task.Run(() => adpt.Fill(dt));
                    }

                }
                var distinctProcesses = dt.AsEnumerable()
                                                   .GroupBy(row => new
                                                   {
                                                       ProcessId = row["Program_id"],
                                                       ProcessName = row["ProgramName"],
                                                       SubProcessId = row["Sub_ProgramId"],
                                                       SubProcessName = row["SubProcessName"],
                                                       UserId = row["Userid"],
                                                       UserRoleName = row["UserRoleName"]
                                                   })
                                                   .Select(g => new
                                                   {
                                                       ProcessId = g.Key.ProcessId,
                                                       ProcessName = g.Key.ProcessName,
                                                       SubProcessId = g.Key.SubProcessId,
                                                       SubProcessName = g.Key.SubProcessName,
                                                       UserId = g.Key.UserId,
                                                       UserRoleName = g.Key.UserRoleName
                                                   }).ToList();
                List<string> distinctFeatureNames = dt.AsEnumerable().Select(row => row["FeatureName"].ToString()).Distinct().ToList();
                UserAcesslevel.DistinctFeatureNames = distinctFeatureNames;
                httpContext.Session.SetString("FeatureList", JsonConvert.SerializeObject(distinctFeatureNames));
                var groupedData = dt.AsEnumerable().GroupBy(row => new{RoleName = row["UserRoleName"].ToString(), }).ToDictionary(
                            g => g.Key.RoleName,
                            g => g.Select(row => row["FeatureName"].ToString()).Distinct().ToList());
                httpContext.Session.SetString("RoleFeatureMapping", JsonConvert.SerializeObject(groupedData));
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<int> CheckAccountUserAsync(string UserID, string Password)
        {
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


                        return 1;
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
