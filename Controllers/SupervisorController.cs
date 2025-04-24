using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.SS.Formula.Functions;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;

namespace QMS.Controllers
{
    public class SupervisorController : Controller
    {
        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly DL_Operation dl_Ops;
        private readonly DL_Agent dl_Agent;
        private readonly DL_QaManager dl_qa;
        private readonly dl_Supervisor dl_super;
        private readonly Dl_Admin _admin;
        private readonly Dl_Coaching dl_Coaching;
        public SupervisorController(DL_Operation adls, DL_QaManager dl_qas, Dl_Coaching dcl, DL_Agent adla, dl_Supervisor dl_super, Dl_Admin admin     )
        {

            dl_Ops = adls;
            dl_qa = dl_qas;
            dl_Agent = adla;
            this.dl_super = dl_super;
            _admin = admin;
            dl_Coaching = dcl;
        }   

        [HttpPost]
        public async Task<IActionResult> ExtenedCoching([FromBody] CoachingRequestModel model)
        {
           await  dl_Coaching.ExtendCauching(model.ProgramID , model.SUBProgramID ,model.AgentID);
            return Json(new { success = true, message = "Coaching extended successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> HRPIP([FromBody] CoachingRequestModel model)
        {
            await dl_Coaching.PIPCauching(model.ProgramID, model.SUBProgramID, model.AgentID);
            return Json(new { success = true, message = "Coaching PIP successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> ClosedCoching([FromBody] CoachingRequestModel model)
        {
            await dl_Coaching.ClosedCauching(model.ProgramID, model.SUBProgramID, model.AgentID);
            return Json(new { success = true, message = "Coaching extended successfully." });
        }

        public async Task<IActionResult> DashBoard(string RoleName, string Featureid, string SubFeatureid)
        {
            List<ReviewDataModel> coutingList = await dl_qa.GetCaoutingList();
            DataTable dt = await _admin.GetProcessListAsync();
            var processList = dt.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.Process = processList;
            return View(coutingList);
        }
    }
}
