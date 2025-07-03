using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingSystem.Data;
using MovieBookingSystem.Models;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MovieBookingSystem.Controllers
{
    public class HallController : Controller
    {
        private readonly AppDbContext _context;

        public HallController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Halls.ToListAsync());
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> GetHalls(int page = 1, int size = 10)
        {
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "GetHallsPaged";
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

            var halls = new List<object>();
            while (await reader.ReadAsync())
            {
                halls.Add(new
                {
                    hallId = reader.GetInt32(0),
                    name = reader.GetString(1),
                    capacity = reader.GetInt32(2),
                    location = reader.GetString(3)
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
                data = halls,
                last_page = (int)Math.Ceiling((double)totalCount / size)
            });
        }


        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Capacity,Location")] Hall hall)
        {
            if (!ModelState.IsValid) return View(hall);

            _context.Halls.Add(hall);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hall = await _context.Halls.FindAsync(id);
            if (hall == null) return NotFound();

            return View(hall);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HallId,Name,Capacity,Location")] Hall hall)
        {
            if (id != hall.HallId) return NotFound();
            if (!ModelState.IsValid) return View(hall);

            _context.Update(hall);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hall = await _context.Halls.FirstOrDefaultAsync(h => h.HallId == id);
            if (hall == null) return NotFound();

            return View(hall);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExcel()
        {
            var halls = await _context.Halls.ToListAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Halls");

            // Header
            worksheet.Cell(1, 1).Value = "Name";
            worksheet.Cell(1, 2).Value = "Capacity";
            worksheet.Cell(1, 3).Value = "Location";

            // Data
            for (int i = 0; i < halls.Count; i++)
            {
                var h = halls[i];
                worksheet.Cell(i + 2, 1).Value = h.Name;
                worksheet.Cell(i + 2, 2).Value = h.Capacity;
                worksheet.Cell(i + 2, 3).Value = h.Location;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Halls.xlsx");
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hall = await _context.Halls.FindAsync(id);
            _context.Halls.Remove(hall);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
