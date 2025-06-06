﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingSystem.Data;
using MovieBookingSystem.Models;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Data;

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
        public async Task<IActionResult> GetDirectors(int page = 1, int size = 10)
        {
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "GetDirectorsPaged";
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

            var directors = new List<object>();
            while (await reader.ReadAsync())
            {
                directors.Add(new
                {
                    directorId = reader.GetInt32(0),
                    name = reader.GetString(1),
                    biography = reader.GetString(2),
                    dateOfBirth = reader.GetString(3),
                    imageUrl = reader.GetString(4)
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
                data = directors,
                last_page = (int)Math.Ceiling((double)totalCount / size)
            });
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

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var director = await _context.Directors.FindAsync(id);

            if (director == null)
                return NotFound();

            return View(director);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DirectorId,Name,Biography,DateOfBirth,ImageUrl")] Director director)
        {
            if (id != director.DirectorId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(director);

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


    }
}
