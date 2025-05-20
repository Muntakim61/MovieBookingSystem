using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingSystem.Data;
using MovieBookingSystem.Models;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace MovieBookingSystem.Controllers
{
    public class DirectorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DirectorController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index()
        {
            return View(await _context.Directors.ToListAsync());
        }
        [HttpGet]
        public async Task<IActionResult> GetDirectors()
        {
            var directors = await _context.Directors
                .Select(a => new
                {
                    actorId = a.DirectorId,
                    name = a.Name,
                    biography = a.Biography,
                    dateOfBirth = a.DateOfBirth.ToString("yyyy-MM-dd"),
                    imageUrl = a.ImageUrl
                }).ToListAsync();

            return Json(directors);
        }

       
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var director = await _context.Directors.FirstOrDefaultAsync(m => m.DirectorId == id);
            if (director == null) return NotFound();

            return View(director);
        }

        
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Biography,DateOfBirth,ImageUrl")] Director director)
        {
            TempData["ConsoleMessage"] = "Entered Create POST action.";

            if (!ModelState.IsValid)
            {
                TempData["ConsoleMessage"] = "ModelState is invalid.";
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ValidationErrors"] = string.Join("; ", errors);
                return View(director);
            }

            TempData["ConsoleMessage"] = "ModelState is valid. Adding director to context.";
            _context.Add(director);

            TempData["ConsoleMessage"] = "Saving changes to database.";
            await _context.SaveChangesAsync();

            TempData["ConsoleMessage"] = "Director saved successfully. Redirecting to Index.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var director = await _context.Directors
                .FirstOrDefaultAsync(a => a.DirectorId == id);

            if (director == null)
                return NotFound();

            return View(director);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var director = await _context.Directors
                .Include(a => a.Movies)
                .FirstOrDefaultAsync(a => a.DirectorId == id);

            if (director == null)
                return NotFound();


            //_context.Movies.RemoveRange(director.Movies);

            _context.Directors.Remove(director);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
