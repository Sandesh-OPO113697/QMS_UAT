using Microsoft.AspNetCore.Mvc;
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
        public OperationController(DL_Operation adls, DL_QaManager dl_qas, DL_Agent adla)
        {

            dl_Ops = adls;
            dl_qa = dl_qas;
            dl_Agent = adla;

        }

        public async Task<IActionResult> Dashboard()
        {


           
            List<ZTcaseModel> ZTlist = await dl_Ops.ZtcaseShow();

            var viewModel = new DisputeFeedbackViewModel
            {
                
                ZTcaseList = ZTlist
            };

            return View(viewModel);
        }


        public async Task<IActionResult> OperationZtCase(string TransactionID)
        {
            TempData["TtransactionID"] = TransactionID;



            DataTable dt = await dl_qa.GetQaManagerZtCaseViewDetails(TransactionID);
            DataTable dt12 = await dl_Agent.getCQScoreQADisputeSection(TransactionID);
            byte[] audioBytes = dt12.Rows[0]["AudioData"] as byte[];
            if (audioBytes != null)
            {
                string base64Audio = Convert.ToBase64String(audioBytes);
                ViewBag.AudioData = "data:audio/wav;base64," + base64Audio;
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
