using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnePieceStocks.Data;

namespace OnePieceStocks.Controllers
{
    public class WeeklyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WeeklyController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> TopEarners()
        {
            var players = await _context.Players
                .Include(p => p.Holdings)
                .ThenInclude(h => h.Character)
                .ToListAsync();

            var results = players.Select(p => new
            {
                PlayerName = p.Name,
                WeeklyStockGain = p.Holdings.Sum(h => h.Character.WeeklyIncreaseAmount - h.Character.WeeklyDecreaseAmount),
                Wallet = p.Wallet,
                TotalWorth = p.Wallet + p.Holdings.Sum(h => h.Quantity * h.Character.CurrentPrice)
            })
            .OrderByDescending(x => x.WeeklyStockGain)
            .ToList();

            ViewBag.TopThree = results.Take(3).ToList();

            return View(results);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetWeek()
        {
            var characters = await _context.Characters.ToListAsync();

            foreach (var character in characters)
            {
                character.WeeklyIncreaseAmount = 0;
                character.WeeklyDecreaseAmount = 0;
                character.WeeklyIncreaseCount = 0;
                character.WeeklyDecreaseCount = 0;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Weekly values reset.";
            return RedirectToAction(nameof(TopEarners));
        }
    }
}