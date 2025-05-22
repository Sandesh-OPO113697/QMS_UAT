using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;

namespace QMS.Controllers
{
    public class OutlierManagementController : Controller
    {
        
        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly DL_QaManager dl_qa;
        private readonly DL_Agent dl_Agent;
        private readonly Dl_Admin _dlamin;
        private readonly Dl_Outline _dlout;
        public OutlierManagementController(DL_QaManager adl, DL_Agent adla, Dl_formBuilder dl_FormBuilder, Dl_Admin dlamin, Dl_Outline dlout)
        {
            dl_qa = adl;
            dl_Agent = adla;
            this.dl_FormBuilder = dl_FormBuilder;
            _dlamin = dlamin;
            _dlout = dlout;
        }
        public async Task< IActionResult> Datasource()
        {
            return View();
        }

        public async Task<IActionResult> Identification()
        {
            return View();
        }

        public async  Task< ActionResult> CallQualityScore()
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
        public async Task<ActionResult> Lowest_Quartile()
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

        public async Task<IActionResult> getCallQalityDash([FromBody] Process_SUbProcess id)
        {
            DataTable dt = await _dlout.DahsboardCallQuality(id.ProcessID, id.SUBProcessID);

            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }

            return Json(new { datatable = list });
        }

        public async Task<IActionResult> GetLowestQuatile([FromBody] Process_SUbProcess id)
        {
            DataTable dt = await _dlout.GetQuatile(id.ProcessID, id.SUBProcessID);

            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }

            return Json(new { datatable = list });
        }


        public async Task<IActionResult>Agentperformance_on_metrics([FromBody] Process_SUbProcess id)
        {
            DataTable dt = await _dlout.AgentPerformence(id.ProcessID, id.SUBProcessID);

            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }

            return Json(new { datatable = list });
        }


        public async Task<IActionResult> Agent_performance([FromBody] Process_SUbProcess id)
        {
            DataTable dt = await _dlout.AgentPerformence_LQ(id.ProcessID, id.SUBProcessID);

            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }

            return Json(new { datatable = list });
        }

        public async Task<IActionResult> GetTransactionAuditDetails([FromBody] Process_SUbProcess id)
        {
            DataTable dt = await _dlout.TransactionAudit(id.ProcessID, id.SUBProcessID);

            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }

            return Json(new { datatable = list });
        }


        public async Task<IActionResult> GetTransactionAuditDetailsIdentification([FromBody] Process_SUbProcess id)
        {
            DataTable dt = await _dlout.TransactionAuditIdentification(id.ProcessID, id.SUBProcessID);

            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }

            return Json(new { datatable = list });
        }


        public async Task< ActionResult> AgentPerformance()
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
        public async Task<ActionResult> AgentPerformanceLowestScore()
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


        public async Task< ActionResult> TransformationAudits()
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
        public async Task<ActionResult> TransformationAuditsIdentification()
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

        public ActionResult AnotherView()
        {
            
            return View();
        }
    }
}
