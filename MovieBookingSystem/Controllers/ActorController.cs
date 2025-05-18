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

        // GET: Actor
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

        // GET: Actor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FirstOrDefaultAsync(m => m.ActorId == id);
            if (actor == null) return NotFound();

            return View(actor);
        }

        // GET: Actor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Biography,DateOfBirth,ImageUrl")] Actor actor)
        {
            TempData["ConsoleMessage"] = "Entered Create POST action.";

            if (!ModelState.IsValid)
            {
                TempData["ConsoleMessage"]="ModelState is invalid.";
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation Error: {error}");
                }
                return View(actor);
            }

            Console.WriteLine("ModelState is valid. Adding actor to context.");
            _context.Add(actor);

            Console.WriteLine("Saving changes to database.");
            await _context.SaveChangesAsync();

            Console.WriteLine("Actor saved successfully. Redirecting to Index.");
            return RedirectToAction(nameof(Index));
        }


        // GET: Actor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null) return NotFound();

            return View(actor);
        }

        // POST: Actor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ActorId,Name,Biography,DateOfBirth,ImageUrl")] Actor actor)
        {
            if (id != actor.ActorId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Actors.Any(e => e.ActorId == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FirstOrDefaultAsync(m => m.ActorId == id);
            if (actor == null) return NotFound();

            return View(actor);
        }

        // POST: Actor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            _context.Actors.Remove(actor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
