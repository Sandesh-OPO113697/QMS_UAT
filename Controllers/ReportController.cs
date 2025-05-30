using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.SS.Formula;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;
using Formula = QMS.Models.Formula;

namespace QMS.Controllers
{
    public class ReportController : Controller
    {
        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly DL_QaManager dl_qa;
        private readonly DL_Agent dl_Agent;
        private readonly dl_Report _dlreport;
        private readonly Dl_Admin _dlamin;

        public ReportController(DL_QaManager adl, DL_Agent adla, Dl_formBuilder dl_FormBuilder, Dl_Admin dlamin, dl_Report dlrp)
        {
            dl_qa = adl;
            dl_Agent = adla;
            this.dl_FormBuilder = dl_FormBuilder;
            _dlamin = dlamin;
            _dlreport = dlrp;
        }

        [HttpPost]
        public async Task<JsonResult> SaveFormulaFields([FromBody] FormulaModel model)
        {
            if (model == null || model.fields == null || model.fields.Count == 0 || string.IsNullOrEmpty(model.formulaName))
            {
                return Json(new { success = false, message = "Invalid data" });
            }
            else
            {
                await _dlreport.CreateFormula(model);
                return Json(new { success = true });
            }


        }
        [HttpPost]
        public async Task<IActionResult> GetFormulaFields([FromBody] Pulltemplate model)
        {

            DataTable dt = await _dlreport.GetFieldsByFormula(model.formulaName, model.programId, model.subprogram, model.agentname);
            var list = dt.AsEnumerable()
                             .Select(row => dt.Columns.Cast<DataColumn>()
                             .ToDictionary(col => col.ColumnName, col => row[col])).ToList();

            return Json(new { report = list });
        }

        public async Task<IActionResult> getFormulas([FromBody] Process_SUbProcess id)
        {
            List<Formula> list = await _dlreport.GetTemplate(id.ProcessID, id.SUBProcessID);
            return Json(new { agentTlList = list });
        }


        public async Task<IActionResult> CheckFormulaisCrated([FromBody] CheckTemplate id)
        {
            var status = await _dlreport.checkTemplate(id.programId, id.subprogram, id.formulaName);
            return Json(new { status = status });
        }

        public async Task<IActionResult> CreateFormula()
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
    }

}