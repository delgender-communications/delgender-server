using Microsoft.AspNetCore.Mvc;

namespace DelgenderCommunicationsAPI.Controllers
{
    public class StaffController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
