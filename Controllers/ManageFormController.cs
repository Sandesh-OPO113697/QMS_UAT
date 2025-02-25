using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;
using System.Reflection;

namespace QMS.Controllers
{
    public class ManageFormController : Controller
    {
        private readonly Dl_Admin _admin;
        private readonly Dl_formBuilder dl_FormBuilder;
        public ManageFormController(Dl_formBuilder adl, Dl_Admin adam)
        {
         
            _admin = adam;
            dl_FormBuilder= adl;
        }

        public async Task<IActionResult> EditForm()
        {
            string locationid = UserInfo.LocationID;
            var data = await _admin.GetProcessListByLocation(locationid);
            var processList = data.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.ProcessList = processList;
            var sections = dl_FormBuilder.GetSectionMater();

            ViewBag.Section_Category = dl_FormBuilder.GetSectionCategories();
            ViewBag.RatingMaste = dl_FormBuilder. GetRatings();
            return View(sections);
        }


        public async Task< IActionResult> ViewForm()
        {
            var fields = await dl_FormBuilder.GetStaticfiedls();
            ViewBag.Fields = fields;
            return View();
        }



        [HttpPost]
        public async Task<JsonResult> InsertdynamicFeilds([FromBody] FormDataModel request)
        {
            if (request == null || request.fields == null || request.fields.Count == 0)
            {
                return Json(new { success = false, message = "No data received" });
            }

            int result = await dl_FormBuilder.addDynamicFeilds(request.fields); 

            return result > 0
                ? Json(new { success = true, message = "Data received successfully!" })
                : Json(new { success = false, message = "Failed to insert data." });
        }
        [HttpPost]
        public async Task<JsonResult> InsertSectionFeilds([FromBody] SectionFormData request)
        {
            if (request == null || request.sections == null || request.sections.Count == 0)
            {
                return Json(new { success = false, message = "No data received" });
            }

            int result = await dl_FormBuilder.AddSectionfeilds(request.sections);

            return result > 0
                ? Json(new { success = true, message = "Data received successfully!" })
                : Json(new { success = false, message = "Failed to insert data." });
        }




        public IActionResult FormBuilder()
        {
            return View();
        }

        public async Task< IActionResult> CreateForm()
        {
            string locationid = UserInfo.LocationID;
            var data = await _admin.GetProcessListByLocation(locationid);
            var processList = data.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.ProcessList = processList;
            DataSet dt = await dl_FormBuilder.GetSectionFeildAsync();
            var data1 = dt.Tables[0];
            var data2 = dt.Tables[1];
            var data3 = dt.Tables[2];
            var Section_Category = data1.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["id"].ToString(),
                Text = $"{row["SectionName"]}",
            }).ToList();
            ViewBag.Section_Category = Section_Category;

            var RatingMaste = data2.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["Id"].ToString(),
                Text = $"{row["RatingName"]}",
            }).ToList();
            ViewBag.RatingMaste = RatingMaste;

            var DynamicFields = data3.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["id"].ToString(),
                Text = $"{row["fields_Value"]}",
            }).ToList();
            ViewBag.DynamicFields = DynamicFields;
            return View();
        }
    }
}
