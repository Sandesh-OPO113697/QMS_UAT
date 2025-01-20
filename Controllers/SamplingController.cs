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

       
        public async Task<JsonResult> GetSamplingCount([FromBody] DropDawon id)
        {
            string SampleSixe = await dlSampling.GetSamplingCountByProcessandsub(id.Id.ToString(), "0");
            string QACount = await dlSampling.GetQACountByProcessandsub(id.Id.ToString(), "0");

            return Json(new { sucess = true, SampleSize = SampleSixe, qacount = QACount });
        }
        public async Task<JsonResult> GetSamplingCountwithSubProcess([FromBody] Process_SUbProcess id)
        {
            string SampleSixe = await dlSampling.GetSamplingCountByProcessandsub(id.ProcessID.ToString(), id.SUBProcessID.ToString());
            string QACount = await dlSampling.GetQACountByProcessandsub(id.ProcessID.ToString(), id.SUBProcessID.ToString());
            return Json(new { sucess = true, SampleSize = SampleSixe, qacount = QACount });
        }

        public async Task<IActionResult> WorkAllowcation(int SubFeatureid, string RoleName, int Featureid)
        {
            var AuditType = await dlSampling.GetAuditType();
            ViewBag.AuditTypeList = AuditType;
            string locationid = UserInfo.LocationID;
            var Location = await _dlamin.GetLocationAsync();
            if (UserInfo.UserType == "Admin")
            {
                var filteredLocation = Location.Where(loc => loc.Value == locationid).ToList();
                ViewBag.LocationName = Location;
            }
            else
            {
                var filteredLocation = Location.Where(loc => loc.Value == locationid).ToList();
                ViewBag.LocationName = filteredLocation;
            }

            var RoleList = await dlSampling.GetRoleList(RoleName);
            ViewBag.RoleList = RoleList;
            DataTable dt = await _dlamin.GetProcessListAsync();
            var processList = dt.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.Process = processList;
            return View();
        }

        public IActionResult DashBoard(int SubFeatureid, string RoleName, int Featureid)
        {
            return View();
        }

        public async Task<ActionResult> Sample_calculator(string RoleName, string Featureid, string SubFeatureid)
        {
            string locationid = UserInfo.LocationID;
            var data = await _dlamin.GetProcessListByLocation(locationid);
            var processList = data.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.ProcessList = processList;
            string Role = RoleName;
            string Userid = UserInfo.UserName;
            string LocationID = UserInfo.LocationID;
            ViewBag.Featureid = Featureid;
            ViewBag.SubFeatureid = SubFeatureid;

            ViewBag.rolename = Role;
            return View();
        }

        public async Task<JsonResult> AssignSapleSize([FromBody] Sampling ID)
        {

            var dt = await _dlamin.GetProcessListByLocation(UserInfo.LocationID);
            await dlSampling.AddSamplingCount(ID.Id, ID.Featureid, ID.SubFeatureid, ID.rolename, ID.Processs, ID.SubProcess);

            return Json(new { success = true, message = "Sample size assigned successfully." });
        }
    }
}
