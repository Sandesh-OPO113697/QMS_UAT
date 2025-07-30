using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QMS.DataBaseService;
using QMS.Models;
using System.Data;

namespace QMS.Controllers
{
    public class LogInController : Controller
    {
        private readonly D_Login _login;
        private readonly DLConnection _dlcon;
        private readonly HttpResponse response;
        public LogInController(D_Login dl, DLConnection dlcon ,  IHttpContextAccessor httpContextAccessor)
        {
            _login = dl;
            _dlcon = dlcon;
            this.response = httpContextAccessor.HttpContext?.Response;
        }

        [HttpPost]
        public async Task<JsonResult> CheckUsername(string username)
        {
            try
            {
                // Fetch the data from your method
                DataTable result = await _login.getEmailAndPhone(username);
                bool isValid = result != null && result.Rows.Count > 0;

                // Convert DataTable to a List of dictionaries for easier serialization
                var rows = new List<Dictionary<string, object>>();
                if (isValid)
                {
                    foreach (DataRow row in result.Rows)
                    {
                        var rowDict = new Dictionary<string, object>();
                        foreach (DataColumn column in result.Columns)
                        {
                            rowDict[column.ColumnName] = row[column];
                        }
                        rows.Add(rowDict);
                    }
                }

                return Json(new
                {
                    success = isValid,
                    data = isValid ? rows : null
                });
            }
            catch (Exception ex)
            {
                // Optional: log the exception if logging is available
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }




        public async Task< ActionResult> UserLogIn()
        {
            
            response.Cookies.Delete("Token");
            return View();
        }
        public async Task<ActionResult> SendOTP(string email, string phone , string username)
        {
            try
            {
                Random generator = new Random();
                string OTPPhone = generator.Next(0, 9999).ToString("D4");
                string OTPEmail = generator.Next(0, 9999).ToString("D4");
               int rsult= await _login.SaveEmailPhoneOTP(OTPEmail, OTPPhone, email, phone, username);
                if (rsult==1)
                {

                    await _login.SendEmail(email, OTPEmail);
                    string result = await _login.SendOTPAsync(OTPPhone, phone);
                    return Json(new { success = true, message = "OTP sent successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Incorrect Email Or Phone" });
                }
              
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<JsonResult> ResetPassword(string emailOTP, string phoneOTP, string newPassword, string username)
        {
            int rsult = await _login.varifyEmailPhoneOTP(emailOTP, phoneOTP, username);
            if (rsult==1)
            {
                int Result = await _login.CheckUserIsValidAsync(username, newPassword);
                if (Result==1)
                {
                    return Json(new { success = true });
                }else
                {
                    return Json(new { success = false });
                }
               
            }
            else
            {
                return Json(new { success = false });
            }
       
        }
        [HttpPost]
        public async Task<JsonResult> ResetPasswordWithoutOTP( string newPassword, string username)
        {
          
                    int Result = await _login.ResetPasswordFDirstTimeAsync(username, newPassword);
                if (Result == 1)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false });
                }

            

        }

        [HttpPost]
        public async Task<ActionResult> Login(string Username, string Password)
        {
            int IsSuperAdmun = await _login.CheckSuperAdminIsValid(Username, Password );
            if (IsSuperAdmun == 1)
            {
                HttpContext.Session.SetString("UserType", "SuperAdmin");
                return RedirectToAction("Dashboard", "SuperAdmin");
                //await _login.AssignRoleToUser(Username, HttpContext);
            }
            else
            {
                await _login.AssignRoleToUser(Username, HttpContext);
                int IsValid = await _login.CheckAccountUserAsync(Username, Password);
                int NotePadAcess = await _login.IsNotePadAcess(Username);

                if (NotePadAcess == 1)
                {
                    HttpContext.Session.SetString("NotePadAcess", "Yes");
                }
                else
                {
                    HttpContext.Session.SetString("NotePadAcess", "NO");
                }
                if (IsValid == 1)
                {
                    if (UserInfo.UserType == "Admin")
                    {
                        HttpContext.Session.SetString("UserType", "Admin");
                        return RedirectToAction("DashBoard", "QAManager");
                    }
                    else if (UserInfo.UserType == "SiteAdmin")
                    {
                        HttpContext.Session.SetString("UserType", "SiteAdmin");
                        return RedirectToAction("DashBoard", "QAManager");
                    }
                    else if (UserInfo.UserType == "Leadership")
                    {
                        HttpContext.Session.SetString("UserType", "Leadership");
                        return RedirectToAction("Dashboard", "Leadership");
                    }
                    else if (UserInfo.UserType == "QA Manager")
                    {
                        HttpContext.Session.SetString("UserType", "QA Manager");
                        return RedirectToAction("Dashboard", "QAManager");
                    }
                    else if (UserInfo.UserType == "Monitor Supervsior")
                    {
                        HttpContext.Session.SetString("UserType", "Monitor Supervsior");
                        return RedirectToAction("Dashboard", "MonitorSupervsior");
                    }

                    else if (UserInfo.UserType == "Monitor")
                    {
                        HttpContext.Session.SetString("UserType", "QA Manage");
                        return RedirectToAction("Dashboard", "Monitor");
                    }

                    else if (UserInfo.UserType == "Operation Manager")
                    {
                        HttpContext.Session.SetString("UserType", "AccountUser");
                        return RedirectToAction("Dashboard", "Operation");
                    }

                    else if (UserInfo.UserType == "HR")
                    {
                        HttpContext.Session.SetString("UserType", "HR");
                        return RedirectToAction("Dashboard", "HR");
                    }

                    else if (UserInfo.UserType == "Agent")
                    {
                        HttpContext.Session.SetString("UserType", "Agent");
                        return RedirectToAction("Dashboard", "Agent");
                    }
                    else
                    {
                        HttpContext.Session.SetString("UserType", "AccountUser");
                        return RedirectToAction("DashBoard", "Admin");
                    }
                   
                }

                else if (IsValid == 2)
                {

                    TempData["Reset"] = "Please Reset your Password";

                    TempData["Username"] = Username;
                    return RedirectToAction("UserLogIn", "LogIn");
                }

                else
                {
                    TempData["LoginMessage"] = "User Invalid";
                    return RedirectToAction("UserLogIn", "LogIn");                
                }
            }
        }
        public IActionResult RedirectBasedOnUser()
        {
            var userType = UserInfo.UserType;

            switch (userType)
            {
                case "Admin":
                    HttpContext.Session.SetString("UserType", "Admin");
                    return RedirectToAction("DashBoard", "QAManager");

                case "SiteAdmin":
                    HttpContext.Session.SetString("UserType", "SiteAdmin");
                    return RedirectToAction("DashBoard", "QAManager");

                case "Leadership":
                    HttpContext.Session.SetString("UserType", "Leadership");
                    return RedirectToAction("Dashboard", "Leadership");

                case "QA Manager":
                    HttpContext.Session.SetString("UserType", "QA Manager");
                    return RedirectToAction("Dashboard", "QAManager");

                case "Monitor Supervsior":
                    HttpContext.Session.SetString("UserType", "Monitor Supervsior");
                    return RedirectToAction("Dashboard", "MonitorSupervsior");

                case "Monitor":
                    HttpContext.Session.SetString("UserType", "Monitor");
                    return RedirectToAction("Dashboard", "Monitor");

                case "Operation Manager":
                    HttpContext.Session.SetString("UserType", "AccountUser");
                    return RedirectToAction("Dashboard", "Operation");

                case "HR":
                    HttpContext.Session.SetString("UserType", "HR");
                    return RedirectToAction("Dashboard", "HR");

                case "Agent":
                    HttpContext.Session.SetString("UserType", "Agent");
                    return RedirectToAction("Dashboard", "Agent");

                default:
                    HttpContext.Session.SetString("UserType", "AccountUser");
                    return RedirectToAction("DashBoard", "Admin");
            }
        }

        [Route("Error")]
        public async Task<ActionResult> Unauthorized( string message)
        {

            ViewBag.ErrorMassge = "An unexpected error occurred. Please try again later.";
            return View();
        }
        public IActionResult DownloadAttachment(string fileName)
        {
            // Fetch file from DB using the filename
            DataTable dt = _login.GetNotificationByIser(UserInfo.UserName).Result;
            var fileRow = dt.AsEnumerable().FirstOrDefault(r => r["AttachmentFileName"].ToString() == fileName);

            if (fileRow != null)
            {
                byte[] fileBytes = (byte[])fileRow["Files"];
                return File(fileBytes, "application/octet-stream", fileName);
            }

            return NotFound();
        }

        [HttpGet]
        public async  Task<IActionResult> NotificationClosed()
        {
            await _login.EndNotificationByuser(UserInfo.UserName);

            return Ok("Success");
        }

        public async Task<IActionResult> GetNotification()
        {
            DataTable dt = await _login.GetNotificationByIser(UserInfo.UserName);

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                string fileString = row["Files"]?.ToString();
                var result = new
                {
                    UserCode = row["UserCode"].ToString(),
                    Subject = row["Subject"].ToString(),
                    Body = row["Body"].ToString(),
                    attachmentFileName = row["AttachmentFileName"].ToString(),
                    fileBase64 = fileString // Already a string
                };

                return Json(result);
            }

            return Json(null);
        }





    }
}
