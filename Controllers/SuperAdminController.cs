using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.ResponseCompression;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text.Json;

namespace QMS.Controllers
{
    public class SuperAdminController : Controller
    {
        private readonly DL_SuperAdmin _super;
        public SuperAdminController(DL_SuperAdmin dl)
        {
            _super = dl;
        }
        public async Task<IActionResult> Dashboard()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType == "SuperAdmin")
            {
                var accountData = await _super.GetAccountDetailsAsync();
                return View(accountData);
            }
            if (userType == "Admin")
            {
                return View();
            }
            else
            {
                return View();
            }

        }

        [HttpPost]
        [HttpPost]
        public async Task<JsonResult> DeactivateUsers(List<string> checkedUsers, List<string> uncheckedUsers)
        {
            if (checkedUsers == null || uncheckedUsers == null)
            {
                return Json(new { success = false, message = "Binding failed!" });
            }

            // Log received data
            System.Diagnostics.Debug.WriteLine($"Checked Users: {string.Join(", ", checkedUsers)}");
            System.Diagnostics.Debug.WriteLine($"Unchecked Users: {string.Join(", ", uncheckedUsers)}");

            await _super.DeactivateUserByAccountAsync(checkedUsers, uncheckedUsers);


            return Json(new { success = false });

        }   
        public class DeactivationRequest
        {
            public List<string> ActiveUsers { get; set; }
            public List<string> InactiveUsers { get; set; }
        }

        public async Task<IActionResult> UpdateAccountStatus([FromBody] dynamic request )
        {

            string accountId = string.Empty;
            int isActive = 0; 
            try
            {
                if (request.TryGetProperty("accountId", out JsonElement accountIdElement))
                {
                    accountId = accountIdElement.GetString();
                }

                if (request.TryGetProperty("isActive", out JsonElement aisActiveElement))
                {
                    isActive = aisActiveElement.GetInt32();
                }
                await _super.UpdateAccountStatus(accountId, isActive);

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            return Json(new { success = true });
        }


        [HttpPost]
        public async Task<JsonResult> GetUsersByAccount([FromBody] dynamic request)
        {
            string accountId = string.Empty;
            try
            {
                if (request.TryGetProperty("accountId", out JsonElement accountIdElement))
                {
                    accountId = accountIdElement.GetString();
                }
            }
            catch(Exception ex)
            {

            }
            var data = await _super.GetUserByAccountIDAsync(accountId); // Await the async method
            return Json(new { success = true, data });
        }
       

        public async Task<IActionResult> CreateAccount()
        {
            var accountData = await _super.GetAccountDetailsAsync();

            return View(accountData);
        }


        public async Task<IActionResult> CreateUser()
        {
            var accountData = await _super.GetAccountDetailsAsync();
            var accountList = accountData.AsEnumerable()
                                      .Select(row => new SelectListItem
                                      {
                                          Value = row["AccountID"].ToString(),
                                          Text = row["AccountName"].ToString()
                                      }).ToList();

            return View(accountList);

        }
        [HttpPost]
        public async Task<ActionResult> CreateUser(AdminCreateViewModel model)
        {
            string UserName = model.UserName;
            var accountData = await _super.GetAccountDetailsAsync();
            var accountList = accountData.AsEnumerable()
                                      .Select(row => new SelectListItem
                                      {
                                          Value = row["AccountID"].ToString(),
                                          Text = row["AccountName"].ToString()
                                      }).ToList();
            if (string.IsNullOrEmpty(model.AccountID) || string.IsNullOrEmpty(model.Password) || model.AccountID == "" || UserName.Substring(0, 4) == "____" || string.IsNullOrEmpty(model.UserName))
            {

                ViewBag.ErrorLog = "Please select all valid fields.";
                return View(accountList);
            }
            else
            {
                await _super.CreateAdminAsync(model.UserName, model.Password, model.AccountID);
                ViewBag.ErrorLog = "User Created Successfully  !!!";
                return View(accountList);
            }
        }

        [HttpPost]
        public async Task<ActionResult> BindAccountPrifix(string accountId, string elementId)
        {

            string AccountPrifix = await _super.GetAccountPrifixAsync(accountId, elementId);
            if (AccountPrifix == null)
            {
                return Json(new { success = false, AccountPrifix = "" });
            }
            else
            {
                return Json(new { success = true, AccountPrifix });
            }

        }

    }
}
