using Microsoft.AspNetCore.Mvc;

namespace DelgenderCommunicationsAPI.Controllers
{
    public class CustomersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
