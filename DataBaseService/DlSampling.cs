using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Data;

namespace QMS.DataBaseService
{
    public class DlSampling
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dcl;
        public DlSampling(IConfiguration configuration, DL_Encrpt dL_Encrpt, DLConnection dL)
        {
            _con = configuration.GetConnectionString("Master_Con");
            _enc = dL_Encrpt;
            _dcl = dL;
        }

        public async Task AddSamplingCount(int SamplingSize , string FetureID , string SubFetureid , string RoleName , string process , string subprocess)
        {
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("sp_Sampling", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SamplingSize", SamplingSize);
                    cmd.Parameters.AddWithValue("@RoleName", RoleName);
                    cmd.Parameters.AddWithValue("@Location", UserInfo.LocationID);
                    cmd.Parameters.AddWithValue("@Process", process);
                    cmd.Parameters.AddWithValue("@SubProcess" , subprocess);
                    cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

        }

    }
}
