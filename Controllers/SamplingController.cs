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
        [HttpPost]
        public async  Task<JsonResult> SubmitData([FromBody]  Dictionary<string, string> formData)
        {
            if (formData == null || formData.Count == 0)
            {
                return Json(new { success = false, message = "Invalid data received" });
            }
            else
            {
                await dlSampling.InsertAllocationDetails(formData);
            }
            return Json(new { success = true, message = "Data saved successfully!" });
        }

        public async Task<JsonResult> AssignFiltersAginProcess([FromBody] AssignFilter ID)
        {
            await dlSampling.AssignFiltersAgainProcess(ID.AhtMin, ID.AhtMax, ID.Disposition, ID.Process, ID.SubProcess);
            return Json(new { success = true, message = "filters assigned successfully." });
        }
        public async Task<JsonResult> GetSamplingCount([FromBody] DropDawon id)
        {
            string SampleSixe = await dlSampling.GetSamplingCountByProcessandsub(id.Id.ToString(), "0");
            string QACount = await dlSampling.GetQACountByProcessandsub(id.Id.ToString(), "0");
            string TLCount = await dlSampling.GetTLCountByProcessandsub(id.Id.ToString(), "0");

            return Json(new { sucess = true, SampleSize = SampleSixe, qacount = QACount, tlcount = TLCount });
        }
        public async Task<JsonResult> GetSamplingCountwithSubProcess([FromBody] Process_SUbProcess id)
        {
            string SampleSixe = await dlSampling.GetSamplingCountByProcessandsub(id.ProcessID.ToString(), id.SUBProcessID.ToString());
            string QACount = await dlSampling.GetQACountByProcessandsub(id.ProcessID.ToString(), id.SUBProcessID.ToString());
            string TLCount = await dlSampling.GetTLCountByProcessandsub(id.ProcessID.ToString(), id.SUBProcessID.ToString());
            return Json(new { sucess = true, SampleSize = SampleSixe, qacount = QACount , tlcount= TLCount });
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
            var roleList2 = RoleList.Where(r => r.Text == "QA Manager" || r.Text == "Monitor Supervsior").ToList();

            ViewBag.RoleList = roleList2;
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
        public async Task< ActionResult> SamplingFilers(string RoleName, string Featureid, string SubFeatureid)
        {
            DataTable dt = await _dlamin.GetProcessListAsync();
            var processList = dt.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.ProcessList = processList;
            return View();
        }
    }
}
