using Microsoft.VisualBasic;
using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace QMS.DataBaseService
{
    public class DLConnection
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;

        public DLConnection(IConfiguration configuration, DL_Encrpt dL_Encrpt)
        {
            _con = configuration.GetConnectionString("Master_Con");
            _enc = dL_Encrpt;
        }

        public async Task<string> GetDynStrByAccountAsync(string AccountID)
        {
            string query = "SELECT Account_DB_IP, Account_db_Name, Account_User_ID, Account_Password FROM AccountDetails WHERE AccountID = @AccountID";
            string connString = string.Empty;

            // Await the decryption of the connection string asynchronously
            string Conn = await _enc.DecryptAsync(_con);

            using (SqlConnection conn = new SqlConnection(Conn))
            {
                await conn.OpenAsync();

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@AccountID", AccountID);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) 
                    {
                        
                        string accountDbName = await _enc.DecryptAsync(reader["Account_db_Name"].ToString());
                        string accountUserId = await _enc.DecryptAsync(reader["Account_User_ID"].ToString());
                        string accountDbPassword = await _enc.DecryptAsync(reader["Account_Password"].ToString());
                        string Account_DB_IP = await _enc.DecryptAsync(reader["Account_DB_IP"].ToString());

                        connString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};",
                                                   Account_DB_IP,
                                                   accountDbName,
                                                   accountUserId,
                                                   accountDbPassword);
                        UserInfo.Dnycon = connString;
                    }
                }
            }

            return connString;
        }

        public async Task<string> GetDynStrByUserIDAsync(string UserID)
        {
            string prefix = string.Empty;
            if (!string.IsNullOrEmpty(UserID) && UserID.Length >= 3)
            {
                prefix = UserID.Substring(0, 3);
            }
            string RncPrifix = await _enc.EncryptAsync(prefix);
            string connString = string.Empty;

            string Conn = await _enc.DecryptAsync(_con);

            string query = "SELECT Account_DB_IP, Account_db_Name, Account_User_ID, Account_Password FROM AccountDetails WHERE AccountPrefix = @Prifix";

            using (SqlConnection conn = new SqlConnection(Conn))
            {
                await conn.OpenAsync(); 

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Prifix", RncPrifix);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) 
                    {
                        if (await reader.ReadAsync()) 
                        {
                            string accountDbName = await _enc.DecryptAsync(reader["Account_db_Name"].ToString());
                            string accountUserId = await _enc.DecryptAsync(reader["Account_User_ID"].ToString());
                            string accountDbPassword = await _enc.DecryptAsync(reader["Account_Password"].ToString());
                            string accountDbIp = await _enc.DecryptAsync(reader["Account_DB_IP"].ToString());

                            connString = string.Format(
                                "Data Source={0};Initial Catalog={1};User ID={2};Password={3};",
                                accountDbIp,
                                accountDbName,
                                accountUserId,
                                accountDbPassword
                            );
                            UserInfo.Dnycon = connString;
                        }
                    }
                }
            }

            return connString;
        }
    }
}
