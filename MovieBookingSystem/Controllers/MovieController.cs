using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingSystem.Data;
using MovieBookingSystem.Models;

namespace MovieBookingSystem.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovieController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ✅ GET: for Tabulator fetch (read)
        [HttpGet]
        public async Task<IActionResult> GetMovies()
        {
            var movies = await _context.Movies
                .Include(m => m.Director)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .ToListAsync();

            var movieList = movies.Select(m => new
            {
                m.MovieId,
                m.Title,
                m.Genre,
                m.ReleaseDate,
                m.Duration,
                m.Price,
                DirectorName = m.Director.Name,
                ActorIds = m.MovieActors.Select(ma => ma.ActorId)
            });

            return Json(movieList);
        }

        // ✅ POST: Create or Update
        [HttpPost]
        public async Task<IActionResult> SaveMovie([FromBody] Movie movie)
        {
            if (movie.MovieId == 0)
            {
                _context.Movies.Add(movie);
            }
            else
            {
                _context.Movies.Update(movie);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ✅ DELETE
        [HttpPost]
        public async Task<IActionResult> DeleteMovie([FromBody] int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieActors)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
                return Json(new { success = false });

            _context.MovieActors.RemoveRange(movie.MovieActors);
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
