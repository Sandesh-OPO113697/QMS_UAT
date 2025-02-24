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
            return View();
        }
    }
}
