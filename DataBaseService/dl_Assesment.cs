using QMS.Models;
using System.Data.SqlClient;
using System.Data;

namespace QMS.DataBaseService
{
    public class dl_Assesment
    {
        public async Task<int> createAssesmemnt(List<QuestionModel> questions)
        {
            int testID = 0;
            string Testname = questions[0].subject.ToString();
            string category = questions[0].category.ToString();
            string ProgramID = questions[0].programId.ToString();
            string SubProgramId = questions[0].SUBProgramID.ToString();
            string expiryType = questions[0].expiryType.ToString();
            string expiryDate = questions[0].expiryDate.ToString();
            string expiryHours = questions[0].expiryHours.ToString();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("usp_InsertTestID", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ProgramID", ProgramID);
                        cmd.Parameters.AddWithValue("@SubProgramId", SubProgramId);
                        cmd.Parameters.AddWithValue("@TestName", Testname);
                        cmd.Parameters.AddWithValue("@Category", category);
                        cmd.Parameters.AddWithValue("@expiryType", expiryType);
                        cmd.Parameters.AddWithValue("@expiryDate", expiryDate);
                        cmd.Parameters.AddWithValue("@expiryHours", expiryHours);
                        SqlParameter outputIdParam = new SqlParameter("@TestID", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputIdParam);

                        cmd.ExecuteNonQuery();
                        testID = Convert.ToInt32(outputIdParam.Value);
                    }

                }
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    conn.Open();

                    foreach (var q in questions)
                    {
                        int questionId = 0;
                        using (SqlCommand cmd = new SqlCommand("usp_InsertQuestion", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@ProgramID", q.programId);
                            cmd.Parameters.AddWithValue("@SubProgramId", q.SUBProgramID);
                            cmd.Parameters.AddWithValue("@QuestionText", q.question);
                            cmd.Parameters.AddWithValue("@AnswerType", q.answerType);
                            cmd.Parameters.AddWithValue("@TestID", testID);
                            SqlParameter outputIdParam = new SqlParameter("@QuestionId", SqlDbType.Int)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(outputIdParam);

                            cmd.ExecuteNonQuery();
                            questionId = Convert.ToInt32(outputIdParam.Value);
                        }

                        for (int i = 0; i < q.options.Count; i++)
                        {
                            bool isCorrect = q.correctAnswers.Contains(i);
                            using (SqlCommand optCmd = new SqlCommand("usp_InsertQuestionWithOptions", conn))
                            {
                                optCmd.CommandType = CommandType.StoredProcedure;

                                optCmd.Parameters.AddWithValue("@QuestionId", questionId);
                                optCmd.Parameters.AddWithValue("@OptionText", q.options[i]);
                                optCmd.Parameters.AddWithValue("@IsCorrect", isCorrect);
                                optCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                return 0;

            }
        }
    }


}
