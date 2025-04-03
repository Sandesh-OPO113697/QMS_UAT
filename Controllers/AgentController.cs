using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;
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
                return Json(new { success = true, message = "Acknowlegde has been Done...." });
            }


        }
        public async Task<IActionResult> Dashboard()
        {
            DataTable dt1 = await dl_Agent.getMonitororIds();
            DataTable dt2 = await dl_Agent.getDisputeMonitororIds();
            List<AgentFeedBackDetails> feedbackList = dt1.AsEnumerable().Select(row => new AgentFeedBackDetails
            {
                TransactionID = row.Field<string>("TransactionID")
            }).ToList();

            List<AgentFeedBackDetails> disputeList = dt2.AsEnumerable().Select(row => new AgentFeedBackDetails
            {
                TransactionID = row.Field<string>("TransactionID")
            }).ToList();


            var viewModel = new AgentDashboardViewModel
            {
                FeedbackList = feedbackList,
                DisputeList = disputeList
            };

            return View(viewModel);
        }

        public async Task<IActionResult> AgentFeedBack(string TransactionID)
        {
            TempData["TtransactionID"] = TransactionID;
            DataTable dt1 = await dl_Agent.getAgentFeedbackSection(TransactionID);
            DataTable dt12 = await dl_Agent.getCQScoreSection(TransactionID);
            string CQScore = dt12.Rows[0]["CQ_Score"].ToString();
            string QA_Comments = dt12.Rows[0]["QA_Comments"].ToString();
            byte[] audioBytes = dt12.Rows[0]["AudioData"] as byte[];
            if (audioBytes != null)
            {
                string base64Audio = Convert.ToBase64String(audioBytes);
                ViewBag.AudioData = "data:audio/wav;base64," + base64Audio;
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
                Commentssection = row.Field<string>("Commentssection")
            }).ToList();

            return View(sectionList);
        }
       
    }
}
