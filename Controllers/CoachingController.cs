using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;

namespace QMS.Controllers
{
    public class CoachingController : Controller
    {
        private readonly Dl_Coaching dl_coching;
        private readonly Dl_Admin _admin;
        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly dl_Monitoring dl_monitor;
        private readonly Dl_UpdateManagement dl_udm;
        public CoachingController(Dl_UpdateManagement adpt, Dl_formBuilder adl, Dl_Coaching dl, Dl_Admin adam, dl_Monitoring dmmonoi)
        {
            dl_coching = dl;
            _admin = adam;
            dl_FormBuilder = adl;
            dl_monitor = dmmonoi;
            dl_udm = adpt;
        }
        public async Task<IActionResult> getQaManagertList([FromBody] Process_SUbProcess id)
        {
            List<object> list = await dl_coching.GetQaManagerList(id.ProcessID, id.SUBProcessID);

            return Json(new { agentTlList = list });
        }
        public async Task< IActionResult> Dashboard()
        {
            DataTable dt = await _admin.GetProcessListAsync();
            var processList = dt.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.Process = processList;
            return View();
        }
    }
}
