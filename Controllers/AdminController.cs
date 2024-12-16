using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.DataBaseService;
using QMS.Models;
using System;
using System.Data;

namespace QMS.Controllers
{
    public class AdminController : Controller
    {
        private readonly Dl_Admin _admin;
        public AdminController(Dl_Admin adl)
        {
            _admin= adl;
        }
        public async Task<JsonResult> GetProcessList([FromBody] DropDawon id)
        {
            
            var proces = await _admin.GetProcessName(id.Id);
            return Json(new { success = true , proces });
        }
        public async Task<JsonResult> GetSUBProcessList([FromBody] DropDawon id)
        {

            var proces = await _admin.GetProcessName(id.Id);
            return Json(new { success = true, proces });
        }
        public async Task< IActionResult> CreateUser()
        {
            var Location = await _admin.GetLocationAsync();
            var Role = await _admin.GetRoleAsync();
            var prifix= await _admin.GetPrefixAsync();
            DataTable dt =await _admin.GetUserListAsync();
            ViewBag.Locations = Location;
            ViewBag.Role = Role;
            return View(dt);
        }
     
        public async Task<JsonResult> ActiveDeActiveUser([FromBody] UserStatusModel model)
        {
            try
            {
                await _admin.DeactiveActiveUser(model.Id, model.IsActive);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the status." });
            }
        }
    }
}
