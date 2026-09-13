using Microsoft.AspNetCore.Mvc;

namespace DelgenderCommunicationsAPI.Controllers
{
    public class InvoicesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
