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
            // Default name
            string fullName = "User";

            // If user is logged in, try to get their full name
            if (User.Identity.IsAuthenticated)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (user != null)
                {
                    fullName = user.FullName;
                }
            }

            // Pass FullName to View
            ViewData["FullName"] = fullName;

            // Get latest 6 movies
            var movies = await _context.Movies
                .OrderByDescending(m => m.ReleaseDate)
                .Take(6)
                .ToListAsync();

            // Pass movies list to View
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
        public IActionResult UserAction()
        {
            return View();
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

        public IActionResult TestDb()
        {
            bool canConnect = _context.Database.CanConnect();
            return Content(canConnect ? "Connected to DB" : "Failed to connect to DB");
        }
    }
}
