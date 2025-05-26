using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingSystem.Data;
using MovieBookingSystem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MovieBookingSystem.Controllers
{
    public class HallController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HallController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Halls.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> GetHalls()
        {
            var halls = await _context.Halls
                .Select(h => new {
                    hallId = h.HallId,
                    name = h.Name,
                    capacity = h.Capacity,
                    location = h.Location
                }).ToListAsync();

            return Json(halls);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Capacity,Location")] Hall hall)
        {
            if (!ModelState.IsValid) return View(hall);

            _context.Halls.Add(hall);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hall = await _context.Halls.FindAsync(id);
            if (hall == null) return NotFound();

            return View(hall);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HallId,Name,Capacity,Location")] Hall hall)
        {
            if (id != hall.HallId) return NotFound();
            if (!ModelState.IsValid) return View(hall);

            _context.Update(hall);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hall = await _context.Halls.FirstOrDefaultAsync(h => h.HallId == id);
            if (hall == null) return NotFound();

            return View(hall);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hall = await _context.Halls.FindAsync(id);
            _context.Halls.Remove(hall);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
