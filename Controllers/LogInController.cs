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
                if (IsValid == 1)
                {
                    if (UserInfo.UserType == "Admin")
                    {
                        HttpContext.Session.SetString("UserType", "Admin");
                        return RedirectToAction("DashBoard", "Admin");
                    }
                    else if (UserInfo.UserType == "SiteAdmin")
                    {
                        HttpContext.Session.SetString("UserType", "SiteAdmin");
                        return RedirectToAction("DashBoard", "Admin");
                    }
                    else if (UserInfo.UserType == "QA Manager")
                    {
                        HttpContext.Session.SetString("UserType", "QA Manager");
                        return RedirectToAction("Dashboard", "QAManager");
                    }
                    else if (UserInfo.UserType == "Operation Manager")
                    {
                        HttpContext.Session.SetString("UserType", "Operation Manager");
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
                    return RedirectToAction("UserLogIn", "LogIn");
                }

                else
                {
                    TempData["LoginMessage"] = "User Invalid";
                    return RedirectToAction("UserLogIn", "LogIn");                
                }
            }
        }
        public async Task<ActionResult> Unauthorized()
        {
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
