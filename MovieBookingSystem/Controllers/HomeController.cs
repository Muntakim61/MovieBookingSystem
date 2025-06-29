using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieBookingSystem.Data;
using MovieBookingSystem.Models;
using System.Diagnostics;

namespace MovieBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            string fullName = "User";

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (user != null)
                {
                    fullName = user.FullName;
                }
            }
       
            ViewData["FullName"] = fullName;
          
            var movies = await _context.Movies
                .OrderByDescending(m => m.ReleaseDate)
                .Take(6)
                .ToListAsync();

            return View(movies);
        }


        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return View();
        }

        [Authorize(Roles = "User")]
        public IActionResult UserDashboard()
        {
            return View("User"); ;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var errorModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(errorModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubscribeNewsletter(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["ToastMessage"] = "Please enter a valid email address.";
                TempData["ToastType"] = "error";
            }
            else
            {
                TempData["ToastMessage"] = "Thank you for subscribing!";
                TempData["ToastType"] = "success";
            }

            return RedirectToAction("Index");
        }


        public IActionResult TestDb()
        {
            bool canConnect = _context.Database.CanConnect();
            return Content(canConnect ? "Connected to DB" : "Failed to connect to DB");
        }
    }
}
