using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;
using System.Diagnostics;

namespace QMS.Controllers
{
    public class NotpadController : Controller
    {
        private readonly DL_Module _module;
        private readonly Dl_Admin _admin;
        private readonly Dl_formBuilder dl_FormBuilder;

        private readonly DL_Notpad dl_Notpad;

        public NotpadController(DL_Module adl, Dl_Admin adam, Dl_formBuilder adl2, DL_Notpad not)
        {
            _module = adl;
            _admin = adam;

            dl_FormBuilder = adl2;

            dl_Notpad = not;
        }

        public async Task<ActionResult> Notpad()
        {
            string locationid = UserInfo.LocationID;
            var data = await _admin.GetProcessListByLocation(locationid);
            var processList = data.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.ProcessList = processList;

            return View();
        }

        public async Task<ActionResult> UseNotpad()
        {


            return View();
        }




        [HttpPost]
        public async Task<List<SelectListItem>> GetUsersByProgram([FromBody] Agentlist request)
        {
            string ProcessID = request.ProcessID;
            string SuprocessID = request.SUBProgramID;
            var Agnetlist = await dl_Notpad.Getuser(ProcessID, SuprocessID);

            return Agnetlist;
        }


        [HttpPost]
        public async Task<IActionResult> InsertAgentMapping([FromBody] AgentMappingRequest request)
        {
            if (request == null || request.AgentIds == null || !request.AgentIds.Any())
            {
                return BadRequest("No agents selected.");
            }

            await dl_Notpad.InsertAgentMapping(request.ProcessName, request.SubProcessName, request.AgentIds, request.NotepadAccess);

            return Ok("Data inserted successfully.");
        }





    }
}
