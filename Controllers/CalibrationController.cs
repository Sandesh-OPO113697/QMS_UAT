using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.SS.Formula.Functions;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;

namespace QMS.Controllers
{
    public class CalibrationController : Controller
    {


        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly DL_Hr dl_HR;
        private readonly DL_Agent dl_Agent;
        private readonly DL_QaManager dl_qa;
        private readonly dl_Calibration dl_qcouch;

        private readonly Dl_Admin _admin;
        public CalibrationController(DL_Hr dl_HRs, DL_QaManager dl_qas, DL_Agent adla, dl_Calibration dl_couch , Dl_Admin adl )
        {
             this.dl_HR = dl_HRs;
            dl_qa = dl_qas;
            dl_Agent = adla;
            dl_qcouch = dl_couch;
            _admin = adl;
        }
        [HttpPost]
        public async Task<JsonResult> InsertSectionAudit([FromBody] List<SectionAuditModel> sectionData)
        {


            if (sectionData == null || sectionData.Count == 0)
            {
                return Json(new { success = false, message = "No data received" });
            }
            var result = await dl_qcouch.SubmiteSectionEvaluation(sectionData);
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
        public async Task<JsonResult> GetRecListBydate([FromBody] RecApiModel request)
        {
            if (request == null || string.IsNullOrEmpty(request.AgentId) || string.IsNullOrEmpty(request.FromDate) || string.IsNullOrEmpty(request.ToDate))
            {
                return Json(new { success = false, message = "Invalid input data." });
            }
            else
            {
                var reclist = await dl_qcouch.GetRecListByAPi(request.FromDate, request.ToDate, request.AgentId);


                return Json(new { success = true, message = "sucesss.", reclist = reclist });
            }

        }
        [HttpPost]
        public async Task< IActionResult> SubmiteCalibration([FromBody] CalibratorModel model)
        {
             await dl_qcouch.SubmiteCalibrationDetails(model.ProgramId , model.SubProgram , model.transactionID , model.CalibratedComment ,  model.SelectedParticipants);
            return Ok(new { message = "Data received successfully" });
        }

        public async Task<IActionResult> Dashboard()
        {
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
