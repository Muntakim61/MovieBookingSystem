﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingSystem.Data;
using MovieBookingSystem.Models;
using System.Data;
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
        public async Task<IActionResult> GetActors(int page = 1, int size = 10)
        {
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "GetActorsPaged";         
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

            var actors = new List<object>();

            
            while (await reader.ReadAsync())
            {
                actors.Add(new
                {
                    actorId = reader.GetInt32(0),
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
                data = actors,
                last_page = (int)Math.Ceiling((double)totalCount / size)
            });
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

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var actor = await _context.Actors.FindAsync(id);

            if (actor == null)
                return NotFound();

            return View(actor);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ActorId,Name,Biography,DateOfBirth,ImageUrl")] Actor actor)
        {
            if (id != actor.ActorId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(actor);

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

    }
}
