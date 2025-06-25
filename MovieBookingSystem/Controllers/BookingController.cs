using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieBookingSystem.Data;
using MovieBookingSystem.Models;
using MovieBookingSystem.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace MovieBookingSystem.Controllers
{
    [Authorize] 
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly BookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(AppDbContext context, BookingService bookingService, ILogger<BookingController> logger)
        {
            _context = context;
            _bookingService = bookingService;
            _logger = logger;
        }

        // GET: Booking/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? movieId)
        {
            ViewData["Movies"] = new SelectList(await _context.Movies.ToListAsync(), "MovieId", "Title");
            ViewData["Halls"] = new SelectList(await _context.Halls.ToListAsync(), "HallId", "Name");

            if (movieId != null)
                ViewData["SelectedMovieId"] = movieId;

            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (!ModelState.IsValid)
            {
                TempData["BookingError"] = "Validation failed. Please check your input.";
                return RedirectToAction("Index", "Home");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["BookingError"] = "You must be logged in to book.";
                return RedirectToAction("Index", "Home");
            }

            booking.UserId = userId;
            booking.BookingDate = DateTime.Now;
            booking.Status = BookingStatus.Pending;

            try
            {
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction("UserBookings");
            }
            catch
            {
                TempData["BookingError"] = "Something went wrong while saving your booking.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Booking/UserBookings
        [HttpGet]
        public async Task<IActionResult> UserBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bookings = await _context.Bookings
                .Include(b => b.Movie)
                .Include(b => b.Hall)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            return View(bookings);
        }

        // GET: Booking/AdminPending  <-- Add this missing method
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AdminPending()
        {
            var pendingBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Movie)
                .Include(b => b.Hall)
                .Where(b => b.Status == BookingStatus.Pending)
                .ToListAsync();

            return View(pendingBookings);
        }

        // POST: Booking/Approve/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var (success, message) = await _bookingService.ApproveBookingAsync(id);
            if (!success)
            {
                TempData["Error"] = message;
            }
            else
            {
                TempData["Success"] = message;
            }
            return RedirectToAction(nameof(AdminPending));
        }

        // POST: Booking/Reject/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AdminPending));
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == id && b.UserId == userId);

            if (booking == null) return NotFound();

            if (booking.Status != BookingStatus.Pending)
            {
                TempData["Error"] = "Only pending bookings can be cancelled.";
                return RedirectToAction(nameof(UserBookings));
            }

            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking cancelled successfully.";
            return RedirectToAction(nameof(UserBookings));
        }

    }
}
