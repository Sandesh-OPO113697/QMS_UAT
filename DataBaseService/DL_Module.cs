using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Data;

namespace QMS.DataBaseService
{
    public class DL_Module
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dcl;
        public DL_Module(IConfiguration configuration, DL_Encrpt dL_Encrpt, DLConnection dL)
        {
            _con = configuration.GetConnectionString("Master_Con");
            _enc = dL_Encrpt;
            _dcl = dL;
        }

        public async Task UpdatePauseCount(int ProcessID, int SubProcess, string Pause)
        {

            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    string query = "EditFormvalue";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "UpdatePasuseCount");
                        cmd.Parameters.AddWithValue("@Pausecontt", Pause);
                        cmd.Parameters.AddWithValue("@processID", ProcessID);
                        cmd.Parameters.AddWithValue("@SubprocessID", SubProcess);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch
            {

            }

        }

    }
}
