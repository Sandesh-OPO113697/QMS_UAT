using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.Encription;
using QMS.Models;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace QMS.DataBaseService
{
    public class DL_Notpad
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dcl;
        public DL_Notpad(IConfiguration configuration, DL_Encrpt dL_Encrpt, DLConnection dL)
        {
            _con = configuration.GetConnectionString("Master_Con");
            _enc = dL_Encrpt;
            _dcl = dL;
        }

        public async Task<List<SelectListItem>> Getuser(String ProcessID, String SuprocessID)
        {

            var processes = new List<SelectListItem>();
            string storedProcedure = "sp_GetUsersByProgramAndSubProgram";

            try
            {
                using (var connection = new SqlConnection(UserInfo.Dnycon))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(storedProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ProgramId", ProcessID);
                        command.Parameters.AddWithValue("@SubProgramId", SuprocessID);


                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string displayText = reader["Name"].ToString();
                                string value = reader["EMPID"].ToString(); ;

                                bool isSelected = Convert.ToInt32(reader["NotepadAccess"]) == 1;

                                processes.Add(new SelectListItem
                                {
                                    Text = displayText,
                                    Value = value,
                                    Selected = isSelected

                                }); ;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

            return processes;
        }




        public async Task InsertAgentMapping(string processName, string subProcessName, List<string> agentIds, bool notepadAccess)
        {
            string notepadAccessStr = notepadAccess ? "1" : "0";

            using (var connection = new SqlConnection(UserInfo.Dnycon))
            {
                await connection.OpenAsync();

                using (var commands = new SqlCommand("sp_InactiveAgentMapping", connection))
                {
                    commands.CommandType = CommandType.StoredProcedure;
                    commands.Parameters.AddWithValue("@ProcessName", processName);
                    commands.Parameters.AddWithValue("@SubProcessName", subProcessName);
                    await commands.ExecuteNonQueryAsync();
                }

                foreach (var agentId in agentIds)
                {
                    using (var command = new SqlCommand("sp_InsertAgentMapping", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@AgentID", agentId);
                        command.Parameters.AddWithValue("@ProcessName", processName);
                        command.Parameters.AddWithValue("@SubProcessName", subProcessName);
                        command.Parameters.AddWithValue("@NotepadAccess", notepadAccessStr);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

    }
}
