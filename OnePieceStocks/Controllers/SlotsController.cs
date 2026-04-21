using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnePieceStocks.Data;
using OnePieceStocks.Models;

namespace OnePieceStocks.Controllers
{
    [Authorize]
    public class SlotsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SlotsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            await LoadPlayers();

            if (!User.IsInRole("Admin"))
            {
                var username = User.Identity?.Name;
                var player = await _context.Players
                    .Include(p => p.Holdings)
                    .FirstOrDefaultAsync(p => p.UserId == username);

                if (player == null)
                {
                    ViewBag.NoLinkedPlayer = true;
                    return View();
                }

                ViewBag.CurrentPlayer = player;
                ViewBag.UsedSlots = player.Holdings.Count(h => h.Quantity > 0);
                ViewBag.NextBuyCost = await GetNextBuyCost(player.TotalSlots);
                ViewBag.NextSellValue = GetSellValue(player.TotalSlots);
                return View();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int playerId)
        {
            var player = await GetAllowedPlayer(playerId);
            if (player == null)
            {
                return Forbid();
            }

            var settings = await GetSettings();
            var cost = await GetNextBuyCost(player.TotalSlots);

            if (cost == null)
            {
                TempData["ErrorMessage"] = "No more slots can be bought.";
                return RedirectToAction(nameof(Index));
            }

            if (player.Wallet < cost.Value)
            {
                TempData["ErrorMessage"] = "Not enough money to buy the next slot.";
                return RedirectToAction(nameof(Index));
            }

            player.Wallet -= cost.Value;
            player.TotalSlots += 1;

            _context.ActivityLogs.Add(new ActivityLog
            {
                ActivityType = "Slot Change",
                Message = $"{player.Name} bought slot {player.TotalSlots} for {cost.Value:N0}."
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Slot purchased for {cost.Value:N0}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sell(int playerId)
        {
            var player = await GetAllowedPlayer(playerId);
            if (player == null)
            {
                return Forbid();
            }

            player = await _context.Players
                .Include(p => p.Holdings)
                .FirstOrDefaultAsync(p => p.Id == player.Id);

            if (player == null)
            {
                return NotFound();
            }

            var usedSlots = player.Holdings.Count(h => h.Quantity > 0);
            if (player.TotalSlots - 1 < usedSlots)
            {
                TempData["ErrorMessage"] = "Cannot sell a slot that is currently needed.";
                return RedirectToAction(nameof(Index));
            }

            if (player.TotalSlots <= 0)
            {
                TempData["ErrorMessage"] = "No slots available to sell.";
                return RedirectToAction(nameof(Index));
            }

            var sellValue = GetSellValue(player.TotalSlots);
            if (sellValue <= 0)
            {
                TempData["ErrorMessage"] = "That slot cannot be sold.";
                return RedirectToAction(nameof(Index));
            }

            int soldSlotNumber = player.TotalSlots;
            player.TotalSlots -= 1;
            player.Wallet += sellValue;

            _context.ActivityLogs.Add(new ActivityLog
            {
                ActivityType = "Slot Change",
                Message = $"{player.Name} sold slot {soldSlotNumber} for {sellValue:N0}."
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Slot sold for {sellValue:N0}.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadPlayers()
        {
            if (User.IsInRole("Admin"))
            {
                var players = await _context.Players
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                ViewBag.PlayerId = new SelectList(players, "Id", "Name");
            }
        }

        private async Task<Player?> GetAllowedPlayer(int playerId)
        {
            if (User.IsInRole("Admin"))
            {
                return await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);
            }

            var username = User.Identity?.Name;
            return await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId && p.UserId == username);
        }

        private async Task<GameSettings> GetSettings()
        {
            var settings = await _context.GameSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new GameSettings();
                _context.GameSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return settings;
        }

        private async Task<decimal?> GetNextBuyCost(int currentSlots)
        {
            var settings = await GetSettings();
            int nextSlot = currentSlots + 1;

            if (nextSlot <= 6)
            {
                return settings.SlotRebuyPrice;
            }

            return nextSlot switch
            {
                7 => settings.Slot7Price,
                8 => settings.Slot8Price,
                9 => settings.Slot9Price,
                10 => settings.Slot10Price,
                _ => null
            };
        }

        private decimal GetSellValue(int currentSlots)
        {
            var settings = _context.GameSettings.FirstOrDefault() ?? new GameSettings();

            return currentSlots switch
            {
                <= 6 => settings.SlotSellPrice,
                7 => settings.Slot7Price * 0.5m,
                8 => settings.Slot8Price * 0.5m,
                9 => settings.Slot9Price * 0.5m,
                10 => settings.Slot10Price * 0.5m,
                _ => 0
            };
        }
    }
}