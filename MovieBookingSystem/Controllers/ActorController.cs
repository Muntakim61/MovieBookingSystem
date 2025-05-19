using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingSystem.Data;
using MovieBookingSystem.Models;
using System.Threading.Tasks;

namespace MovieBookingSystem.Controllers
{
    public class ActorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.ToListAsync());
        }
        [HttpGet]
        public async Task<IActionResult> GetActors()
        {
            var actors = await _context.Actors
                .Select(a => new
                {
                    actorId = a.ActorId,
                    name = a.Name,
                    biography = a.Biography,
                    dateOfBirth = a.DateOfBirth.ToString("yyyy-MM-dd"),
                    imageUrl = a.ImageUrl
                }).ToListAsync();

            return Json(actors);
        }

        
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FirstOrDefaultAsync(m => m.ActorId == id);
            if (actor == null) return NotFound();

            return View(actor);
        }

        
        public IActionResult Create()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Biography,DateOfBirth,ImageUrl")] Actor actor)
        {
            TempData["ConsoleMessage"] = "Entered Create POST action.";

            if (!ModelState.IsValid)
            {
                TempData["ConsoleMessage"] = "ModelState is invalid.";
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ValidationErrors"] = string.Join("; ", errors);
                return View(actor);
            }

            TempData["ConsoleMessage"] = "ModelState is valid. Adding actor to context.";
            _context.Add(actor);

            TempData["ConsoleMessage"] = "Saving changes to database.";
            await _context.SaveChangesAsync();

            TempData["ConsoleMessage"] = "Actor saved successfully. Redirecting to Index.";
            return RedirectToAction(nameof(Index));
        }

      
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var actor = await _context.Actors
                .FirstOrDefaultAsync(a => a.ActorId == id);

            if (actor == null)
                return NotFound();

            return View(actor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors
                .Include(a => a.MovieActors)
                .FirstOrDefaultAsync(a => a.ActorId == id);

            if (actor == null)
                return NotFound();

            
            _context.MovieActors.RemoveRange(actor.MovieActors);

            _context.Actors.Remove(actor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
