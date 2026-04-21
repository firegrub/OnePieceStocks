using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnePieceStocks.Data;

namespace OnePieceStocks.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeaderboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var players = await _context.Players
                .Include(p => p.Holdings)
                .ThenInclude(h => h.Character)
                .ToListAsync();

            var leaderboard = players
                .Select(p => new
                {
                    PlayerId = p.Id,
                    PlayerName = p.Name,
                    Wallet = p.Wallet,
                    StockValue = p.Holdings.Sum(h => h.Quantity * h.Character.CurrentPrice),
                    TotalWorth = p.Wallet + p.Holdings.Sum(h => h.Quantity * h.Character.CurrentPrice),
                    Holdings = p.Holdings.Select(h => new
                    {
                        CharacterName = h.Character.Name,
                        Quantity = h.Quantity,
                        Price = h.Character.CurrentPrice,
                        TotalValue = h.Quantity * h.Character.CurrentPrice
                    }).ToList()
                })
                .OrderByDescending(p => p.TotalWorth)
                .ToList();

            return View(leaderboard);
        }
    }
}