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
        public LogInController(D_Login dl)
        {
            _login=dl;
        }
        public ActionResult UserLogIn()
        {
            return View();
        }

        [HttpPost]
        public async Task< ActionResult> Login(string Username, string Password)
        {
           int IsValid=  await _login.CheckUserLogInAsync(Username , Password);
            
            if(IsValid ==1)
            {
                HttpContext.Session.SetString("UserType", "SuperAdmin");
            }
            else
            {
                int Result = await _login.CheckAccountUserAsync( Username,  Password);
                if (UserInfo.UserType=="Admin")
                {
                    HttpContext.Session.SetString("UserType", "Admin");

                }
                if (UserInfo.UserType == "QE")
                {
                    HttpContext.Session.SetString("UserType", "QE");

                }
              


            }
            return RedirectToAction("Dashboard", "SuperAdmin");

        }
      


    }
}
