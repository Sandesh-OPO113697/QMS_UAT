using QMS.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.VisualBasic.FileIO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static System.Collections.Specialized.BitVector32;
using Org.BouncyCastle.Asn1.Cms;

namespace QMS.DataBaseService
{
    public class Dl_formBuilder
    {

        public async Task<DataTable> GetProcessListAsync()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("EditFormvalue", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                  
                        cmd.Parameters.AddWithValue("@Operation", "GetProcessByadmin");
                

                    SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                    await Task.Run(() => adpt.Fill(dt));
                }
            }
            return dt;
        }
        public async Task<bool> ActivateFormByID(int processId, int subProcessId)
        {
            using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("EditFormvalue", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Operation", "ActiveReplicatedForm");
                    cmd.Parameters.AddWithValue("@ProcessID", processId);
                    cmd.Parameters.AddWithValue("@SubProcessID", subProcessId);
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<DataTable> GetGriedOfReplicatedForm()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("EditFormvalue", conn))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        if(UserInfo.UserType=="Admin")
                        {
                            cmd.Parameters.AddWithValue("@Operation", "GetToActivereplicatdForm");
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@Operation", "GetToActivereplicatdFormByLocation");
                            cmd.Parameters.AddWithValue("@LocationID", UserInfo.LocationID);

                        }
                          
                        
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


        public async Task<int> FormReplicationfromOld(FormReplicationModel model)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("FormCreation", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "ReplicateForm");
                        cmd.Parameters.AddWithValue("@Process", model.ProcessID);
                        cmd.Parameters.AddWithValue("@SubProcess", model.SUBProcessID);
                        cmd.Parameters.AddWithValue("@ProcessIDOLD", model.ProcessIDOLD);
                        cmd.Parameters.AddWithValue("@SUBProcessIDOLD", model.SUBProcessIDOLD);
                        cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);
                        object result = await cmd.ExecuteScalarAsync();
                        return (result != null && int.TryParse(result.ToString(), out int value)) ? value : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return -1; // Indicate error
            }
        }


        public async Task<int> DisableFormTable(Process_SUbProcess model)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("FormCreation", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "DisableForm");
                        cmd.Parameters.AddWithValue("@Process", model.ProcessID);
                        cmd.Parameters.AddWithValue("@SubProcess", model.SUBProcessID);

                        object result = await cmd.ExecuteScalarAsync();
                        return (result != null && int.TryParse(result.ToString(), out int value)) ? value : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return -1; // Indicate error
            }
        }
        public async Task<int> CheckIsFormCreatedInData(Process_SUbProcess model)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("FormCreation", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "CheckIsformcreatd");
                        cmd.Parameters.AddWithValue("@Process", model.ProcessID);
                        cmd.Parameters.AddWithValue("@SubProcess", model.SUBProcessID);

                        object result = await cmd.ExecuteScalarAsync();
                        return result != null ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return -1; // Indicate error
            }
        }
        public async Task<int> CheckIsFormreplicatedInData(Process_SUbProcess model)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("FormCreation", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "CheckIsformReplicated");
                        cmd.Parameters.AddWithValue("@Process", model.ProcessID);
                        cmd.Parameters.AddWithValue("@SubProcess", model.SUBProcessID);

                        object result = await cmd.ExecuteScalarAsync();
                        return result != null ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return -1; // Indicate error
            }
        }


        public async Task<int> UpdateValueInDynamicmaster(DynamicModelNew model)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("FormCreation", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "UpdateDynamicFeilds");
                        cmd.Parameters.AddWithValue("@Root_Cause_Analysis", model.Root_Cause_Analysis ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Predictive_Analysis", model.Predictive_Analysis ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ZT_Classification", model.ZT_Classification ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Zero_Tolerance", model.Zero_Tolerance ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Process", model.ProgramID > 0 ? model.ProgramID : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@SubProcess", model.SubProgramID > 0 ? model.SubProgramID : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName ?? (object)DBNull.Value);

                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0 ? result : 0;
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error: {ex.Message}");
                return -1;
            }
        }




        public async Task<int> UpdateValueInDynamicmasterReplicatedForm(DynamicModelNew model)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("FormCreation", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "UpdateDynamicFeildsReplicated");
                        cmd.Parameters.AddWithValue("@Root_Cause_Analysis", model.Root_Cause_Analysis ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Predictive_Analysis", model.Predictive_Analysis ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ZT_Classification", model.ZT_Classification ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Zero_Tolerance", model.Zero_Tolerance ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Process", model.ProgramID > 0 ? model.ProgramID : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@SubProcess", model.SubProgramID > 0 ? model.SubProgramID : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName ?? (object)DBNull.Value);

                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0 ? result : 0;
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error: {ex.Message}");
                return -1;
            }
        }



        public async Task<DataTable> GetDynamicGriedAsync(int processID, int SubprocessID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("EditFormvalue", conn))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "GetDyanamicFeildForedit");
                        cmd.Parameters.AddWithValue("@processID", processID);
                        cmd.Parameters.AddWithValue("@SubprocessID", SubprocessID);
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



        public async Task<DataTable> GetDynamicGriedReplicatedAsync(int processID, int SubprocessID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("EditFormvalue", conn))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "GetDyanamicFeilReplicatedit");
                        cmd.Parameters.AddWithValue("@processID", processID);
                        cmd.Parameters.AddWithValue("@SubprocessID", SubprocessID);
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

        public async Task<int> InsertValueInDynamicmaster(DynamicModelNew model)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(UserInfo.Dnycon))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("FormCreation", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "InserDynamicFeilds");
                        cmd.Parameters.AddWithValue("@Root_Cause_Analysis", model.Root_Cause_Analysis ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Predictive_Analysis", model.Predictive_Analysis ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ZT_Classification", model.ZT_Classification ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Zero_Tolerance", model.Zero_Tolerance ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Process", model.ProgramID > 0 ? model.ProgramID : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@SubProcess", model.SubProgramID > 0 ? model.SubProgramID : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedBy", UserInfo.UserName ?? (object)DBNull.Value);

                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0 ? result : 0;  
                    }
                }
            }
            catch (Exception ex)
            {
               
                Console.WriteLine($"Error: {ex.Message}");
                return -1; 
            }
        }


        public async Task<DataTable> getZT_Classification()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("EditFormvalue", conn))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "getZT_Classification");
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
        public async Task<DataTable> GetgetPredictive_Analysis()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("EditFormvalue", conn))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "getPredictive_Analysis");
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
        public async Task<DataTable> GetRoot_Cause_AnalysisAsync()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("EditFormvalue", conn))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation" , "getRouteCause");
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
        public async Task<int> UpdatedynamicFeilds(DynamicFieldUpdateRequest dt)
        {
            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("EditFormvalue", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Operation", "UpdatedynamicDrop");
                    cmd.Parameters.AddWithValue("@Id", dt.Id);
                    cmd.Parameters.AddWithValue("@NewdropValue", dt.NewValue);
                    var result = await cmd.ExecuteScalarAsync();
                    int rowsAffected = result != null ? Convert.ToInt32(result) : 0;
                    return rowsAffected;

                }
            }
        }
        public async Task<bool> DeleteSectionAsync(int sectionId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("EditFormvalue", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "deleteSection");
                        cmd.Parameters.AddWithValue("@Id", sectionId);
                        var result = await cmd.ExecuteScalarAsync();
                        int rowsAffected = result != null ? Convert.ToInt32(result) : 0;

                        if (rowsAffected > 0)
                            return (true);
                        else
                            return (false);
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task EditSectionRow(SectionGridModel section)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("EditFormvalue", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "UpdateSecrtion");
                        cmd.Parameters.AddWithValue("@Id", section.Id);
                        cmd.Parameters.AddWithValue("@Category", section.Category);
                        cmd.Parameters.AddWithValue("@SectionId", section.SectionId);
                     
                        cmd.Parameters.AddWithValue("@Scorable", section.Scorable);
                        cmd.Parameters.AddWithValue("@Score", section.Score);
                        cmd.Parameters.AddWithValue("@Level", section.Level);
                       
                        cmd.Parameters.AddWithValue("@UserName", UserInfo.UserName);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
           
            }
            catch (Exception ex)
            {
                
            }
        }
        public async Task<DataSet> GetSectionDropdownDataAsync()
        {
            DataSet ds = new DataSet();

            using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("EditFormvalue", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Operation", "GetSection");

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable sectionTable = new DataTable();
                        adapter.Fill(sectionTable);
                        sectionTable.TableName = "Sections"; // Naming the table
                        ds.Tables.Add(sectionTable);
                    }
                }

                using (SqlCommand cmd = new SqlCommand("EditFormvalue", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Operation", "GetRatings");

                    using (SqlDataAdapter ratingAdpt = new SqlDataAdapter(cmd))
                    {
                        DataTable ratingTable = new DataTable();
                        ratingAdpt.Fill(ratingTable);
                        ratingTable.TableName = "Ratings"; // Naming the table
                        ds.Tables.Add(ratingTable);
                    }
                }
            }

            return ds;
        }
        public async Task<DataTable> GetdynamicFeildsGriedAsync(int processID, int SubprocessID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("EditFormvalue", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "GetDyanamicFeilReplicatedit");
                        cmd.Parameters.AddWithValue("@processID", processID);
                        cmd.Parameters.AddWithValue("@SubprocessID", SubprocessID);
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

        public async Task<DataTable> GetSectionGriedAsync(int processID , int SubprocessID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("EditFormvalue", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation" , "GetSectionGried");
                        cmd.Parameters.AddWithValue("@processID", processID);
                        cmd.Parameters.AddWithValue("@SubprocessID" , SubprocessID);
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

        public async Task<DataTable> GetSectionGriedReplicatedAsync(int processID, int SubprocessID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("EditFormvalue", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Operation", "GetSectionGriedReplicated");
                        cmd.Parameters.AddWithValue("@processID", processID);
                        cmd.Parameters.AddWithValue("@SubprocessID", SubprocessID);
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
                          
                            cmd.Parameters.AddWithValue("@secScorable", field.Scorable);
                            cmd.Parameters.AddWithValue("@secScore", field.Score);
                            cmd.Parameters.AddWithValue("@secLevel", field.Level);
                        
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



        public async Task<int> UpdateSectionfeilds(List<SectionUpdateModel> fields)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("FormCreation", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "DeleteDectionData");
                        cmd.Parameters.AddWithValue("@Process", fields[0].ProgramID);
                        cmd.Parameters.AddWithValue("@SubProcess", fields[0].SubProgramID);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

            }
            catch (Exception ex)
            {

            }

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
                            cmd.Parameters.AddWithValue("@Operation", "updateDectionFeild");
                            cmd.Parameters.AddWithValue("@secCategory", field.Category);
                            cmd.Parameters.AddWithValue("@secSectionname", field.Section);

                            cmd.Parameters.AddWithValue("@secScorable", field.Scorable);
                            cmd.Parameters.AddWithValue("@secScore", field.Score);
                            cmd.Parameters.AddWithValue("@secLevel", field.Level);

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





        public async Task<int> UpdateSectionfeildsReplicatedForm(List<SectionUpdateModel> fields)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(UserInfo.Dnycon))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("FormCreation", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Operation", "DeleteDectionDataReplicatedForm");
                        cmd.Parameters.AddWithValue("@Process", fields[0].ProgramID);
                        cmd.Parameters.AddWithValue("@SubProcess", fields[0].SubProgramID);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

            }
            catch (Exception ex)
            {

            }

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
                            cmd.Parameters.AddWithValue("@Operation", "updateDectionFeildReplicated");
                            cmd.Parameters.AddWithValue("@secCategory", field.Category);
                            cmd.Parameters.AddWithValue("@secSectionname", field.Section);

                            cmd.Parameters.AddWithValue("@secScorable", field.Scorable);
                            cmd.Parameters.AddWithValue("@secScore", field.Score);
                            cmd.Parameters.AddWithValue("@secLevel", field.Level);

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
