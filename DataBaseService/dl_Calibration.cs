using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace QMS.DataBaseService
{
    public class dl_Calibration
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dlcon;
        private readonly HttpResponse response;

        public dl_Calibration(DL_Encrpt dL_Encrpt)
        {

            _enc = dL_Encrpt;
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
