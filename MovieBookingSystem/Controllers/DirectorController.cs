using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingSystem.Data;
using MovieBookingSystem.Models;
using System.Threading.Tasks;
using System.Linq;

namespace MovieBookingSystem.Controllers
{
    public class DirectorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DirectorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Director
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

        // GET: Director/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var director = await _context.Directors.FirstOrDefaultAsync(m => m.DirectorId == id);
            if (director == null) return NotFound();

            return View(director);
        }

        // GET: Director/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Director/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DirectorId,Name,Biography,DateOfBirth,ImageUrl")] Director director)
        {
            if (ModelState.IsValid)
            {
                _context.Add(director);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(director);
        }

        // GET: Director/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var director = await _context.Directors.FindAsync(id);
            if (director == null) return NotFound();

            return View(director);
        }

        // POST: Director/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DirectorId,Name,Biography,DateOfBirth,ImageUrl")] Director director)
        {
            if (id != director.DirectorId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(director);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Directors.Any(e => e.DirectorId == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(director);
        }

        // GET: Director/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var director = await _context.Directors.FirstOrDefaultAsync(m => m.DirectorId == id);
            if (director == null) return NotFound();

            return View(director);
        }

        // POST: Director/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var director = await _context.Directors.FindAsync(id);
            _context.Directors.Remove(director);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
