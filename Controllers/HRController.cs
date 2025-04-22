using Microsoft.AspNetCore.Mvc;
using QMS.DataBaseService;
using QMS.Models;
using System;
using System.Data;

namespace QMS.Controllers
{
    public class HRController : Controller
    {

        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly DL_Hr dl_hr;
        private readonly DL_Agent dl_Agent;
        private readonly DL_QaManager dl_qa;
        public HRController(DL_Hr dl_HRs, DL_QaManager dl_qas, DL_Agent adla)
        {

            dl_hr = dl_HRs;
            dl_qa = dl_qas;
            dl_Agent = adla;

        }

      

        public async Task<IActionResult> Dashboard()
        {


            List<ZtHrCase> ListHR = await dl_hr.ZtCaseHr();
            List<ZTcaseModel> ZTlistPanel = await dl_hr.ZtCasePanel();

            var viewModel = new ZtPanelHrCase
            {
                ZTcaseHrList = ListHR,
                PanelList = ZTlistPanel
            };

            return View(viewModel);
        }



        public async Task<IActionResult> HrZtCases(string TransactionID)
        {
            TempData["TtransactionID"] = TransactionID;

            ViewBag.TransactionID = TransactionID;

            DataTable dt = await dl_hr.GetPanelZtCaseViewDetails(TransactionID);
            DataSet dt12 = await dl_Agent.getCQScoreQADisputeSection(TransactionID);
            
            try
            {
                if (dt12.Tables[0].Rows.Count > 0)
                {
                    byte[] audioBytes = dt12.Tables[0].Rows[0]["AudioData"] as byte[];
                    if (audioBytes != null)
                    {
                        string base64Audio = Convert.ToBase64String(audioBytes);
                        ViewBag.AudioData = "data:audio/wav;base64," + base64Audio;
                    }
                }
            }
            catch (Exception ex)
            {

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
            string Ops_Manager_Comments = dt.Rows[0]["Ops_Manager_Comments"].ToString();
            string Panel_Comments = dt.Rows[0]["Panel_Comments"].ToString();
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
            ViewBag.Ops_Manager_Comments = Ops_Manager_Comments;
            ViewBag.Panel_Comments = Panel_Comments;

            return View();
        }


        public async Task<IActionResult> PanelCases(string TransactionID)
        {
            TempData["TtransactionID"] = TransactionID;

            ViewBag.TransactionID = TransactionID;

            DataTable dt = await dl_hr.GetPanelZtCaseViewDetails(TransactionID);
            DataSet dt12 = await dl_Agent.getCQScoreQADisputeSection(TransactionID);
            try
            {
                if (dt12.Tables[0].Rows.Count > 0)
                {
                    byte[] audioBytes = dt12.Tables[0].Rows[0]["AudioData"] as byte[];
                    if (audioBytes != null && audioBytes.Length > 0)
                    {
                        string base64Audio = Convert.ToBase64String(audioBytes);
                        ViewBag.AudioData = "data:audio/wav;base64," + base64Audio;
                    }
                }

            }
            catch (Exception ex)
            {

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
            string Ops_Manager_Comments = dt.Rows[0]["Ops_Manager_Comments"].ToString();
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
            ViewBag.Ops_Manager_Comments = Ops_Manager_Comments;

            return View();
        }



        [HttpPost]
        public async Task<IActionResult> SubmiPanelCommentsStatusApprove(string comment)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return Json(new { success = false, message = "Comment cannot be empty." });
            }
            else
            {
                string TransactionID = TempData["TtransactionID"].ToString();
                await dl_hr.SubmitePanelApprove(comment, TransactionID);
                return Json(new { success = true, message = "Panel Approved Successfully...." });
            }

        }


        [HttpPost]
        public async Task<IActionResult> SubmitPanelCommentsStatusReject(string comment)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return Json(new { success = false, message = "Comment cannot be empty." });
            }
            else
            {
                string TransactionID = TempData["TtransactionID"].ToString();
                await dl_hr.SubmitePanelReject(comment, TransactionID);
                return Json(new { success = true, message = "Panel Manager Reject Successfully...." });
            }

        }


        [HttpPost]
        public async Task<IActionResult> SubmiHRCommentsStatusApprove(string comment)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return Json(new { success = false, message = "Comment cannot be empty." });
            }
            else
            {
                string TransactionID = TempData["TtransactionID"].ToString();
                await dl_hr.SubmiteHRApprove(comment, TransactionID);
                return Json(new { success = true, message = "Data has been Submit Successfully...." });
            }

        }

    }
}
