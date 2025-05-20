using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.SS.Formula.Functions;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;

namespace QMS.Controllers
{
    public class OperationController : Controller
    {
        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly DL_Operation dl_Ops;
        private readonly DL_Agent dl_Agent;
        private readonly DL_QaManager dl_qa;
        private readonly Dl_Admin dl_admin;
   
        public OperationController(DL_Operation adls, DL_QaManager dl_qas, DL_Agent adla, Dl_Admin dl_admin )
        {

            dl_Ops = adls;
            dl_qa = dl_qas;
            dl_Agent = adla;
            this.dl_admin = dl_admin;
        }
        [HttpPost]
        public async Task<ActionResult> SubmitTest(AttemptTestViewModel model)
        {

            string TestName = model.TestName;
            DataTable dt = await dl_Agent.Submiteassesment(model);

            int totalQuestions = dt.Rows.Count;
            int correctAnswers = dt.AsEnumerable()
                                   .Count(row => Convert.ToBoolean(row["IsCorrect"]));
            double scorePercentage = totalQuestions > 0
                ? Math.Round((double)correctAnswers / totalQuestions * 100, 2)
                : 0;

            // Pass the result data to view using a view model
            var resultViewModel = new TestResultViewModel
            {
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctAnswers,
                ScorePercentage = scorePercentage
            };


            return View(resultViewModel);
        }
        public async Task<IActionResult> AttempAssesment(int TestID)
        {
            var model = await dl_Agent.AttempTest(TestID);

            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> UploadAPRDEtails(string SUBProgramID, string ProgramID, IFormFile files)
        {
            await dl_qa.UploadAPR(ProgramID, SUBProgramID, files);
            TempData["SucessAPR"] = "APR Upload Sucessfull...!";

            return RedirectToAction("UploadAPR");
        }
        public async Task<IActionResult> Dashboard()
        {

            DataTable assment = await dl_Agent.GetAssesment();

            List<ZTcaseModel> ZTlist = await dl_Ops.ZtcaseShow();

            List<Calibration_ViewModel> Calibration = await dl_Ops.Participants_View();

            List<AssesmentModel> assmentonl = assment.AsEnumerable().Select(row => new AssesmentModel
            {
                TestID = row.Field<int>("TestID"),
                TestName = row.Field<string>("TestName"),
                TestCategory = row.Field<string>("TestCategory"),
                CreatedDate = row.Field<DateTime>("CreatedDate"),
                expiryType = row.Field<string>("expiryType"),
                expiryDate = row.Field<DateTime>("expiryDate"),
                expiryHours = row.Field<int>("expiryHours")

            }).ToList();


            var viewModel = new OperationViewModel
            {
                
                ZTcaseList = ZTlist,
                Calibration= Calibration,
                  assmentonl = assmentonl
            };

            return View(viewModel);
        }

        public async Task<IActionResult> UploadAPR()
        {

            DataTable dt = await dl_admin.GetProcessListAsync();
            var processList = dt.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.Process = processList;

            return View();
        }
        public async Task<IActionResult> MonitorCalibration(string TransactionID)
        {

            return View();
        }
        [HttpPost]
        public async Task<JsonResult> InsertSectionAudit([FromBody] List<OperationCallibration> sectionData)
        {


            if (sectionData == null || sectionData.Count == 0)
            {
                return Json(new { success = false, message = "No data received" });
            }
            var result = await dl_Ops.SubmiteSectionEvaluation(sectionData);
            if (result == 1)
            {
                return Json(new { success = true, message = "Ok" });
            }
            else
            {
                return Json(new { success = false, message = "No data Insert" });
            }
        }
        public async Task<IActionResult> Participants_Calibration_View(string TransactionID , string Process ,string subProcess)
        {

            ViewBag.TransactionID = TransactionID;
            ViewBag.Process = Process;
            ViewBag.subProcess = subProcess;
            return View();
        }

        public async Task<IActionResult> OperationZtCase(string TransactionID)
        {
            TempData["TtransactionID"] = TransactionID;

            ViewBag.TransactionID = TransactionID;

            DataTable dt = await dl_qa.GetQaManagerZtCaseViewDetails(TransactionID);
            DataSet dt12 = await dl_Agent.getCQScoreQADisputeSection(TransactionID);

            if (dt12.Tables[0].Rows.Count > 0)
            {
                byte[] audioBytes = dt12.Tables[0].Rows[0]["AudioData"] as byte[];
                if (audioBytes != null)
                {
                    string base64Audio = Convert.ToBase64String(audioBytes);
                    ViewBag.AudioData = "data:audio/wav;base64," + base64Audio;
                }
            }



            string ProgramID = dt.Rows[0]["ProgramID"].ToString();
            string SubProgramID = dt.Rows[0]["SubProgramID"].ToString();
            string AgentName = dt.Rows[0]["AgentName"].ToString();
            string EmployeeID = dt.Rows[0]["EmployeeID"].ToString();
            string AgentSupervsor = dt.Rows[0]["AgentSupervsor"].ToString();
            string ZTRaisedBy = dt.Rows[0]["ZTRaisedBy"].ToString();
            string ZTRaisedDate = dt.Rows[0]["ZTRaisedDate"].ToString();
            string TransactionDate = dt.Rows[0]["TransactionDate"].ToString();
            string Zt_History = dt.Rows[0]["Zt_History"].ToString();
            string QA_Manager_Comments = dt.Rows[0]["QA_Manager_Comments"].ToString();
            string ZTClassification = dt.Rows[0]["ZTClassification"].ToString();
            ViewBag.ProgramID = ProgramID;
            ViewBag.SubProgramID = SubProgramID;
            ViewBag.AgentName = AgentName;
            ViewBag.EmployeeID = EmployeeID;
            ViewBag.AgentSupervsor = AgentSupervsor;
            ViewBag.ZTRaisedBy = ZTRaisedBy;
            ViewBag.ZTRaisedDate = ZTRaisedDate;
            ViewBag.TransactionDate = TransactionDate;
            ViewBag.ZTClassification = ZTClassification;
            ViewBag.Zt_History = Zt_History;
            ViewBag.QA_Manager_Comments = QA_Manager_Comments;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> SubmiOperationManagerStatusApprove(string comment)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return Json(new { success = false, message = "Comment cannot be empty." });
            }
            else
            {
                string TransactionID = TempData["TtransactionID"].ToString();
                await dl_Ops.SubmiteOperationManagerApprove(comment, TransactionID);
                return Json(new { success = true, message = "Opration Manager Approved Successfully...." });
            }

        }


        [HttpPost]
        public async Task<IActionResult> SubmiOperationManagerStatusReject(string comment)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return Json(new { success = false, message = "Comment cannot be empty." });
            }
            else
            {
                string TransactionID = TempData["TtransactionID"].ToString();
                await dl_Ops.SubmiteOperationManagerReject(comment, TransactionID);
                return Json(new { success = true, message = "Opration Manager Reject Successfully...." });
            }

        }
    }
}
