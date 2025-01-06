using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.DataBaseService;
using QMS.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.Json;

namespace QMS.Controllers
{
    public class AdminController : Controller
    {
        private readonly Dl_Admin _admin;
        public AdminController(Dl_Admin adl)
        {
            _admin = adl;
        }
        public async Task<JsonResult> ActiveDeActiveSubProcess(int id, int isActive)
        {
            try
            {
                await _admin.DeactiveActiveSubProcess(id, Convert.ToBoolean(isActive));
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the status." });
            }
        }
        public async Task<JsonResult> ActiveDeActiveProcess(int id, int isActive)
        {
            try
            {
                await _admin.DeactiveActiveProcess(id,Convert.ToBoolean(isActive));
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the status." });
            }
        }
        [HttpPost]
        public async Task<ActionResult> UpdateProcessName(int id, string processName)
        {
            try
            {
                await _admin.UpdateProcess(Convert.ToInt32(id), processName);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {

                return Json(new { success = false });
            }
        }

        [HttpPost]
        public async Task<JsonResult> UpdateSubProcessName(int id, string newName)
        {
            int subProcessId = Convert.ToInt32(id);
            await _admin.UpdateSubProcessByName(subProcessId, newName);

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<ActionResult> InsertSubProcess(string Location_ID, string SubProcess, string ProgramID)
        {
            List<string> errorMessages = new List<string>();

            if (string.IsNullOrEmpty(Location_ID) || Location_ID == "Select Location")
            {
                errorMessages.Add("Please select a valid location.");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateProcess");
            }
            if (string.IsNullOrEmpty(ProgramID))
            {
                errorMessages.Add("Please Select Process Name.");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateProcess");
            }

            if (string.IsNullOrEmpty(SubProcess))
            {
                errorMessages.Add("Please Enter Data Sub Process .");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateProcess");
            }


            await _admin.InsertSubProcessDetailsAsync(Location_ID, ProgramID, SubProcess);
            errorMessages.Add("Sub-Process Created Sucessfully !");
            TempData["ErrorMessages"] = errorMessages;
            return RedirectToAction("CreateProcess");

        }

        public async Task<ActionResult> CreateSubProcess()
        {
            var Location = await _admin.GetLocationAsync();
            ViewBag.Locations = Location;
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> InsertProcess(string Location_ID, string Program, string DataRetentions)
        {
            List<string> errorMessages = new List<string>();

            if (string.IsNullOrEmpty(Location_ID) || Location_ID == "Select Location")
            {
                errorMessages.Add("Please select a valid location.");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateProcess");
            }
            if (string.IsNullOrEmpty(Program))
            {
                errorMessages.Add("Please Enter Process Name.");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateProcess");
            }

            if (string.IsNullOrEmpty(DataRetentions))
            {
                errorMessages.Add("Please Enter Data Retention Day .");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateProcess");
            }


            await _admin.InsertProcessDetailsAsync(Location_ID, Program, DataRetentions);
            errorMessages.Add("Process Created Sucessfully !");
            TempData["ErrorMessages"] = errorMessages;
            return RedirectToAction("CreateProcess");

        }
        public async Task<JsonResult> GetProcessListByLocation(string locationid)
        {
            var data = await _admin.GetProcessListByLocation(locationid);
            var processList = data.AsEnumerable().Select(row => new
            {
                id = row["ID"],
                locationName = row["LocationName"],
                processName = row["ProcessName"],
                active_Status = row["Active_Status"]
                
            }).ToList();

            return Json(processList);
        }
        public async Task<ActionResult> CreateProcess()
        {
            var Location = await _admin.GetLocationAsync();
            ViewBag.Locations = Location;
           
            return View();
        }

        
        public async Task<ActionResult> DashBoard()
        {
            DataTable dt = await _admin.GetProcessListAsync();
            return View(dt);
        }

        [HttpPost]
        public async Task<List<SelectListItem>> GetRoleByRole([FromBody] DropDawnString request)
        {
            string RoleID = request.ID;
            string UserName = await _admin.GetUserNameByID(RoleID);
            var Feture = await _admin.GetRoleAndSubAsync(UserName);
            return Feture;
        }
        [HttpPost]
        public async Task<List<SelectListItem>> GetProgramByRole([FromBody] DropDawnString request)
        {
            string RoleID = request.ID;
            string UserName = await _admin.GetUserNameByID(RoleID);
            var Feture = await _admin.GetProcessesAndSubAsync(UserName);
            return Feture;
        }
        [HttpPost]
        public async Task<List<SelectListItem>> GetFeatureByRole([FromBody] DropDawnString request)
        {
            string RoleID = request.ID;
            var Feture = await _admin.GetFeatureByRole(RoleID);
            return Feture;
        }

        public async Task<ActionResult> FeatureMapping()
        {
            var User = await _admin.GetRoleAsync();
            var Feature = await _admin.GetFeature();
            ViewBag.UserList = User;
            ViewBag.Feature = Feature;
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> FeatureInsert(ProcessAssignViewModel model)
        {
            if (string.IsNullOrEmpty(model.User_ID) || model.User_ID == "Select Role")
            {
                TempData["RoleCreate"] = "Select Role";
                return RedirectToAction("FeatureMapping");
            }
            else
            {
                await _admin.AssignFeature(model.User_ID, model.UserName, model.SelectedProcesses);
                TempData["RoleCreate"] = "Feature Map SucessFully";
            }

            return RedirectToAction("FeatureMapping");

        }

        [HttpPost]
        public async Task<ActionResult> RoleInsert(ProcessAssignViewModel model)
        {
            if (string.IsNullOrEmpty(model.User_ID) || model.User_ID == "Select User")
            {
                TempData["RoleCreate"] = "Select Role";
                return RedirectToAction("RoleMapping");
            }
            else
            {
                await _admin.AssignRole(model.User_ID, model.UserName, model.SelectedProcesses);
                TempData["RoleCreate"] = "Role Map SucessFully";
            }

            return RedirectToAction("RoleMapping");

        }
        public async Task<ActionResult> RoleMapping()
        {
            var User = await _admin.GetUsersAsync();
            var RoleList = await _admin.GetRoleAndSubAsync(UserInfo.UserName);
            ViewBag.UserList = User;
            ViewBag.RoleList = RoleList;
            return View();
        }
        public async Task<ActionResult> ProcessAssign()
        {
            try
            {
                var User = await _admin.GetUsersAsync();
                var ProceccList = await _admin.GetProcessesAndSubAsync(UserInfo.UserName);
                ViewBag.UserList = User;
                ViewBag.ProcessList = ProceccList;

            }
            catch (Exception ex)
            {
                TempData["ProceeeCreate"] = ex.Message;

            }

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ProcessAssign(ProcessAssignViewModel model)
        {
            if (string.IsNullOrEmpty(model.User_ID) || model.User_ID == "Select User")
            {
                TempData["ProceeeCreate"] = "Select Users";
                return RedirectToAction("ProcessAssign");
            }
            else
            {
                await _admin.AssignProcess(model.User_ID, model.UserName, model.SelectedProcesses);
                TempData["ProceeeCreate"] = "Process Map SucessFully";
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

            //if (string.IsNullOrEmpty(Password))
            //{
            //    errorMessages.Add("Password must be at least 6 characters long.");
            //    TempData["ErrorMessages"] = errorMessages;
            //    return RedirectToAction("CreateUser");
            //}

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
