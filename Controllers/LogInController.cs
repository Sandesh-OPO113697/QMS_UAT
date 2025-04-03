using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QMS.DataBaseService;
using QMS.Models;

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
                    else
                    {
                        HttpContext.Session.SetString("UserType", "AccountUser");
                        return RedirectToAction("DashBoard", "Admin");
                    }
                   
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
    }
}
