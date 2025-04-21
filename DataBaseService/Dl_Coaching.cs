using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Data;

namespace QMS.DataBaseService
{
    public class Dl_Coaching
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dcl;
        public Dl_Coaching(DL_Encrpt dL_Encrpt, DLConnection dL)
        {

            _enc = dL_Encrpt;
            _dcl = dL;
        }
        public async Task<List<object>> GetQaManagerList(int ProcessID, int SubProcessID)
        {

            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("Coaching", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "GetQaManager");
                        cmd.Parameters.AddWithValue("@ProgramID", ProcessID);
                        cmd.Parameters.AddWithValue("@SubProcessID", SubProcessID);
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        adpt.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
               
                list.Add(new
                {

                    User_Id = row["User_Id"].ToString(),

                    UserName = row["UserName"].ToString(),
                   
                });
            }

            return list;

        }
    }

}
