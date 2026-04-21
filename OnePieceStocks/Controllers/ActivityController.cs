using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnePieceStocks.Data;

namespace OnePieceStocks.Controllers
{
    public class ActivityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivityController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _context.ActivityLogs
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return View(logs);
        }
    }
}