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
        private readonly AppDbContext _context;

        public MovieController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ UPDATED to send movie list to Index view
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.Include(m => m.Director).ToListAsync();
            return View(movies);
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
                m.ImageUrl,
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

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = await _context.Movies
                .Include(m => m.MovieActors)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
                return NotFound();

            var vm = new MovieEditViewModel
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Genre = movie.Genre,
                ReleaseDate = movie.ReleaseDate,
                Duration = movie.Duration,
                Price = movie.Price,
                ImageUrl = movie.ImageUrl,
                SelectedDirectorId = movie.DirectorId ?? 0,
                SelectedActorIds = movie.MovieActors?.Select(ma => ma.ActorId).ToList(),

                Directors = _context.Directors.Select(d => new SelectListItem
                {
                    Value = d.DirectorId.ToString(),
                    Text = d.Name
                }).ToList(),

                Actors = _context.Actors.Select(a => new SelectListItem
                {
                    Value = a.ActorId.ToString(),
                    Text = a.Name
                }).ToList()
            };

            return View(vm);
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

        // GET: /Movie/DownloadExcel
        [HttpGet]
        public async Task<IActionResult> DownloadExcel()
        {
            var movies = await _context.Movies
                .Include(m => m.Director)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .ToListAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Movies");

            // Header
            worksheet.Cell(1, 1).Value = "Title";
            worksheet.Cell(1, 2).Value = "Genre";
            worksheet.Cell(1, 3).Value = "Release Date";
            worksheet.Cell(1, 4).Value = "Duration";
            worksheet.Cell(1, 5).Value = "Price";
            worksheet.Cell(1, 6).Value = "Director";
            worksheet.Cell(1, 7).Value = "Image URL";

            // Data
            for (int i = 0; i < movies.Count; i++)
            {
                var m = movies[i];
                worksheet.Cell(i + 2, 1).Value = m.Title;
                worksheet.Cell(i + 2, 2).Value = m.Genre;
                worksheet.Cell(i + 2, 3).Value = m.ReleaseDate.ToShortDateString();
                worksheet.Cell(i + 2, 4).Value = m.Duration;
                worksheet.Cell(i + 2, 5).Value = m.Price;
                worksheet.Cell(i + 2, 6).Value = m.Director?.Name ?? "";
                worksheet.Cell(i + 2, 7).Value = m.ImageUrl ?? "";
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Movies.xlsx");
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
