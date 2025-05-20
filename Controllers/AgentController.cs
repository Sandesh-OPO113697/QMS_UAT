using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace QMS.Controllers
{
    public class AgentController : Controller
    {
        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly DL_Agent dl_Agent;
        public AgentController(DL_Agent adl, Dl_formBuilder dl_FormBuilder)
        {
            dl_Agent = adl;
            this.dl_FormBuilder = dl_FormBuilder;
        }
        [HttpPost]
        public async Task< ActionResult> SubmitFeedbackAgentSurvey(FeedbackViewModel model)
        {
            await dl_Agent.SubmiteAgentSurvey(model);
            return RedirectToAction("Dashboard");
        }
            [HttpPost]
        public async Task< IActionResult> SubmitFeedback(string comment)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return Json(new { success = false, message = "Comment cannot be empty." });
            }
            else
            {
                string TransactionID = TempData["TtransactionID"].ToString();
                await dl_Agent.SubmiteAgentAkowedge(comment, TransactionID);
                return Json(new { success = true, message = "Acknowlegde has been Done...." });
            }

        }

        [HttpPost]
        public async Task<IActionResult> SubmitDisputeFeedback(string comment)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return Json(new { success = false, message = "Comment cannot be empty." });
            }
            else
            {
                string TransactionID = TempData["TtransactionDisputeID"].ToString();
                await dl_Agent.SubmiteDisputeAgentAkowedge(comment, TransactionID);
                return Json(new { success = true, message = "Acknowlegde has been Done...." });
            }

        }

        [HttpPost]
        public async Task<IActionResult> DisputeFeedback(string comment)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return Json(new { success = false, message = "Comment cannot be empty." });
            }
            else
            {
                string TransactionID = TempData["TtransactionID"].ToString();
                await dl_Agent.DisputeAgentFeedback(comment, TransactionID);
                return Json(new { success = true, message = "Dispute has been Done...." });
            }


        }
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                DataTable survey = await dl_Agent.GetAgentSurveyDashboard();
                DataTable assment = await dl_Agent.GetAssesment();
                DataTable dt1 = await dl_Agent.getMonitororIds();
                DataTable dt2 = await dl_Agent.getDisputeMonitororIds();
                DataTable dt3 = await dl_Agent.getZtSignOffData();
                List<AgentFeedBackDetails> feedbackList = dt1.AsEnumerable().Select(row => new AgentFeedBackDetails
                {
                    TransactionID = row.Field<string>("TransactionID"),
                    CreatedDate = row.Field<DateTime>("CreatedDate")


                }).ToList();


                List<AgentFeedBackDetails> agentsurvey = survey.AsEnumerable().Select(row => new AgentFeedBackDetails
                {
                    TransactionID = row.Field<string>("TransactionID"),
                    CreatedDate = row.Field<DateTime>("CreatedDate")


                }).ToList();


                List<AgentFeedBackDetails> disputeList = dt2.AsEnumerable().Select(row => new AgentFeedBackDetails
                {
                    TransactionID = row.Field<string>("TransactionID"),
                    CreatedDate = row.Field<DateTime>("CreatedDate")

                }).ToList();

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


                List<ZtSignOffDataAgentWise> ZtSignOff = dt3.AsEnumerable().Select(row => new ZtSignOffDataAgentWise
                {
                    Process = row.Field<string>("Process"),
                    SubProcessName = row.Field<string>("SubProcessName"),
                    Process_1 = row.Field<string>("Process_1"),
                    Process_2 = row.Field<string>("Process_2"),
                    Process_3 = row.Field<string>("Process_3"),
                    Process_4 = row.Field<string>("Process_4"),
                    Process_5 = row.Field<string>("Process_5"),
                    Process_6 = row.Field<string>("Process_6"),
                    Process_7 = row.Field<string>("Process_7"),
                    Process_8 = row.Field<string>("Process_8"),
                    Process_9 = row.Field<string>("Process_9"),
                    Process_10 = row.Field<string>("Process_10")


                }).ToList();


                var viewModel = new AgentDashboardViewModel
                {
                    FeedbackList = feedbackList,
                    DisputeList = disputeList,
                    ZtSignOffDataAgent= ZtSignOff,
                    assmentonl= assmentonl,
                    agentsurvey= agentsurvey
                };

                return View(viewModel);
            }
            catch(Exception ex)
            {
                return View();
            }
          
        }
        [HttpPost]
        public async Task< ActionResult> SubmitTest(AttemptTestViewModel model)
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


        public async Task<IActionResult> AttemptAgentSurvey(string TransactionID)
        {
            var model = new FeedbackViewModel
            {
                MonitoringId = TransactionID,
                AgentId = "agentId",
                Questions = new List<FeedbackQuestion>
            {
                new FeedbackQuestion { QuestionId = 1, QuestionText = "1. How would you rate the Quality of your feedback /Coaching experience?" },
                new FeedbackQuestion { QuestionId = 2, QuestionText = "2. Was your QA/TL knowledgeable and able to provide directions on improving performance?" },
                new FeedbackQuestion { QuestionId = 3, QuestionText = "3. Was the QA/TL provide feedback/coaching in a polite and a friendly manner?" },
                new FeedbackQuestion { QuestionId = 4, QuestionText = "4. Was the QA/TL able to demonstrate the change that is expected?" }
            }
            };

            return View(model);
            return View();
        }

        public async Task<IActionResult> AgentFeedBack(string TransactionID)
        {
            TempData["TtransactionID"] = TransactionID;

            DataTable dt1 = await dl_Agent.getAgentFeedbackSection(TransactionID);
            DataSet dt12 = await dl_Agent.getCQScoreSection(TransactionID);

            string CQScore = string.Empty;
            string QA_Comments = string.Empty;

            if (dt12.Tables.Count > 0 && dt12.Tables[0].Rows.Count > 0)
            {
                CQScore = dt12.Tables[0].Rows[0]["CQ_Score"]?.ToString();
                QA_Comments = dt12.Tables[0].Rows[0]["QA_Comments"]?.ToString();
            }
            try
            {
                if (dt12.Tables.Count > 0 && dt12.Tables[0].Rows.Count > 0)
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
           
            ViewBag.QA_Comments = QA_Comments;
            ViewBag.cqscore = CQScore;

            var sectionList = dt1.AsEnumerable().Select(row => new AgentfeedbackSectionModel
            {
                category = row.Field<string>("category"),
                level = row.Field<string>("level"),
                Section = row.Field<string>("SectionName"),
                QA_rating = row.Field<string>("QA_rating"),
                Scorable = row.Field<string>("Scorable"),
                Weightage = row.Field<string>("Weightage"),
                Commentssection = row.Field<string>("Commentssection"),
                 Fatal = row.Field<string>("Fatal")
            }).ToList();

            return View(sectionList);
        }



        public async Task<IActionResult> AgentDisputeFeedBack(string TransactionID)
        {
            TempData["TtransactionDisputeID"] = TransactionID;
            DataTable dt1 = await dl_Agent.getAgentFeedbackSection(TransactionID);
            DataSet dt12 = await dl_Agent.getCQScoreQADisputeSection(TransactionID);
          

            string CQScore = string.Empty;
            string QA_Comments = string.Empty;
            string CalibratedComment = string.Empty;

            if (dt12.Tables.Count > 0 && dt12.Tables[0].Rows.Count > 0)
            {
                CQScore = dt12.Tables[0].Rows[0]["CQ_Score"]?.ToString();
                QA_Comments = dt12.Tables[0].Rows[0]["QA_Comments"]?.ToString();
                CalibratedComment = dt12.Tables[0].Rows[0]["CalibratedComment"]?.ToString();
            }
            if (!string.IsNullOrEmpty(QA_Comments)  && dt12.Tables[0].Rows.Count > 0)
            {
                byte[] audioBytes = dt12.Tables[0].Rows[0]["AudioData"] as byte[];
                if (audioBytes != null && audioBytes.Length > 0)
                {
                    string base64Audio = Convert.ToBase64String(audioBytes);
                    ViewBag.AudioData = "data:audio/wav;base64," + base64Audio;
                }
            }




            ViewBag.QA_Comments = QA_Comments;
            ViewBag.cqscore = CQScore;
            ViewBag.Calibrated = CalibratedComment;
            var sectionList = dt1.AsEnumerable().Select(row => new AgentfeedbackSectionModel
            {

                category = row.Field<string>("category"),
                level = row.Field<string>("level"),
                Section = row.Field<string>("SectionName"),
                QA_rating = row.Field<string>("QA_rating"),
                Scorable = row.Field<string>("Scorable"),
                Weightage = row.Field<string>("Weightage"),
                Commentssection = row.Field<string>("Commentssection"),
                Fatal = row.Field<string>("Fatal")
            }).ToList();

            return View(sectionList);
        }


    }
}
