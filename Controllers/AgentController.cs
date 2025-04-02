using Microsoft.AspNetCore.Mvc;

namespace QMS.Controllers
{
    public class AgentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
