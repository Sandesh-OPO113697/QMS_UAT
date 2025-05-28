using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Bcpg.Sig;
using QMS.DataBaseService;
using QMS.Models;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Transactions;

namespace QMS.Controllers
{

    public class QAManagerController : Controller
    {
        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly DL_QaManager dl_qa;
        private readonly DL_Agent dl_Agent;
        private readonly Dl_Admin _dlamin;
        public QAManagerController(DL_QaManager adl, DL_Agent adla, Dl_formBuilder dl_FormBuilder, Dl_Admin dlamin)
        {
            dl_qa = adl;
            dl_Agent = adla;
            this.dl_FormBuilder = dl_FormBuilder;
            _dlamin = dlamin;
        }
        [HttpPost]
        public async Task<IActionResult> SavePerformance([FromBody] PerformanceData data)
        {
           await   dl_qa.SubmiteAgentSurvey(data);
            return Ok(new { status = "success" });
        }
        public async Task<IActionResult> TransactionID([FromBody] DropDawnString id)
        {
            try
            {

                DataTable dt = await dl_qa.GetTransactionIDByUser(id.ID);
                var performanceList = new List<string>();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        performanceList.Add(Convert.ToString(row["TransactionID"]));
                    }
                }
                return Json(new { performance = performanceList });
            }
            catch (Exception ex)
            {
                return Json(new {test=""});
            }
        }

        public async Task<IActionResult> Survey()
        {
            DataTable dt = await _dlamin.GetProcessListAsync();
            var processList = dt.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.Process = processList;
           

            return View();
        }
        public async Task<IActionResult> ZtTriggerSignOff()
        {
            string locationid = UserInfo.LocationID;
            var data = await _dlamin.GetProcessListByLocation(locationid);
            var processList = data.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.ProcessList = processList;
          
            string Userid = UserInfo.UserName;
            string LocationID = UserInfo.LocationID;
           
            return View();

            
        }


        [HttpPost]
        public async Task<JsonResult> Agentacknowledements()
        {
          
            int result = await dl_qa.UpdateAgentacknowledements();
            return Json(new { success = true, message = "Agent acknowledements successfully!!!" });
        }



        [HttpPost]
        public async Task<JsonResult> InsertSectionFeilds([FromBody] ZtSignOffSectionDatas request)
        {
            if (request == null || request.sections == null || request.sections.Count == 0)
            {
                return Json(new { success = false, message = "No data received" });
            }

            try
            {
                int result = await dl_qa.InsertZtSignOff(request.sections);

                return result < 0
                    ? Json(new { success = true, message = "Data inserted successfully!" })
                    : Json(new { success = false, message = "Failed to insert data." });
            }
            catch (Exception ex)
            {
               
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }




        [HttpPost]
        public async Task<JsonResult> InsertSectionAudit([FromBody] List<SectionAuditModel> sectionData)
        {

            string TransactionID = TempData["TransactionID_Dispute"].ToString();
            if (sectionData == null || sectionData.Count == 0)
            {
                return Json(new { success = false, message = "No data received" });
            }
            var result = await dl_qa.UpdateSectionByQAEvaluation(sectionData, TransactionID);
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
        public async Task<IActionResult> SubmiteCochingComment(string AgentID, string ReviewDate, string Comment , string NumberOFReview)
        {
            await dl_qa.SubmiteCochingComment(AgentID , ReviewDate , Comment , NumberOFReview);
            return RedirectToAction("Dashboard");

        }


        [HttpPost]
        public async Task<IActionResult> SubmitDisputeFeedbackByQA(string comment, string calibration, string totalScore)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return Json(new { success = false, message = "Comment cannot be empty." });
            }
            else
            {
                string TransactionID = TempData["TransactionID_Dispute"].ToString();
                await dl_qa.SubmiteDisputeAkowedge(comment, calibration, TransactionID, totalScore);
                return Json(new { success = true, message = "Dispute has been Done...." });
            }


        }


        public async Task<IActionResult> SubmiteCoaching(string AgentID)
            {
            List<ReviewDataModel> coutingList = await dl_qa.GetCaoutingList();
            
            var data = coutingList.FirstOrDefault(x => x.AgentID == AgentID);
            var comment1 = data.Comment1;
            var comment2 = data.Comment2;
            var comment3 = data.Comment3;
            var comment4 = data.Comment4;
            var comment5 = data.Comment5;
            var comment6 = data.Comment6;
            var ReviewDate1 = data.FirstReview;
            var ReviewDate2 = data.SecondReview;
            var ReviewDate3 = data.ThirdReview;
            var ReviewDate4 = data.FourthReview;
            var ReviewDate5 = data.FifthReview;
            var ReviewDate6 = data.SixReview;
            ViewBag.AgentID = AgentID;
            if (comment1 == "")
            {
                ViewBag.NumberOfReview = 1;
                ViewBag.ReviewDate = ReviewDate1;
            }
            else if (comment2 == "")
            {
                ViewBag.NumberOfReview = 2;
                ViewBag.ReviewDate = ReviewDate1;
            }
            else if (comment3 == "")
            {
                ViewBag.NumberOfReview = 3;
                ViewBag.ReviewDate = ReviewDate1;
            }
            else if (comment4 == "")
            {
                ViewBag.NumberOfReview = 4;
                ViewBag.ReviewDate = ReviewDate1;
            }
            else if (comment5 == "" && ReviewDate5 != "")
            {
                ViewBag.NumberOfReview = 5;
                ViewBag.ReviewDate = ReviewDate5;
            }
            else if (comment6 == "" && ReviewDate5 != "")
            {
                ViewBag.NumberOfReview = 6;
                ViewBag.ReviewDate = ReviewDate6;
            }
            else
            {
                ViewBag.NumberOfReview = 0;
                ViewBag.ReviewDate = "Null";

            }
                List<MatrixAllDetails> ListCoutingMatrix = await dl_qa.GetMatrixList(AgentID);

            return View( ListCoutingMatrix);
            
        }

        public async Task<IActionResult> Dashboard()
        {

            try
            {
                DataTable assment = await dl_Agent.GetAssesment();
                List<DisputeCallfeedbackModel> List = await dl_qa.DisputeAgentFeedback();
                List<ZTcaseModel> ZTlist = await dl_qa.ZtcaseShow();
                List<ReviewDataModel> coutingList = await dl_qa.GetCaoutingList();

                List<AgentToQASurveyModel> AgentToQASurveylist = await dl_qa.AgentToQASurveylist();

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

                var viewModel = new DisputeFeedbackViewModel
                {
                    DisputeList = List,
                    ZTcaseList = ZTlist,
                    ReviewDataModel = coutingList,
                    assmentonl = assmentonl,
                    AgentToQASurvey= AgentToQASurveylist
                };

                return View(viewModel);
            }
            catch(Exception ex)
            {
                return View();
            }
           
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

        public async Task<IActionResult> CallibrationDetailsTransactionId()
        {

            DataTable dt = await dl_qa.CallibrationBypaticipatesByUserID();
          
            return View(dt);
        }

        public async Task<IActionResult> CallibrationDetails(string transactionId)
        {

            DataTable dt = await dl_qa.CallibrationBypaticipates(transactionId);
            var model = TransformDataTableToViewModel(dt);
            return View(model);
        }
        private List<CalibrationRowViewModel> TransformDataTableToViewModel(DataTable dt)
        {
            var model = new List<CalibrationRowViewModel>();

            if (dt.Rows.Count == 0)
                return model;
            var participantNames = dt.Columns.Cast<DataColumn>()
                .Where(c => c.ColumnName.StartsWith("QA_rating", StringComparison.OrdinalIgnoreCase) && c.ColumnName != "QA_rating")
                .Select(c => c.ColumnName.Replace("QA_rating", "").Trim())
                .ToList();

            foreach (DataRow row in dt.Rows)
            {
                var rowVM = new CalibrationRowViewModel
                {
                    Category = row["category"].ToString(),
                    Level = Convert.ToInt32(row["level"]),
                    SectionName = row["SectionName"].ToString(),
                };
                foreach (var name in participantNames.Prepend("")) 
                {
                  string CreatedBy=  row[$"CreatedBy{name}"].ToString();
                    var key = string.IsNullOrEmpty(name) ? "Master" : CreatedBy;

                    rowVM.ParticipantData[key] = new CalibrationParticipantData
                    {
                        QA_rating = row[$"QA_rating{name}"].ToString(),
                        Scorable = row[$"Scorable{name}"].ToString(),
                        Weightage = row[$"Weightage{name}"].ToString(),
                        Fatal = row[$"Fatal{name}"].ToString()
                    };
                    if (!string.IsNullOrEmpty(name))
                    {
                        int Count = 1 + Convert.ToInt32(name);
                        rowVM.ParticipantData[key].Variance = row[$"r{Count.ToString()}Variance"].ToString();
                    }
                }
                model.Add(rowVM);
            }

            return model;
        }
        public async Task<IActionResult> EditAgentFeedBack(string TransactionID)
        {
            try
            {
                TempData["TransactionID_Dispute"] = TransactionID;

                ViewBag.TransactionID = TransactionID;
                DataTable dt1 = await dl_Agent.getPrrocessAndSubProcess(TransactionID);
                string processID = dt1.Rows[0]["ProgramID"].ToString();
                string SUBprocessID = dt1.Rows[0]["SubProgramID"].ToString();
                ViewBag.Agent_Comment = dt1.Rows[0]["Agent_Comment"].ToString();
                ViewBag.Remarks = dt1.Rows[0]["Remarks"].ToString();
                var dataTable = await dl_qa.GetMonitporedSectionGriedAsync(Convert.ToInt32(processID), Convert.ToInt32(SUBprocessID), TransactionID);
                var sectionList = dataTable.AsEnumerable().Select(row => new MonitoredSectionGridModel
                {

                    category = row.Field<string>("category"),
                    level = row.Field<string>("level"),
                    QA_rating = row.Field<string>("QA_rating"),
                    SectionName = row.Field<string>("SectionName"),
                    Scorable = row.Field<string>("Scorable"),
                    Weightage = row.Field<string>("Weightage"),
                    Commentssection = row.Field<string>("Commentssection"),
                    Fatal = row.Field<string>("Fatal")


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
            catch (Exception ex)
            {
                return View();
            }
            
            

        }
        public async Task<IActionResult> ZeroTolerance(string TransactionID)
        {
            ViewBag.TransactionID = TransactionID;
            TempData["TtransactionID"] = TransactionID;
            DataTable dt = await dl_qa.GetQaManagerZtCaseViewDetails(TransactionID);
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
            catch(Exception ec)
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


            return View();
        }



        [HttpPost]
        public async Task<IActionResult> SubmiQaManagerStatusApprove(string comment, string ZTHistory)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return Json(new { success = false, message = "Comment cannot be empty." });
            }
            else
            {
                string TransactionID = TempData["TtransactionID"].ToString();
                await dl_qa.SubmiteQaManagerApprove(comment, ZTHistory, TransactionID);
                return Json(new { success = true, message = "QA Manager Approved Successfully...." });
            }

        }


        [HttpPost]
        public async Task<IActionResult> SubmiQaManagerStatusReject(string comment, string ZTHistory)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return Json(new { success = false, message = "Comment cannot be empty." });
            }
            else
            {
                string TransactionID = TempData["TtransactionDisputeID"].ToString();
                await dl_qa.SubmiteQaManagerReject(comment, ZTHistory, TransactionID);
                return Json(new { success = true, message = "QA Manager Reject Successfully...." });
            }

        }


      


        public async Task<IActionResult> AgentSurveyView(string TransactionID)
        {
            TempData["TtransactionID"] = TransactionID;
            try
            {
                List<AgentToQASurveyModel> AgentToQASurveylists = await dl_qa.AgentToQASurveylistView(TransactionID);

                ViewBag.Transaction_ID = TransactionID;

                if (AgentToQASurveylists?.Any() == true)
                {
                    ViewBag.AgentComment = AgentToQASurveylists.First().AgentComment;
                }
                else
                {
                    ViewBag.AgentComment = "No comments available."; 
                }

                var viewModel = new DisputeFeedbackViewModel
                {
                 
                    AgentToQASurvey = AgentToQASurveylists
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return View();
            }

        }

    }


}
