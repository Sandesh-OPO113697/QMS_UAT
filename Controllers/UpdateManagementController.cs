using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using NPOI.OpenXmlFormats.Dml.Diagram;
using NPOI.SS.UserModel;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;
using System.Reflection.Emit;
using System.Threading.Tasks;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace QMS.Controllers
{
    public class UpdateManagementController : Controller
    {
        private readonly DlSampling dlSampling;
        private readonly Dl_Admin _admin;
        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly dl_Monitoring dl_monitor;
        private readonly Dl_UpdateManagement  dl_udm;
        public UpdateManagementController(Dl_UpdateManagement  adpt ,Dl_formBuilder adl, DlSampling dl, Dl_Admin adam, dl_Monitoring dmmonoi)
        {
            dlSampling = dl;
            _admin = adam;
            dl_FormBuilder = adl;
            dl_monitor = dmmonoi;
            dl_udm = adpt;
        }
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] MailModel model)
        {
            foreach (var code in model.AgentCodes)
            {
                // Insert into EmailNotification table
                await dl_udm.InsertEmailNotificationAsync(
                    code,
                    model.Subject,
                    model.Body,
                    model.AttachmentFileName,
                     model.AttachmentBase64,
                    true // Notified = true
                );
            }

            //bool status = await dl_udm.SendMail(model.To, model.Subject, model.Body, model.AttachmentBase64, model.AttachmentFileName);
            return Ok(new { success = true, message = true ? "Notification sent successfully!" : "Failed to send Notification." });
        }
        public async  Task<IActionResult> Dashboard()
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

        public async Task <IActionResult> getTlAndAgentList([FromBody] Process_SUbProcess id)
        {
            List<object> list = await dl_udm.GetTLAndAgentList(id.ProcessID , id.SUBProcessID);
       
            return Json(new { agentTlList  = list });
        }
    }
}
