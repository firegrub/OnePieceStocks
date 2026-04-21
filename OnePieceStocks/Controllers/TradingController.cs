using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnePieceStocks.Data;
using OnePieceStocks.Models;

namespace OnePieceStocks.Controllers
{
    [Authorize]
    public class TradingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TradingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new TradeRequest();

            var tradingBlocked = await CheckTradingBlocked();
            if (tradingBlocked != null)
            {
                ViewBag.TradingClosedMessage = tradingBlocked;
                return View(model);
            }

            if (!User.IsInRole("Admin"))
            {
                var currentUsername = User.Identity?.Name;
                var player = await _context.Players.FirstOrDefaultAsync(p => p.UserId == currentUsername);

                if (player == null)
                {
                    ViewBag.NoLinkedPlayer = true;
                    return View(model);
                }

                model.PlayerId = player.Id;
            }

            await LoadDropdowns(model.PlayerId, null);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TradeRequest request)
        {
            var tradingBlocked = await CheckTradingBlocked();
            if (tradingBlocked != null)
            {
                ViewBag.TradingClosedMessage = tradingBlocked;
                return View(request);
            }

            if (!User.IsInRole("Admin"))
            {
                var currentUsername = User.Identity?.Name;
                var linkedPlayer = await _context.Players.FirstOrDefaultAsync(p => p.UserId == currentUsername);

                if (linkedPlayer == null)
                {
                    ViewBag.NoLinkedPlayer = true;
                    await LoadDropdowns(null, request.CharacterId);
                    ModelState.AddModelError("", "No player account is linked to this username.");
                    return View(request);
                }

                request.PlayerId = linkedPlayer.Id;
            }

            await LoadDropdowns(request.PlayerId, request.CharacterId);

            var player = await _context.Players
                .Include(p => p.Holdings)
                .FirstOrDefaultAsync(p => p.Id == request.PlayerId);

            var character = await _context.Characters
                .FirstOrDefaultAsync(c => c.Id == request.CharacterId);

            if (player == null)
            {
                ModelState.AddModelError("", "Player not found.");
                return View(request);
            }

            if (character == null)
            {
                ModelState.AddModelError("", "Character not found.");
                return View(request);
            }

            if (request.ActionType == "Buy")
            {
                var totalCost = character.CurrentPrice * request.Quantity;

                var existingHolding = await _context.Holdings
                    .FirstOrDefaultAsync(h => h.PlayerId == request.PlayerId && h.CharacterId == request.CharacterId);

                var usedSlots = await _context.Holdings
                    .Where(h => h.PlayerId == request.PlayerId && h.Quantity > 0)
                    .CountAsync();

                var needsNewSlot = existingHolding == null;

                var totalOwnedForCharacter = await _context.Holdings
                    .Where(h => h.CharacterId == request.CharacterId)
                    .SumAsync(h => h.Quantity);

                var playerOwnedForCharacter = existingHolding?.Quantity ?? 0;

                if (totalOwnedForCharacter + request.Quantity > character.MaxStockUnits)
                {
                    ModelState.AddModelError("", $"Only {character.MaxStockUnits - totalOwnedForCharacter} shares remain available for this character.");
                    return View(request);
                }

                if (playerOwnedForCharacter + request.Quantity > 1700)
                {
                    ModelState.AddModelError("", "A single player cannot own more than 1700 units of one stock.");
                    return View(request);
                }


                if (totalOwnedForCharacter + request.Quantity > character.MaxStockUnits)
                {
                    ModelState.AddModelError("", $"Only {character.MaxStockUnits - totalOwnedForCharacter} shares remain available for this character.");
                    return View(request);
                }

                if (playerOwnedForCharacter + request.Quantity > 1700)
                {
                    ModelState.AddModelError("", "A single player cannot own more than 1700 units of one stock.");
                    return View(request);
                }

                if (player.Wallet < totalCost)
                {
                    ModelState.AddModelError("", "Not enough money.");
                    return View(request);
                }

                if (needsNewSlot && usedSlots >= player.TotalSlots)
                {
                    ModelState.AddModelError("", "No slot available.");
                    return View(request);
                }

                player.Wallet -= totalCost;

                if (existingHolding == null)
                {
                    existingHolding = new Holding
                    {
                        PlayerId = player.Id,
                        CharacterId = character.Id,
                        Quantity = request.Quantity,
                        AverageBuyPrice = character.CurrentPrice
                    };

                    _context.Holdings.Add(existingHolding);
                }
                else
                {
                    var oldTotalShares = existingHolding.Quantity;
                    var oldTotalCost = existingHolding.AverageBuyPrice * oldTotalShares;
                    var newCost = character.CurrentPrice * request.Quantity;
                    var newTotalShares = oldTotalShares + request.Quantity;

                    existingHolding.Quantity = newTotalShares;
                    existingHolding.AverageBuyPrice = (oldTotalCost + newCost) / newTotalShares;
                }

                _context.TradeHistories.Add(new TradeHistory
                {
                    PlayerId = player.Id,
                    CharacterId = character.Id,
                    TradeType = "Buy",
                    Quantity = request.Quantity,
                    PricePerShare = character.CurrentPrice,
                    TotalAmount = totalCost,
                    Date = DateTime.UtcNow
                });

                _context.ActivityLogs.Add(new ActivityLog
                {
                    ActivityType = "Trade",
                    Message = $"{player.Name} bought {request.Quantity} of {character.Name} for {totalCost:N0}."
                });

                await _context.SaveChangesAsync();

                ViewBag.SuccessMessage = "Purchase completed.";
                ModelState.Clear();

                var resetModel = new TradeRequest();
                if (!User.IsInRole("Admin"))
                {
                    resetModel.PlayerId = player.Id;
                }

                await LoadDropdowns(resetModel.PlayerId, null);
                return View(resetModel);
            }

            if (request.ActionType == "Sell")
            {
                var existingHolding = await _context.Holdings
                    .AsTracking()
                    .FirstOrDefaultAsync(h => h.PlayerId == request.PlayerId && h.CharacterId == request.CharacterId);

                if (existingHolding == null)
                {
                    ModelState.AddModelError("", "You do not own this character.");
                    return View(request);
                }

                if (request.Quantity <= 0)
                {
                    ModelState.AddModelError("", "Quantity must be at least 1.");
                    return View(request);
                }

                if (existingHolding.Quantity <= 0)
                {
                    ModelState.AddModelError("", "You do not own any shares of this character.");
                    return View(request);
                }

                if (request.Quantity > existingHolding.Quantity)
                {
                    ModelState.AddModelError("", $"You only own {existingHolding.Quantity} shares.");
                    return View(request);
                }

                var totalValue = character.CurrentPrice * request.Quantity;

                existingHolding.Quantity -= request.Quantity;
                player.Wallet += totalValue;

                if (existingHolding.Quantity == 0)
                {
                    _context.Holdings.Remove(existingHolding);
                }

                _context.TradeHistories.Add(new TradeHistory
                {
                    PlayerId = player.Id,
                    CharacterId = character.Id,
                    TradeType = "Sell",
                    Quantity = request.Quantity,
                    PricePerShare = character.CurrentPrice,
                    TotalAmount = totalValue,
                    Date = DateTime.UtcNow
                });

                _context.ActivityLogs.Add(new ActivityLog
                {
                    ActivityType = "Trade",
                    Message = $"{player.Name} sold {request.Quantity} of {character.Name} for {totalValue:N0}."
                });

                await _context.SaveChangesAsync();

                ViewBag.SuccessMessage = "Sale completed.";
                ModelState.Clear();

                var resetModel = new TradeRequest();
                if (!User.IsInRole("Admin"))
                {
                    resetModel.PlayerId = player.Id;
                }

                await LoadDropdowns(resetModel.PlayerId, null);
                return View(resetModel);
            }

            ModelState.AddModelError("", "Invalid trade type.");
            return View(request);
        }

        private async Task<string?> CheckTradingBlocked()
        {
            var settings = await _context.GameSettings.FirstOrDefaultAsync();

            if (settings == null || !settings.IsTradingOpen)
            {
                return "Trading is currently locked.";
            }

            if (settings.TradingClosesAtUtc.HasValue && DateTime.UtcNow > settings.TradingClosesAtUtc.Value)
            {
                settings.IsTradingOpen = false;
                settings.TradingClosesAtUtc = null;
                await _context.SaveChangesAsync();
                return "Trading is currently locked.";
            }

            return null;
        }

        private async Task LoadDropdowns(int? selectedPlayerId = null, int? selectedCharacterId = null)
        {
            var characters = await _context.Characters
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.CharacterId = new SelectList(characters, "Id", "Name", selectedCharacterId);
            ViewBag.CharacterPrices = characters.ToDictionary(c => c.Id, c => c.CurrentPrice);

            if (User.IsInRole("Admin"))
            {
                var players = await _context.Players
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                ViewBag.PlayerId = new SelectList(players, "Id", "Name", selectedPlayerId);
                ViewBag.PlayerWallets = players.ToDictionary(p => p.Id, p => p.Wallet);
            }
            else
            {
                var currentUsername = User.Identity?.Name;
                var player = await _context.Players.FirstOrDefaultAsync(p => p.UserId == currentUsername);
                ViewBag.PlayerWallets = player == null
                    ? new Dictionary<int, decimal>()
                    : new Dictionary<int, decimal> { { player.Id, player.Wallet } };
            }
        }
    }
}