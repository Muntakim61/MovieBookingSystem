using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieBookingSystem.Data;
using MovieBookingSystem.Models;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;



namespace MovieBookingSystem.Controllers
{
    public class MovieController : Controller
    {
        private readonly AppDbContext _context;

        public MovieController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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
                DirectorId = vm.SelectedDirectorId,
                ImageUrl = vm.ImageUrl
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


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetMovies(int page = 1, int size = 10)
        {
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "GetMoviesPaged";
            command.CommandType = CommandType.StoredProcedure;

            var paramPage = command.CreateParameter();
            paramPage.ParameterName = "@PageNumber";
            paramPage.Value = page;
            command.Parameters.Add(paramPage);

            var paramSize = command.CreateParameter();
            paramSize.ParameterName = "@PageSize";
            paramSize.Value = size;
            command.Parameters.Add(paramSize);

            using var reader = await command.ExecuteReaderAsync();

            var movies = new List<object>();
            while (await reader.ReadAsync())
            {
                movies.Add(new
                {
                    movieId = reader.GetInt32(0),
                    title = reader.GetString(1),
                    genre = reader.GetString(2),
                    releaseDate = reader.GetDateTime(3).ToString("yyyy-MM-dd"),
                    imageUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                    duration = reader.GetInt32(5),
                    price = reader.GetDecimal(6),
                    directorName = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
            }

            await reader.NextResultAsync();

            int totalCount = 0;
            if (await reader.ReadAsync())
            {
                totalCount = reader.GetInt32(0);
            }

            return Json(new
            {
                data = movies,
                last_page = (int)Math.Ceiling((double)totalCount / size)
            });
        }


        [Authorize(Roles = "Admin")]
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

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = await _context.Movies
                .Include(m => m.Director)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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
            movie.ImageUrl = vm.ImageUrl;

            
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
