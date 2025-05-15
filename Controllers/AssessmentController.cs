using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;
using System.Data.SqlClient;

namespace QMS.Controllers
{
    public class AssessmentController : Controller
    {
        private readonly Dl_Admin _admin;
        private readonly Dl_formBuilder dl_FormBuilder; 
        private readonly dl_Assesment dl_as;
        public AssessmentController(Dl_formBuilder adl, Dl_Admin adam  , dl_Assesment dl_Assesment)
        {

            _admin = adam;
            dl_FormBuilder = adl;
            dl_as = dl_Assesment;

        }

        public async Task<IActionResult> AttempTest()
        {
            
            return View();
        }
        public async Task<IActionResult> dashboard()
        {
            string locationid = UserInfo.LocationID;
            var data = await _admin.GetProcessListByLocation(locationid);
            var processList = data.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.ProcessList = processList;
            return View();
        }
        [HttpPost]
        public async Task< ActionResult> SubmitQuestions([FromBody] List<QuestionModel> questions)
        {
            int result = await dl_as.createAssesmemnt(questions);
            if (result==1)
            {
                return Json(new { success = true });

            }
            else
            {
                return Json(new { success = false });
            }
               
        }

    }
}
