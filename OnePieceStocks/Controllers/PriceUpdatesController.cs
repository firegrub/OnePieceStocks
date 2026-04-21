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

            if (!request.IsDeathEvent && (!request.CustomValue.HasValue || request.CustomValue.Value <= 0))
            {
                ModelState.AddModelError("", "Enter a custom flat value greater than 0.");
                return View(request);
            }

            decimal oldPrice = character.CurrentPrice;
            decimal newPrice = oldPrice;
            decimal actualChangeAmount = 0;

            if (request.IsDeathEvent)
            {
                newPrice = 0;
                actualChangeAmount = oldPrice;
                character.IsDead = true;
            }
            else
            {
                decimal change = request.CustomValue ?? 0;

                if (request.ChangeDirection == "Decrease" && oldPrice <= 1)
                {
                    var holdingsAtFloor = await _context.Holdings
                        .Include(h => h.Player)
                        .Where(h => h.CharacterId == character.Id && h.Quantity > 0)
                        .ToListAsync();

                    foreach (var holding in holdingsAtFloor)
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
                    actualChangeAmount = 0;
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

                    actualChangeAmount = Math.Abs(newPrice - oldPrice);
                }
            }

            character.CurrentPrice = newPrice;

            if (!request.IsDeathEvent && actualChangeAmount > 0)
            {
                if (request.ChangeDirection == "Increase")
                {
                    character.LifetimeIncreaseAmount += actualChangeAmount;
                    character.WeeklyIncreaseAmount += actualChangeAmount;
                    character.LifetimeIncreaseCount += 1;
                    character.WeeklyIncreaseCount += 1;
                }
                else
                {
                    character.LifetimeDecreaseAmount += actualChangeAmount;
                    character.WeeklyDecreaseAmount += actualChangeAmount;
                    character.LifetimeDecreaseCount += 1;
                    character.WeeklyDecreaseCount += 1;
                }
            }

            if (request.IsDeathEvent)
            {
                character.LifetimeDecreaseAmount += actualChangeAmount;
                character.WeeklyDecreaseAmount += actualChangeAmount;
                character.LifetimeDecreaseCount += 1;
                character.WeeklyDecreaseCount += 1;
            }

            var holdings = await _context.Holdings
                .Include(h => h.Player)
                .Where(h => h.CharacterId == character.Id && h.Quantity > 0)
                .ToListAsync();

            // WALLET ONLY CHANGES ON WINS OR DEATH
            if (actualChangeAmount > 0)
            {
                foreach (var holding in holdings)
                {
                    var walletDelta = actualChangeAmount * holding.Quantity;

                    if (request.IsDeathEvent)
                    {
                        holding.Player.Wallet -= walletDelta;

                        _context.ActivityLogs.Add(new ActivityLog
                        {
                            ActivityType = "Wallet Adjustment",
                            Message = $"{holding.Player.Name} lost {walletDelta:N0} in wallet value from {character.Name} dying."
                        });
                    }
                    else if (request.ChangeDirection == "Increase")
                    {
                        holding.Player.Wallet += walletDelta;

                        _context.ActivityLogs.Add(new ActivityLog
                        {
                            ActivityType = "Wallet Adjustment",
                            Message = $"{holding.Player.Name} gained {walletDelta:N0} in wallet value from {character.Name}."
                        });
                    }
                }
            }

            _context.PriceHistories.Add(new PriceHistory
            {
                CharacterId = character.Id,
                OldPrice = oldPrice,
                NewPrice = newPrice,
                ChangeDirection = request.IsDeathEvent ? "Decrease" : request.ChangeDirection,
                ChangeMode = "Flat",
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