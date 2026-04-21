using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnePieceStocks.Data;
using OnePieceStocks.Models;

namespace OnePieceStocks.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PriceUpdatesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PriceUpdatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            await LoadCharacters();
            return View(new PriceUpdateRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(PriceUpdateRequest request)
        {
            await LoadCharacters(request.CharacterId);

            var character = await _context.Characters.FirstOrDefaultAsync(c => c.Id == request.CharacterId);

            if (character == null)
            {
                ModelState.AddModelError("", "Character not found.");
                return View(request);
            }

            decimal oldPrice = character.CurrentPrice;
            decimal newPrice = oldPrice;

            if (request.IsDeathEvent)
            {
                newPrice = 0;
                character.IsDead = true;
            }
            else
            {
                decimal change = request.CustomValue ?? 0;

                // Convert percent if needed
                if (request.ChangeMode == "Percent")
                {
                    change = oldPrice * (change / 100m);
                }

                // 🚨 $1 PENALTY SYSTEM
                if (request.ChangeDirection == "Decrease" && oldPrice <= 1)
                {
                    var holdings = await _context.Holdings
                        .Include(h => h.Player)
                        .Where(h => h.CharacterId == character.Id && h.Quantity > 0)
                        .ToListAsync();

                    foreach (var holding in holdings)
                    {
                        var tax = (change * 0.5m) * holding.Quantity;

                        holding.Player.Wallet -= tax;

                        _context.ActivityLogs.Add(new ActivityLog
                        {
                            ActivityType = "Penalty",
                            Message = $"{holding.Player.Name} was taxed {tax:N0} because {character.Name} is at $1 and took another loss."
                        });
                    }

                    newPrice = 1;
                }
                else
                {
                    newPrice = request.ChangeDirection == "Increase"
                        ? oldPrice + change
                        : oldPrice - change;

                    if (newPrice < 1)
                    {
                        newPrice = 1;
                    }
                }
            }

            character.CurrentPrice = newPrice;

            _context.PriceHistories.Add(new PriceHistory
            {
                CharacterId = character.Id,
                OldPrice = oldPrice,
                NewPrice = newPrice,
                ChangeDirection = request.ChangeDirection,
                ChangeMode = request.ChangeMode,
                ChangeValue = request.CustomValue ?? 0,
                Date = DateTime.UtcNow,
                Notes = request.Notes,
                IsDeathEvent = request.IsDeathEvent
            });

            _context.ActivityLogs.Add(new ActivityLog
            {
                ActivityType = "Price Update",
                Message = request.IsDeathEvent
                    ? $"{character.Name} died. Price set to 0."
                    : $"{character.Name} changed from {oldPrice:N0} to {newPrice:N0}"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Price updated.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCharacters(int? selectedCharacterId = null)
        {
            var characters = await _context.Characters
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.CharacterId = new SelectList(characters, "Id", "Name", selectedCharacterId);
        }
    }
}