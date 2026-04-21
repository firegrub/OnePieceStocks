using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnePieceStocks.Data;

namespace OnePieceStocks.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rules = await _context.RulePages.FirstOrDefaultAsync();

            var gainers = await _context.Characters
                .Where(c => c.WeeklyIncreaseAmount > 0)
                .OrderByDescending(c => c.WeeklyIncreaseAmount)
                .Take(5)
                .ToListAsync();

            var losers = await _context.Characters
                .Where(c => c.WeeklyDecreaseAmount > 0)
                .OrderByDescending(c => c.WeeklyDecreaseAmount)
                .Take(5)
                .ToListAsync();

            ViewBag.WeeklyGainers = gainers;
            ViewBag.WeeklyLosers = losers;

            return View(rules);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}