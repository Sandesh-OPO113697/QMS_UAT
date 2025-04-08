using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;
using System.Threading.Tasks;

namespace QMS.Controllers
{

    public class QAManagerController : Controller
    {
        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly DL_QaManager dl_qa;
        private readonly DL_Agent dl_Agent;
        public QAManagerController(DL_QaManager adl, DL_Agent adla, Dl_formBuilder dl_FormBuilder)
        {
            dl_qa = adl;
            dl_Agent = adla;
            this.dl_FormBuilder = dl_FormBuilder;
        }

        [HttpPost]
        public async Task<JsonResult> InsertSectionAudit([FromBody] List<SectionAuditModel> sectionData)
        {

            string TransactionID = TempData["TransactionID_Dispute"].ToString();
            if (sectionData == null || sectionData.Count == 0)
            {
                return Json(new { success = false, message = "No data received" });
            }
            var result = await dl_qa.UpdateSectionByQAEvaluation(sectionData , TransactionID);
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
        public async Task<IActionResult> SubmitDisputeFeedbackByQA(string comment, string calibration , string totalScore)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return Json(new { success = false, message = "Comment cannot be empty." });
            }
            else
            {
                string TransactionID = TempData["TransactionID_Dispute"].ToString();
                await dl_qa.SubmiteDisputeAkowedge(comment, calibration ,  TransactionID , totalScore);
                return Json(new { success = true, message = "Dispute has been Done...." });
            }


        }

        public async Task<IActionResult> Dashboard()
        {
            List<DisputeCallfeedbackModel> List= await dl_qa.DisputeAgentFeedback();
            return View(List);
        }

        public async Task<IActionResult> EditAgentFeedBack(string TransactionID)
        {
            TempData["TransactionID_Dispute"] = TransactionID;
            ViewBag.TransactionID = TransactionID;
            DataTable dt1 = await dl_Agent.getPrrocessAndSubProcess(TransactionID);
            string processID = dt1.Rows[0]["ProgramID"].ToString();
            string SUBprocessID = dt1.Rows[0]["SubProgramID"].ToString();
          ViewBag.Agent_Comment = dt1.Rows[0]["Agent_Comment"].ToString();
            var dataTable = await dl_qa.GetMonitporedSectionGriedAsync(Convert.ToInt32(processID), Convert.ToInt32(SUBprocessID));
            var sectionList = dataTable.AsEnumerable().Select(row => new MonitoredSectionGridModel
            {

                category = row.Field<string>("category"),
                level = row.Field<string>("level"),
                QA_rating = row.Field<string>("QA_rating"),
                SectionName = row.Field<string>("SectionName"),
                Scorable = row.Field<string>("Scorable"),
                Weightage = row.Field<string>("Weightage"),
                Commentssection = row.Field<string>("Commentssection")

            
            }).ToList();



            DataTable DynamicFeild = await dl_FormBuilder.GetDynamicGriedAsync(Convert.ToInt32(processID), Convert.ToInt32(SUBprocessID));

            List<SelectListItem> filteredRatingList = new List<SelectListItem>();
            var Rating = DynamicFeild.Rows[0]["Rating"].ToString().Split(',').Select(s => s.Trim()).ToList();
            DataSet ds = await dl_FormBuilder.GetSectionDropdownDataAsync();
            DataTable ratingTableData = ds.Tables[1];
            var ratingTable = ratingTableData.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["Id"].ToString(),
                Text = $"{row["RatingName"]}"
            }).ToList();
            filteredRatingList = ratingTable.Where(item => Rating.Contains(item.Value)).ToList();
            var viewModel = new AgentFeedBackSectionList
            {
                sectionList = sectionList,
                filteredRatingList = filteredRatingList
            };

            return View(viewModel);

        }
    }
}
