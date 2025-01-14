using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;

namespace QMS.Controllers
{
    public class ModuleController : Controller
    {
        private readonly DL_Module _module;
        private readonly Dl_Admin _admin;
        public ModuleController(DL_Module adl, Dl_Admin adam)
        {
            _module = adl;
            _admin = adam;
        }


        public IActionResult Modules(string roleid, string ModuleName)
        {
            switch (roleid)
            {
                case "1":
                    switch (ModuleName)
                    {
                        case "Create Program":
                            return RedirectToAction("CreateProcess" ,"Admin");
                        case "Manage Feature":
                            return RedirectToAction("FeatureMapping" , "Admin");
                        case "Manage Role":
                            return RedirectToAction("RoleMapping", "Admin");
                        case "Create User":
                            return RedirectToAction("CreateUser" , "Admin");
                        case "Create SubProgram":
                            return RedirectToAction("CreateSubProcess" , "Admin");
                        case "Assign Program":
                            return RedirectToAction("ProcessAssign" , "Admin");
                        default:
                            return View();
                    }
                case "3": 
                    switch (ModuleName)
                    {
                        case "Create User":
                            return RedirectToAction("ManageUsers");
                        default:
                            return View();
                    }
                case "2":
                    switch (ModuleName)
                    {
                        case "Create Program":
                            return RedirectToAction("CreateProcessBySiteAdmin");
                        case "Manage Feature":
                            return RedirectToAction("FeatureMapping");
                        case "Manage Role":
                            return RedirectToAction("RoleMapping");
                        case "Create User":
                            return RedirectToAction("CreateUser");
                        case "Create SubProgram":
                            return RedirectToAction("CreateSubProcess");
                        case "Assign Program":
                            return RedirectToAction("ProcessAssign");
                        default:
                            return View();
                    }
                default:
                    return View();
            }
        }
        [HttpPost]
        public async Task<ActionResult> InsertUsers(string Location_ID, string ProgramID, string SUBProgramID, string Role_ID, string UserID, string Password, string UserName, string PhoneNumber)
        {
            List<string> errorMessages = new List<string>();
            string locationid = UserInfo.LocationID;
            await _admin.InsertUserDetailsAsync(locationid, ProgramID, SUBProgramID, Role_ID, UserID, Password, UserName, PhoneNumber);
            errorMessages.Add("User Is Created Sucessfully !");
            TempData["ErrorMessages"] = errorMessages;
            return RedirectToAction("CreateUser");

        }

        
        public async Task<IActionResult> ManageUsers()
        {
            string locationid = UserInfo.LocationID;
            var Location = await _admin.GetLocationAsync();
            var filteredLocation = Location.Where(loc => loc.Value == locationid).ToList();
            var Role = await _admin.GetRoleAsync();
            var filteredRoles = Role.Where(r => r.Text != "SuperAdmin" && r.Text != "SiteAdmin").ToList();
            var prifix = await _admin.GetPrefixAsync();
            var data = await _admin.GetProcessListByLocationAccountAdmin(locationid);
            var processList = data.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.ProcessList = processList;
            DataTable dt = await _admin.GetUserListAsync();
            ViewBag.Locations = filteredLocation;
            ViewBag.Role = filteredRoles;
            return View(dt);
        }
        public async Task<IActionResult> CreateUser()
        {
            string locationid = UserInfo.LocationID;
            var Location = await _admin.GetLocationAsync();
            var filteredLocation = Location.Where(loc => loc.Value == locationid).ToList();
            var Role = await _admin.GetRoleAsync();
            var filteredRoles = Role.Where(r => r.Text != "SuperAdmin" && r.Text != "SiteAdmin").ToList();
            var prifix = await _admin.GetPrefixAsync();
            var data = await _admin.GetProcessListByLocation(locationid);
            var processList = data.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["ID"].ToString(),
                Text = $"{row["ProcessName"]}",
            }).ToList();
            ViewBag.ProcessList = processList;
            DataTable dt = await _admin.GetUserListAsync();
            ViewBag.Locations = filteredLocation;
            ViewBag.Role = filteredRoles;
            return View(dt);
        }
        public async Task<IActionResult> CreateProcessBySiteAdmin()
        {
            string locationid = UserInfo.LocationID;

            var Location = await _admin.GetLocationAsync();
            var filteredLocation = Location.Where(loc => loc.Value == locationid).ToList();
            ViewBag.LocationName = filteredLocation;
            var data = await _admin.GetProcessListByLocation(locationid);
            var processList = data.AsEnumerable().Select(row => new
            {
                id = row["ID"],
                locationName = row["LocationName"],
                processName = row["ProcessName"],
                active_Status = row["Active_Status"]

            }).ToList();
            ViewBag.ProcessList = processList;
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> InsertProcess(string Location_ID, string Program, string DataRetentions)
        {
            List<string> errorMessages = new List<string>();
            if (string.IsNullOrEmpty(Program))
            {
                errorMessages.Add("Please Enter Process Name.");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateProcessBySiteAdmin");
            }
            if (string.IsNullOrEmpty(DataRetentions))
            {
                errorMessages.Add("Please Enter Data Retention Day .");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateProcessBySiteAdmin");
            }
            string locationid = UserInfo.LocationID;
            await _admin.InsertProcessDetailsAsync(locationid, Program, DataRetentions);
            errorMessages.Add("Process Created Sucessfully !");
            TempData["ErrorMessages"] = errorMessages;
            return RedirectToAction("CreateProcessBySiteAdmin");

        }
        public async Task<ActionResult> RoleMapping()
        {
            var User = await _admin.GetUsersAsync();
            var RoleList = await _admin.GetRoleAndSubAsync(UserInfo.UserName);
            var filteredRoles = RoleList.Where(r => r.Text != "SuperAdmin" && r.Text != "SiteAdmin").ToList();
            ViewBag.UserList = User;
            ViewBag.RoleList = filteredRoles;
            return View();
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

        public async Task<ActionResult> FeatureMapping()
        {
            var User = await _admin.GetRoleAsync();
            var Feature = await _admin.GetFeature();
            var filteredRoles = User.Where(r => r.Text != "SuperAdmin" && r.Text != "SiteAdmin").ToList();
            ViewBag.UserList = filteredRoles;
            ViewBag.Feature = Feature;

            return View();
        }
        public async Task<ActionResult> CreateSubProcess()
        {
            string locationid = UserInfo.LocationID;
            var Location = await _admin.GetLocationAsync();
            var filteredLocation = Location.Where(loc => loc.Value == locationid).ToList();
            
            ViewBag.Locations = filteredLocation;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> InsertSubProcess(string Location_ID, string SubProcess, string ProgramID)
        {
            List<string> errorMessages = new List<string>();

            string locationid = UserInfo.LocationID;
            if (string.IsNullOrEmpty(ProgramID))
            {
                errorMessages.Add("Please Select Process Name.");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateSubProcess");
            }

            if (string.IsNullOrEmpty(SubProcess))
            {
                errorMessages.Add("Please Enter Data Sub Process .");
                TempData["ErrorMessages"] = errorMessages;
                return RedirectToAction("CreateSubProcess");
            }
            await _admin.InsertSubProcessDetailsAsync(locationid, ProgramID, SubProcess);
            errorMessages.Add("Sub-Process Created Sucessfully !");
            TempData["ErrorMessages"] = errorMessages;
            return RedirectToAction("CreateSubProcess");

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

    }
}
