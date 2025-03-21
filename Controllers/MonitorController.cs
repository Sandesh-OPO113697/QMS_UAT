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
        [HttpPost]
        [Route("save-audit")]
        public async Task< IActionResult > SaveAudit([FromBody] AuditPauseLog audit)
        {
            if (audit == null)
                return BadRequest("Invalid audit data");
            var data = await dl_monitor.InsertAuditPauseLog(audit);

            return Ok(new { message = "Audit saved successfully!" });
        }


        public async Task<IActionResult> CheckTheAuditIsDone([FromBody] DropDawnString id)
        {
            if (id == null || string.IsNullOrEmpty(id.ID))
            {
                return Json(new { success = false, message = "Invalid ID" });
            }

            try
            {
                string TransactionID = await dl_monitor.CheckAuditByTransactionDone(id.ID.ToString());


                return Json(new { success = true, connid = TransactionID });
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
        [HttpPost]
        public async Task<JsonResult> InsertPridictiveCoauseAudit([FromBody] List<PredictiveEvaluationModel> sectionData)
        {


            if (sectionData == null || sectionData.Count == 0)
            {
                return Json(new { success = false, message = "No data received" });
            }
            var result = await dl_monitor.SubmitePridictiveEvaluation(sectionData);
            if (result == 1)
            {
                return Json(new { success = true, message = "Ok" });
            }
            else
            {
                return Json(new { success = false, message = "No data Insert" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> InsertRouutCoauseAudit([FromBody] List<RootCauseAnalysisModel> sectionData)
        {


            if (sectionData == null || sectionData.Count == 0)
            {
                return Json(new { success = false, message = "No data received" });
            }
            var result = await dl_monitor.SubmiteRouteCauseEvaluation(sectionData);
            if (result == 1)
            {
                return Json(new { success = true, message = "Ok" });
            }
            else
            {
                return Json(new { success = false, message = "No data Insert" });
            }
        }
        [HttpPost]
        public  async Task< JsonResult> InsertSectionAudit([FromBody]  List<SectionAuditModel> sectionData)
        {
            
          
            if (sectionData == null || sectionData.Count == 0)
            {
                return Json(new { success = false, message = "No data received" });
            }
            var result = await dl_monitor.SubmiteSectionEvaluation(sectionData);
            if (result == 1)
            {
                return Json(new { success = true, message = "Ok" });
            }
            else
            {
                return Json(new { success = false, message = "No data Insert" });
            }
        }


        [HttpPost]
        public  async Task  < IActionResult> FormMinitoringAuditr([FromBody] MonitorFormModel model)
        {
            var result= await dl_monitor.SubmiteFormEvaluation(model);
            if(result==1)
            {
                return Ok(new { sucesss =true, message = "Data received successfully" });
            }
            else
            {
                return Ok(new { sucesss = false,  message = "Data received successfully" });
            }
                
        }

            [HttpGet]
        public async Task<JsonResult> GetRCAValues()
        {
            DataSet ds = await dl_monitor.GetRCAVAluesDroppdawn();
            if (ds.Tables.Count == 3) 
            {
                DataTable rca1 = ds.Tables[0]; 
                DataTable rca2 = ds.Tables[1]; 
                DataTable rca3 = ds.Tables[2];
                var rca1List = rca1.AsEnumerable().Select(row => new { ID = row["ID"], RCA_Value = row["RCA_Value"] }).ToList();
                var rca2List = rca2.AsEnumerable().Select(row => new { ID = row["ID"], RCA_Value = row["RCA_Value"] }).ToList();
                var rca3List = rca3.AsEnumerable().Select(row => new { ID = row["ID"], RCA_Value = row["RCA_Value"] }).ToList();

                return Json(new { success = true, rca1 = rca1List, rca2 = rca2List, rca3 = rca3List });
            }
            else
                return Json(new { success = false, message = "fails." });

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
            DataTable LastTransaction = await _admin.GetLastTransactionListAsync();
            ViewBag.LastTransaction = LastTransaction;

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
