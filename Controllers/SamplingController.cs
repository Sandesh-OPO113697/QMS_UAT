using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;

namespace QMS.Controllers
{
    public class SamplingController : Controller
    {
        private readonly DlSampling dlSampling;
        private readonly Dl_Admin _dlamin;
        public SamplingController(DlSampling dl, Dl_Admin dlamin)
        {
            dlSampling = dl;
            _dlamin = dlamin;
        }
        public IActionResult DashBoard(int SubFeatureid, string RoleName, int Featureid)
        {
            return View();
        }

        public async Task< ActionResult> Sample_calculator(string RoleName  , string Featureid , string SubFeatureid)
        {
            string locationid = UserInfo.LocationID;
            var data = await _dlamin.GetProcessListByLocation(locationid);
            var processList = data.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.ProcessList = processList;
            string Role= RoleName;
            string Userid = UserInfo.UserName;
            string LocationID= UserInfo.LocationID;
            ViewBag.Featureid = Featureid;
            ViewBag.SubFeatureid = SubFeatureid;
            
            ViewBag.rolename = Role;
            return View();
        }

        public async Task<JsonResult> AssignSapleSize([FromBody] Sampling ID)
         {

            var dt =await _dlamin.GetProcessListByLocation(UserInfo.LocationID);
             await dlSampling.AddSamplingCount(ID.Id, ID.Featureid , ID.SubFeatureid , ID.rolename , ID.Processs , ID.SubProcess);

            return Json(new { success = true ,message = "Sample size assigned successfully." }); 
        }
    }
}
