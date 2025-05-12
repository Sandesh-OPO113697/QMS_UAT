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
        private readonly DL_QaManager dl_qa;
        public CoachingController(Dl_UpdateManagement adpt, Dl_formBuilder adl, Dl_Coaching dl, Dl_Admin adam, dl_Monitoring dmmonoi , DL_QaManager dqlqac)
        {
            dl_coching = dl;
            _admin = adam;
            dl_FormBuilder = adl;
            dl_monitor = dmmonoi;
            dl_qa = dqlqac;
        }

        public async Task<IActionResult> IsCouchingExtend([FromBody] DropDawnString id)
        {
            try
            {
                string Lastdate = string.Empty;
                DataTable dt = await dl_coching.CheckCouchingExtendOrnot(id.ID);
               
                if(dt.Rows.Count>0)
                {
                    Lastdate = dt.Rows[0]["R_date_4"].ToString();
                    return Json(new { performance = "extend" , fourthDate= Lastdate });
                }
                else
                {
                    return Json(new { performance = "Notextend" });
                }

                
            }
            catch (Exception ex)
            {
                // This helps during debugging
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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
                var Review1 = submission.FormData.Review1;
                var Review2 = submission.FormData.Review2;
                var Review3 = submission.FormData.Review3;
                var Review4 = submission.FormData.Review4;
                var Review5 = submission.FormData.Review5;
                var Review6 = submission.FormData.Review6;
                if(string.IsNullOrEmpty(Review1) && string.IsNullOrEmpty(Review2) && string.IsNullOrEmpty(Review3) && string.IsNullOrEmpty(Review4) && !string.IsNullOrEmpty(Review5) && !string.IsNullOrEmpty(Review6))
                {
                    await dl_coching.SubmitExtendedCountingAsync(submission.MetricsJson, submission.FormData);
                }
                else
                {
                    await dl_coching.SubmitCountingAsync(submission.MetricsJson, submission.FormData);
                }
               
                return Json(new { success = true, message = "Data saved successfully!" });
            }


       
        }
        public async Task<IActionResult> GetCochingPlanList([FromBody] DropDawnString id)
        {

            DataTable dt = await dl_coching.GetCoutingPlanDetailsList(id.ID);

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];

                var performanceList = new List<string>
            {
                Convert.ToString(row["AgentID"]),
                Convert.ToString(row["R_date_1"]),
                Convert.ToString(row["Comment_1"]),
                Convert.ToString(row["R_date_2"]),
                Convert.ToString(row["Comment_2"]),
                Convert.ToString(row["R_date_3"]),
                Convert.ToString(row["Comment_3"]),
                Convert.ToString(row["R_date_4"]),
                Convert.ToString(row["Comment_4"]),
                 Convert.ToString(row["R_date_5"]),
                Convert.ToString(row["Comment_5"]),
                 Convert.ToString(row["R_date_6"]),
                Convert.ToString(row["Comment_6"]),
                Convert.ToString(row["CreatedBy"]),
                Convert.ToString(row["Createddate"])
            };

                return Json(new { performance = performanceList });
            }

            return Json(new { performance = new List<int>() });
        }

        public async Task<IActionResult> GetActualPerformanceList([FromBody] DropDawnString id)
        {
            try
            {
            
                DataTable dt = await dl_coching.GetActualPerformanceList(id.ID);
                var performanceList = new List<int>();

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];

                    performanceList = new List<int>
            {
                Convert.ToInt32(row["C-SAT"]),
                Convert.ToInt32(row["NPS"]),
                Convert.ToInt32(row["FCR"]),
                Convert.ToInt32(row["Repeat"]),
                Convert.ToInt32(row["AHT"]),
                Convert.ToInt32(row["Sales Conversion"]),
                Convert.ToInt32(row["Resolution"])
            };
                }

               
                DataTable Matrix = await dl_coching.GetSelectedMatrixList(id.ID);
                var MatrixList = new List<int>();

                if (Matrix.Rows.Count > 0)
                {
                    var row = Matrix.Rows[0];

                    MatrixList = new List<int>
            {
                Convert.ToInt32(row["C-SAT"]),
                Convert.ToInt32(row["NPS"]),
                Convert.ToInt32(row["FCR"]),
                Convert.ToInt32(row["Repeat"]),
                Convert.ToInt32(row["AHT"]),
                Convert.ToInt32(row["Sales Conversion"]),
                Convert.ToInt32(row["Resolution"])
            };
                }

               
                return Json(new { performance = performanceList, matrixList = MatrixList });
            }
            catch (Exception ex)
            {
          
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public async Task<IActionResult> getQaManagertList([FromBody] Process_SUbProcess id)
        {
            List<object> list = await dl_coching.GetQaManagerList(id.ProcessID, id.SUBProcessID);

            return Json(new { agentTlList = list });
        }

        public async Task<IActionResult> getQaManagertAnTlListList([FromBody] Process_SUbProcess id)
        {
            List<object> list = await dl_coching.GetQaManagerAndteamLeaderList(id.ProcessID, id.SUBProcessID);

            return Json(new { agentTlList = list });
        }
        public async Task<IActionResult> getTlAndAgentList([FromBody] Process_SUbProcess id)
        {
            List<object> list = await dl_coching.GetTLAndAgentList(id.ProcessID, id.SUBProcessID);

            return Json(new { agentTlList = list });
        }
        public async Task<IActionResult> getMatrix([FromBody] Process_SUbProcess id)
        {
            List<object> list = await dl_coching.GetMatrixList(id.ProcessID, id.SUBProcessID);

            return Json(new { agentTlList = list });
        }
        public async Task<IActionResult> getMatrixByProcess([FromBody] ProcessSubProcessName id)
        {
            List<object> list = await dl_coching.GetMatrixByProcessList(id.ProcessID, id.SUBProcessID);

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
            List<ReviewDataModel> coutingList = await dl_qa.GetCaoutingExtendedList();

            return View(coutingList);
        }
        public async Task<IActionResult> getAndAgentListQuatile([FromBody] Process_SUbProcess id)
        {
            List<object> list = await dl_coching.GetTLAndAgentListUatile(id.ProcessID, id.SUBProcessID);

            return Json(new { agentTlList = list });
        }
    }
}
