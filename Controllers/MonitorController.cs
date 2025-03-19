using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;

namespace QMS.Controllers
{
    public class MonitorController : Controller
    {
        private readonly DlSampling dlSampling;
        private readonly Dl_Admin _admin;
        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly dl_Monitoring dl_monitor;
        public MonitorController(Dl_formBuilder adl, DlSampling dl, Dl_Admin adam, dl_Monitoring dmmonoi)
        {
            dlSampling = dl;
            _admin = adam;
            dl_FormBuilder = adl;
            dl_monitor = dmmonoi;
        }

       
        public async Task<IActionResult> GetRecording([FromBody] DropDawnString id)
        {
            if (id == null || string.IsNullOrEmpty(id.ID))
            {
                return Json(new { success = false, message = "Invalid ID" });
            }

            try
            {
                string base64Audio = await dl_monitor.GetRecordingByConnID(id.ID.ToString());
                

                return Json(new { success = true, audioData = base64Audio });
            }
            catch (FileNotFoundException)
            {
                return Json(new { success = false, message = "File not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        public async Task<JsonResult> GetTLName([FromBody] DropDawon id)
        {

            string tl_name = await dl_monitor.GetTeamLeaderName(id.Id.ToString());

            return Json(new { success = true, tl_name });
        }
        [HttpPost]
        public async Task<JsonResult> GetRecListBydate([FromBody] RecApiModel request)
        {
            if (request == null || string.IsNullOrEmpty(request.AgentId) || string.IsNullOrEmpty(request.FromDate) || string.IsNullOrEmpty(request.ToDate))
            {
                return Json(new { success = false, message = "Invalid input data." });
            }
            else
            {
                string reclist = await dl_monitor.GetRecListByAPi(request.FromDate, request.ToDate, request.AgentId);


                return Json(new { success = true, message = "sucesss.", reclist = reclist });
            }

        }

        public async Task<JsonResult> GetDispositoin([FromBody] Process_SUbProcess id)
        {
            List<SelectListItem> sampleSize = await dl_monitor.GetDisposition(id.ProcessID.ToString(), id.SUBProcessID.ToString());
            List<SelectListItem> agentlist = await dl_monitor.GetAgentName(id.ProcessID.ToString(), id.SUBProcessID.ToString());

            return Json(new { success = true, samplesize = sampleSize, agentlist = agentlist });

        }

        public async Task<JsonResult> GetSubDispositoin([FromBody] DispositionModel id)
        {
            List<SelectListItem> sampleSize = await dl_monitor.GetSubDisposition(id.ProcessID.ToString(), id.SUBProcessID.ToString(), id.Disposition.ToString());

            return Json(new { success = true, samplesize = sampleSize });

        }

        public async Task<IActionResult> CallMonitor(int SubFeatureid, string RoleName, int Featureid)
        {
            var AuditType = await dlSampling.GetAuditType();
            ViewBag.AuditTypeList = AuditType;

            DataTable dt = await _admin.GetProcessListAsync();
            var processList = dt.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.Process = processList;
            return View();
        }
    }
}
