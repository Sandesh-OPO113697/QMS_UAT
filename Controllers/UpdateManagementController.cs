using Microsoft.AspNetCore.Mvc;
using QMS.DataBaseService;

namespace QMS.Controllers
{
    public class UpdateManagementController : Controller
    {
        private readonly DlSampling dlSampling;
        private readonly Dl_Admin _admin;
        private readonly Dl_formBuilder dl_FormBuilder;
        private readonly dl_Monitoring dl_monitor;
        public UpdateManagementController(Dl_formBuilder adl, DlSampling dl, Dl_Admin adam, dl_Monitoring dmmonoi)
        {
            dlSampling = dl;
            _admin = adam;
            dl_FormBuilder = adl;
            dl_monitor = dmmonoi;
        }

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
