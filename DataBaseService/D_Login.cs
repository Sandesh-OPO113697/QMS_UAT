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
        public D_Login(IConfiguration configuration , DL_Encrpt dL_Encrpt, DLConnection conn)
        {
            _con = configuration.GetConnectionString("Master_Con");
            _enc = dL_Encrpt;
            _dlcon = conn;
        }
        public async Task<int> CheckAccountUserAsync(string UserID, string Password)
        {
            string Dycon = await _dlcon.GetDynStrByUserIDAsync(UserID);
            try
            {
                using (SqlConnection cc = new SqlConnection(Dycon))
                {
                    await cc.OpenAsync();
                    DataTable dt = new DataTable();
                    SqlCommand cmd = new SqlCommand("Sp_Check_LogIn", cc);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Empcode", await _enc.EncryptAsync(UserID));
                    cmd.Parameters.AddWithValue("@Password", await _enc.EncryptAsync(Password));
                    SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                    await Task.Run(() => adpt.Fill(dt)); 

                    if (dt.Rows.Count > 0)
                    {
                        UserInfo.UserName = await _enc.DecryptAsync(dt.Rows[0]["Name"].ToString());
                        UserInfo.UserType = dt.Rows[0]["usertype"].ToString();
                        UserInfo.IsActive = dt.Rows[0]["isactive"].ToString();
                        UserInfo.LocationID = dt.Rows[0]["Location"].ToString();

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
                    cmd.Parameters.AddWithValue("@Empcode", encryptedPassword);
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
            catch(Exception ex)
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
