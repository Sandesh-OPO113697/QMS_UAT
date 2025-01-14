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
        public ActionResult UserLogIn()
        {
            response.Cookies.Delete("Token");
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ResetPassword(string Username, string Password)
        {

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                return Json(new { success = false, message = "Username and Password are required." });
            }
            else
            {
                int Result = await _login.CheckUserIsValidAsync(Username , Password);
                if (Result == 1)
                {
                    return Json(new { success = true, message = "Password reset successful!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to reset password. Please try again later." });
                }
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
                    }
                    else if (UserInfo.UserType == "QE")
                    {
                        HttpContext.Session.SetString("UserType", "QE");
                    }
                    else
                    {
                        HttpContext.Session.SetString("UserType", "AccountUser");
                    }
                    return RedirectToAction("DashBoard", "Admin");
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
