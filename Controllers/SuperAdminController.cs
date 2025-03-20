using Microsoft.AspNetCore.Authorization;
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

        public async Task<ActionResult> InsertAccount(string AccountName ,string AccountPrefix , string AuthenticationType, string RecApiList, string RecApiConnID)
        {
            string prefix = AccountPrefix?.ToUpper();
            if (string.IsNullOrEmpty(prefix))
            {
                TempData["Validation"] = "Account prefix cannot be empty.";
                return RedirectToAction("CreateAccount");
            }
            if (prefix.Length != 3)
            {
                TempData["Validation"] = "Account prefix must be exactly 3 characters long.";
                return RedirectToAction("CreateAccount");
            }
            if (!string.IsNullOrEmpty(prefix) && System.Text.RegularExpressions.Regex.IsMatch(prefix, @"^[A-Z]+$"))
            {
               
            }
            else
            {
                TempData["Validation"] = "Account prefix must contain only alphanumeric characters.";
                return RedirectToAction("CreateAccount");
            }
            if (string.IsNullOrEmpty(AccountName) || AuthenticationType =="" || AuthenticationType == null)
            {
                TempData["Validation"] = "Account name and AuthenticationType cannot be empty.";
                return RedirectToAction("CreateAccount");
            }
            if (string.IsNullOrEmpty(RecApiList) || RecApiList == "" || RecApiList == null)
            {
                TempData["Validation"] = "RecApiList cannot be empty.";
                return RedirectToAction("CreateAccount");
            }
            if (string.IsNullOrEmpty(RecApiConnID) || RecApiConnID == "" || RecApiConnID == null)
            {
                TempData["Validation"] = "RecApiConnID cannot be empty.";
                return RedirectToAction("CreateAccount");
            }

            bool Result = await _super.ExecuteQueryToCheckPrefixAsync(AccountPrefix , AccountName);
            if(Result==true)
            {
                TempData["Validation"] = "Prrfix Or Account Name Is Already Available Please try anather ";
                return RedirectToAction("CreateAccount");
            }
            else
            {
                await _super.InsertAccountAsync(AccountName, AccountPrefix, AuthenticationType ,  RecApiList, RecApiConnID);
                await _super.CreateAccountByScriptAsync(AccountName);
                TempData["Validation"] = "Account Is Created Sucessfully !.";
                return RedirectToAction("CreateAccount");

            }
            return RedirectToAction("CreateAccount");
        }
        public async Task<IActionResult> UpdateUserStatus([FromBody] dynamic request)
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
                await _super.UpdateUserStatusAsy(accountId, isActive);

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            return Json(new { success = true });
        }
        public async Task<JsonResult> GetAdminUsers(string AccountID)
        {
            var data = await _super.GetUserByAccountAsync(AccountID);
            return Json(data);
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
        public async Task<IActionResult> SaveUserDeactivation([FromBody] UserDeactivationRequest request)
        {
            if (request == null || request.Users == null || !request.Users.Any())
            {
                return BadRequest("No users provided for deactivation.");
            }
            else
            {
               await _super.DeactivateUserByAccountAsync(request);
                return Ok("Users deactivation updated successfully.");
            } 
        }
        public async Task<IActionResult> UpdateAccountStatus([FromBody] dynamic request)
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
                await _super.UpdateAccountStatusAsy(accountId, isActive);

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
            catch (Exception ex)
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
                await _super.CreateAdminAsync(model.UserName, model.Password, model.AccountID , model.Phone , model.Email);
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
