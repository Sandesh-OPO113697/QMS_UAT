using Microsoft.AspNetCore.Mvc;

namespace QMS.Controllers
{
    public class SamplingController : Controller
    {
        public IActionResult DashBoard(int SubFeatureid, string RoleName, int Featureid)
        {
            return View();
        }
    }
}
