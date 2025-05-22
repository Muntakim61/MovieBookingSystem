using Microsoft.AspNetCore.Mvc;

namespace MovieBookingSystem.Controllers
{
    public class HallController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
