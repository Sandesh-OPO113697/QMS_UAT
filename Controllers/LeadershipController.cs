using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;

namespace QMS.Controllers
{
    public class LeadershipController : Controller
    {
        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly DL_Hr dl_hr;
        private readonly DL_Agent dl_Agent;
        private readonly DL_QaManager dl_qa;
        private readonly Dl_Admin _dlamin;
        public LeadershipController(DL_Hr dl_HRs, DL_QaManager dl_qas, DL_Agent adla, Dl_Admin a)
        {

            dl_hr = dl_HRs;
            dl_qa = dl_qas;
            dl_Agent = adla;
            _dlamin = a;

        }



        public async Task<IActionResult> Dashboard()
        {
            DataTable dt = await _dlamin.GetProcessListAsync();

            var processList = dt.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            processList.Insert(0, new SelectListItem
            {
                Value = "ALL", // or "" depending on your requirement
                Text = "ALL"
            });
            ViewBag.Process = processList;

            List<ZtHrCase> ListHR = await dl_hr.ZtCaseHr();
            List<ZTcaseModel> ZTlistPanel = await dl_hr.ZtCasePanel();
            List<CouchingPIP> HRPIP = await dl_hr.CouchingPIP();

            var viewModel = new ZtPanelHrCase
            {
                ZTcaseHrList = ListHR,
                PanelList = ZTlistPanel,
                HRPIP = HRPIP
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Getdashboad([FromBody] DashboardFilterModel model)
        {
            try
            {

                List<ZtHrCase> ListHR = await dl_hr.ZtCaseHr();
                List<ZTcaseModel> ZTlistPanel = await dl_hr.ZtCasePanel();
                List<CouchingPIP> HRPIP = await dl_hr.CouchingPIP();





                IEnumerable<ZtHrCase> baseListZtHrCase;
                IEnumerable<ZTcaseModel> baseListZTcaseModel;
                IEnumerable<CouchingPIP> baseListCouchingPIP;


                if (model.Program == "ALL")
                {
                    baseListZtHrCase = ListHR;
                    baseListZTcaseModel = ZTlistPanel;
                    baseListCouchingPIP = HRPIP;
                }
                else if (model.SubProgram == "ALL")
                {
                    baseListZtHrCase = ListHR.Where(x => x.ProgramID == model.Program);
                    baseListZTcaseModel = ZTlistPanel.Where(x => x.ProgramID == model.Program);
                    baseListCouchingPIP = HRPIP.Where(x => x.Program == model.Program);
                }
                else
                {

                    baseListZtHrCase = ListHR.Where(x => x.ProgramID == model.Program && x.SubProgramID == model.SubProgram);
                    baseListZTcaseModel = ZTlistPanel.Where(x => x.ProgramID == model.Program && x.SubProgramID == model.SubProgram);
                    baseListCouchingPIP = HRPIP.Where(x => x.Program == model.Program && x.SubProgram == model.SubProgram);
                }

                DateTime today = DateTime.Today;
                List<Dictionary<string, object>> baseListTransaction = new();


                switch (model.Filter?.ToLower())
                {
                    case "day":

                        baseListZtHrCase = baseListZtHrCase
                            .Where(x => DateTime.TryParse(x.TransactionDate, out var date) && date.Date == today)
                            .ToList();
                        baseListZTcaseModel = baseListZTcaseModel
                            .Where(x => DateTime.TryParse(x.TransactionDate, out var date) && date.Date == today)
                            .ToList();
                        baseListCouchingPIP = baseListCouchingPIP
                            .Where(x => DateTime.TryParse(x.Createddate, out var date) && date.Date == today)
                            .ToList();


                        break;

                    case "week":
                        var startOfWeek = today.AddDays(-(int)today.DayOfWeek); // Sunday
                        var endOfWeek = startOfWeek.AddDays(6);

                        baseListZtHrCase = baseListZtHrCase
                            .Where(x => DateTime.TryParse(x.TransactionDate, out var date) &&
                                        date.Date >= startOfWeek && date.Date <= endOfWeek)
                            .ToList();

                        baseListZTcaseModel = baseListZTcaseModel
                            .Where(x => DateTime.TryParse(x.ZTRaisedDate, out var date) &&
                                        date.Date >= startOfWeek && date.Date <= endOfWeek)
                            .ToList();

                        baseListCouchingPIP = baseListCouchingPIP
                            .Where(x => DateTime.TryParse(x.Createddate, out var date) &&
                                        date.Date >= startOfWeek && date.Date <= endOfWeek)
                            .ToList();
                        break;

                    case "month":
                        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                        baseListZtHrCase = baseListZtHrCase
                            .Where(x => DateTime.TryParse(x.TransactionDate, out var date) &&
                                        date.Date >= firstDayOfMonth && date.Date <= lastDayOfMonth)
                            .ToList();

                        baseListZTcaseModel = baseListZTcaseModel
                            .Where(x => DateTime.TryParse(x.ZTRaisedDate, out var date) &&
                                        date.Date >= firstDayOfMonth && date.Date <= lastDayOfMonth)
                            .ToList();

                        baseListCouchingPIP = baseListCouchingPIP
                            .Where(x => DateTime.TryParse(x.Createddate, out var date) &&
                                        date.Date >= firstDayOfMonth && date.Date <= lastDayOfMonth)
                            .ToList();
                        break;

                    default:

                        baseListTransaction = baseListTransaction;
                        ;
                        break;
                }


                return Json(new
                {

                    baseListCouchingPIP = baseListCouchingPIP,
                    baseListZtHrCase = baseListZtHrCase,
                    baseListZTcaseModel = baseListZTcaseModel,


                });
            }
            catch (Exception ex)
            {

                return StatusCode(500, "An error occurred while processing the request.");
            }
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
