using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieBookingSystem.Data;
using MovieBookingSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace MovieBookingSystem.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovieController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Movie/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Movie/Create
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new CreateMovieViewModel
            {
                Directors = _context.Directors.Select(d => new SelectListItem
                {
                    Value = d.DirectorId.ToString(),
                    Text = d.Name
                }).ToList(),

                Actors = _context.Actors.Select(a => new SelectListItem
                {
                    Value = a.ActorId.ToString(),
                    Text = a.Name
                }).ToList(),

                ReleaseDate = DateTime.Today
            };

            return View(vm);
        }

        // POST: /Movie/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMovieViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Directors = _context.Directors.Select(d => new SelectListItem
                {
                    Value = d.DirectorId.ToString(),
                    Text = d.Name
                }).ToList();

                vm.Actors = _context.Actors.Select(a => new SelectListItem
                {
                    Value = a.ActorId.ToString(),
                    Text = a.Name
                }).ToList();

                return View(vm);
            }

            var movie = new Movie
            {
                Title = vm.Title,
                Genre = vm.Genre,
                ReleaseDate = vm.ReleaseDate,
                Duration = vm.Duration,
                Price = vm.Price,
                DirectorId = vm.SelectedDirectorId
            };

            if (vm.SelectedActorIds != null)
            {
                movie.MovieActors = vm.SelectedActorIds.Select(actorId => new MovieActor
                {
                    ActorId = actorId
                }).ToList();
            }

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Movie/GetMovies
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

        // POST: /Movie/SaveMovie
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

        // POST: /Movie/DeleteMovie
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

        // POST: /Movie/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MovieEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Directors = _context.Directors.Select(d => new SelectListItem
                {
                    Value = d.DirectorId.ToString(),
                    Text = d.Name
                }).ToList();

                vm.Actors = _context.Actors.Select(a => new SelectListItem
                {
                    Value = a.ActorId.ToString(),
                    Text = a.Name
                }).ToList();

                return View(vm);
            }

            var movie = _context.Movies.Include(m => m.MovieActors).FirstOrDefault(m => m.MovieId == vm.MovieId);
            if (movie == null) return NotFound();

            movie.Title = vm.Title;
            movie.Genre = vm.Genre;
            movie.ReleaseDate = vm.ReleaseDate;
            movie.Duration = vm.Duration;
            movie.Price = vm.Price;
            movie.DirectorId = vm.SelectedDirectorId;

            // Replace movie actors
            movie.MovieActors.Clear();
            foreach (var actorId in vm.SelectedActorIds)
            {
                movie.MovieActors.Add(new MovieActor { MovieId = movie.MovieId, ActorId = actorId });
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
