using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnePieceStocks.Data;
using OnePieceStocks.Models;

namespace OnePieceStocks.Controllers
{
    [Authorize]
    public class BankruptcyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BankruptcyController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var username = User.Identity?.Name;
            var player = await _context.Players
                .Include(p => p.Holdings)
                .FirstOrDefaultAsync(p => p.UserId == username);

            return View(player);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Declare()
        {
            var username = User.Identity?.Name;
            var player = await _context.Players
                .Include(p => p.Holdings)
                .FirstOrDefaultAsync(p => p.UserId == username);

            if (player == null)
            {
                return NotFound();
            }

            _context.Holdings.RemoveRange(player.Holdings);

            player.Wallet = 10000;
            player.TotalSlots = 6;
            player.Notes = $"{player.Notes} | Bankruptcy reset on {DateTime.UtcNow:g}";

            _context.ActivityLogs.Add(new ActivityLog
            {
                ActivityType = "Bankruptcy",
                Message = $"{player.Name} declared bankruptcy and was reset."
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bankruptcy declared. Account reset.";
            return RedirectToAction(nameof(Index));
        }
    }
}