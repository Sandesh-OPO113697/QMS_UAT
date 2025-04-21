using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.POIFS.Crypt.Dsig;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;

namespace QMS.Controllers
{
    public class CoachingController : Controller
    {
        private readonly Dl_Coaching dl_coching;
        private readonly Dl_Admin _admin;
        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly dl_Monitoring dl_monitor;
        private readonly Dl_UpdateManagement dl_udm;
        public CoachingController(Dl_UpdateManagement adpt, Dl_formBuilder adl, Dl_Coaching dl, Dl_Admin adam, dl_Monitoring dmmonoi)
        {
            dl_coching = dl;
            _admin = adam;
            dl_FormBuilder = adl;
            dl_monitor = dmmonoi;
            dl_udm = adpt;
        }
        [HttpPost]
        public async Task< JsonResult> SubmitCoachingData([FromBody] CoachingSubmissionModel submission)
        {
            if (submission.FormData == null || submission.MetricsJson == null)
            {
               
                return Json(new { success = false, message = "Invalid data received" });
            }
            else
            {
                await dl_coching.SubmitCountingAsync(submission.MetricsJson, submission.FormData);
                return Json(new { success = true, message = "Data saved successfully!" });
            }


       
        }

        public async Task<IActionResult> GetActualPerformanceList([FromBody] DropDawnString id)
        {
            try
            {
                DataTable dt = await dl_coching.GetActualPerformanceList(id.ID);

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];

                    var performanceList = new List<int>
            {
                Convert.ToInt32(row["C-SAT"]),
                Convert.ToInt32(row["NPS"]),
                Convert.ToInt32(row["FCR"]),
                Convert.ToInt32(row["Repeat"]),
                Convert.ToInt32(row["AHT"]),
                Convert.ToInt32(row["Sales Conversion"]),
                Convert.ToInt32(row["Resolution"])
            };

                    return Json(new { performance = performanceList });
                }

                return Json(new { performance = new List<int>() });
            }
            catch (Exception ex)
            {
                // This helps during debugging
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        public async Task<IActionResult> getQaManagertList([FromBody] Process_SUbProcess id)
        {
            List<object> list = await dl_coching.GetQaManagerList(id.ProcessID, id.SUBProcessID);

            return Json(new { agentTlList = list });
        }

        public async Task<IActionResult> getMatrix([FromBody] Process_SUbProcess id)
        {
            List<object> list = await dl_coching.GetMatrixList(id.ProcessID, id.SUBProcessID);

            return Json(new { agentTlList = list });
        }
        public async Task< IActionResult> Dashboard()
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
