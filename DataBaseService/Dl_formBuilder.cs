using QMS.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.VisualBasic.FileIO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QMS.DataBaseService
{
    public class Dl_formBuilder
    {

        
        public async Task<List<dynamic>> GetStaticfiedls()
        {
            List<dynamic> fields = new List<dynamic>();

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("GetStaticfeilds", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                fields.Add(new
                                {
                                    Static_Field_Name = reader["Static_Field_Name"].ToString(),
                                    DataType = reader["DataType"].ToString()
                                });
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {

            }
            return fields;
        }
        public async Task<DataSet> GetSectionFeildAsync()
        {
            DataSet dt = new DataSet();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("SectionValues", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                     
                        SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                        await Task.Run(() => adpt.Fill(dt));
                    }
                }
               

            }
            catch (Exception ex)
            {

            }
            return dt;
        }


        public async Task<int> addDynamicFeilds(List<Dynamicfeilds> fields)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    foreach (var field in fields)
                    {
                        using (SqlCommand cmd = new SqlCommand("FormCreation", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Operation", "InserDynamicFeilds");
                            cmd.Parameters.AddWithValue("@DNYFieldName", field.FieldName);
                            cmd.Parameters.AddWithValue("@dnyFieldType", field.FieldType);
                            cmd.Parameters.AddWithValue("@dnyFieldValue", field.FieldValues);
                            cmd.Parameters.AddWithValue("@Process", field.ProgramID);
                            cmd.Parameters.AddWithValue("@SubProcess", field.SubProgramID);
                            cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);
                            cmd.CommandTimeout = 0;

                            object result = await cmd.ExecuteScalarAsync();
                            rowsAffected += Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting dynamic fields: " + ex.Message);
            }

            return rowsAffected;
        }

        public async Task<int> AddSectionfeilds(List<SectionModel> fields)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    foreach (var field in fields)
                    {
                        using (SqlCommand cmd = new SqlCommand("FormCreation", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Operation", "InserSectionFeilds");
                            cmd.Parameters.AddWithValue("@secCategory", field.Category);
                            cmd.Parameters.AddWithValue("@secSection", field.Section);
                            cmd.Parameters.AddWithValue("@secRating", field.Rating);
                            cmd.Parameters.AddWithValue("@secScorable", field.Scorable);
                            cmd.Parameters.AddWithValue("@secScore", field.Score);
                            cmd.Parameters.AddWithValue("@secLevel", field.Level);
                            cmd.Parameters.AddWithValue("@secFatal", field.Fatal);
                            cmd.Parameters.AddWithValue("@Process", field.ProgramID);
                            cmd.Parameters.AddWithValue("@SubProcess", field.SubProgramID);
                            cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);
                            cmd.CommandTimeout = 0;

                            object result = await cmd.ExecuteScalarAsync();
                            rowsAffected += Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting dynamic fields: " + ex.Message);
            }

            return rowsAffected;
        }
    }
}
