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
            dl_FormBuilder = adl;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSection([FromBody] SectionGridModel model)
        {
            try
            {
                bool isDeleted = await dl_FormBuilder.DeleteSectionAsync(model.Id);
                if (isDeleted)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete section." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSection([FromBody] SectionGridModel section)
        {
             await dl_FormBuilder.EditSectionRow(section);
            return Ok();
        }

            [HttpGet]
        public async Task<IActionResult> GetDropdownData()
        {
            DataSet ds = await dl_FormBuilder.GetSectionDropdownDataAsync();

            if (ds.Tables.Count < 2)
                return BadRequest("Failed to retrieve dropdown data.");

            DataTable sectionTable = ds.Tables[0];
            DataTable ratingTable = ds.Tables[1];

            var sectionList = sectionTable.AsEnumerable().Select(row => new
            {
                Id = row.Field<int>("id"),
                Name = row.Field<string>("SectionName")
            }).ToList();

            var ratingList = ratingTable.AsEnumerable().Select(row => new
            {
                Id = row.Field<int>("Id"),
                Name = row.Field<string>("RatingName")
            }).ToList();

            return Json(new { Sections = sectionList, Ratings = ratingList });
        }

        public async Task<IActionResult> GetdynamicfeildsGried([FromBody] Process_SUbProcess id)
        {
            var dataTable = await dl_FormBuilder.GetdynamicFeildsGriedAsync(id.ProcessID, id.SUBProcessID);
            try
            {
                var sectionList = dataTable.AsEnumerable().Select(row => new dynamicgriedModel
                {
                    id = row.Field<int>("id"),
                    Dynamic_field_ID = row.Field<int>("Dynamic_field_ID"),
                    fields_Value = row.Field<string>("fields_Value"),
                    ValueType = row.Field<string>("ValueType"),
                    value = row.Field<string>("value")
                }).ToList();
                return Json(sectionList);
            }
            catch (Exception ex)
            {
                return Json(null);
            }

        }
        public async Task<IActionResult> GetSectionGried([FromBody] Process_SUbProcess id)
        {
            var dataTable = await dl_FormBuilder.GetSectionGriedAsync(id.ProcessID, id.SUBProcessID);
            try
            {
               
                var sectionList = dataTable.AsEnumerable().Select(row => new SectionGridModel
                {
                    Id = row.Field<int>("id"),
                    Category = row.Field<string>("Category"),
                    SectionName = row.Field<string>("SectionName"),
                    SectionId = row.Field<int>("SectionId"),
                    RatingName = row.Field<string>("RatingName"),
                    RatingId = row.Field<int>("Ratingid"),
                    Scorable = row.Field<string>("Scorable"),
                    Score = row.Field<int>("Score"),
                    Level = row.Field<string>("Level"),
                    Fatal = row.Field<string>("Fatal"),
                    Active = row.Field<string>("Active")
                }).ToList();

                return Json(sectionList);

            }
            catch (Exception ex )
            {
                return Json(null);
            }
        
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

            return View();
        }


        public async Task<IActionResult> ViewForm()
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
                ? Json(new { success = true, message = "Data Insert successfully!" })
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
                ? Json(new { success = true, message = "Form Created  successfully!" })
                : Json(new { success = false, message = "Failed to insert data." });
        }




        public IActionResult FormBuilder()
        {
            return View();
        }

        public async Task<IActionResult> CreateForm()
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
