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
            _admin = adl;
        }

        public async Task<ActionResult> ProcessAssign()
        {
            var User = await _admin.GetUsersAsync();
            var ProceccList = await _admin.GetProcessesAndSubAsync(UserInfo.UserName);
            ViewBag.UserList = User;
            ViewBag.ProcessList = ProceccList;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ProcessAssign(ProcessAssignViewModel model)
        {
            if (string.IsNullOrEmpty(model.User_ID) || model.User_ID == "Select User")
            {
                TempData["ErrorMessages"] = "Select Users";
                return RedirectToAction("ProcessAssign");
            }
            else
            {
                await _admin.AssignProcess(model.User_ID, model.UserName, model.SelectedProcesses);
                TempData["ErrorMessages"] = "Process Map SucessFully";
            }

            return RedirectToAction("ProcessAssign");
        }
        [HttpPost]
        public async Task<ActionResult> InsertUsers(string Location_ID, string ProgramID, string SUBProgramID, string Role_ID, string UserID, string Password, string UserName, string PhoneNumber)
        {
            List<string> errorMessages = new List<string>();

            if (string.IsNullOrEmpty(Location_ID) || Location_ID == "Select Location")
            {
                errorMessages.Add("Please select a valid location.");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateUser");
            }

            if (string.IsNullOrEmpty(ProgramID) || ProgramID == "Select Process")
            {
                errorMessages.Add("Please select a valid program.");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateUser");
            }


            if (string.IsNullOrEmpty(Role_ID) || Role_ID == "Select Role")
            {
                errorMessages.Add("Please select a valid role.");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateUser");
            }

            if (string.IsNullOrEmpty(UserID) || UserID == "____")
            {
                errorMessages.Add("User ID cannot be empty or the default value.");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateUser");
            }

            if (string.IsNullOrEmpty(Password))
            {
                errorMessages.Add("Password must be at least 6 characters long.");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateUser");
            }

            if (string.IsNullOrEmpty(UserName))
            {
                errorMessages.Add("User Name cannot be empty.");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateUser");
            }

            if (string.IsNullOrEmpty(PhoneNumber) || PhoneNumber.Length != 10)
            {
                errorMessages.Add("Phone Number must be exactly 10 digits.");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateUser");
            }

            await _admin.InsertUserDetailsAsync(Location_ID, ProgramID, SUBProgramID, Role_ID, UserID, Password, UserName, PhoneNumber);
            errorMessages.Add("User Is Created Sucessfully !");
            TempData["ErrorMessages"] = errorMessages;
            return RedirectToAction("CreateUser");

        }
        public async Task<JsonResult> GetProcessList([FromBody] DropDawon id)
        {

            var proces = await _admin.GetProcessName(id.Id);
            return Json(new { success = true, proces });
        }
        [HttpPost]
        public async Task<JsonResult> HandleBackspace()
        {
            string prefix = await _admin.GetPrefixAsync();
            return Json(new { newUsername = prefix });
        }
        public async Task<JsonResult> GetSUBProcessList([FromBody] DropDawon id)
        {

            var proces = await _admin.GetSUBProcessName(id.Id);
            return Json(new { success = true, proces });
        }
        public async Task<IActionResult> CreateUser()
        {
            var Location = await _admin.GetLocationAsync();
            var Role = await _admin.GetRoleAsync();
            var prifix = await _admin.GetPrefixAsync();
            DataTable dt = await _admin.GetUserListAsync();
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
