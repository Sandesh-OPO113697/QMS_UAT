using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;

namespace QMS.Controllers
{
    public class MonitorSupervsiorController : Controller
    {
        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly DL_QaManager dl_qa;
        private readonly DL_Agent dl_Agent;
        private readonly Dl_Admin _dlamin;
        public MonitorSupervsiorController(DL_QaManager adl, DL_Agent adla, Dl_formBuilder dl_FormBuilder, Dl_Admin dlamin)
        {
            dl_qa = adl;
            dl_Agent = adla;
            this.dl_FormBuilder = dl_FormBuilder;
           _dlamin = dlamin;
        }

        public async Task<IActionResult> Dashboard()
        {

            try
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

                DataTable assment = await dl_Agent.GetAssesment();
                List<DisputeCallfeedbackModel> List = await dl_qa.DisputeAgentFeedback();
                List<ZTcaseModel> ZTlist = await dl_qa.ZtcaseShow();
                List<ReviewDataModel> coutingList = await dl_qa.GetCaoutingList();

                List<AgentToQASurveyModel> AgentToQASurveylist = await dl_qa.AgentToQASurveylist();
                List<UpdateListManagement> UpdateList = await dl_qa.Updatemanagement();

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
                    AgentToQASurvey = AgentToQASurveylist,

                    updateList = UpdateList
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
