using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnePieceStocks.Data;
using OnePieceStocks.Models;

namespace OnePieceStocks.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult CreatePlayerAccount()
        {
            return View(new CreatePlayerAccountModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePlayerAccount(CreatePlayerAccountModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Username already exists.");
                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = model.Username
            };

            var createResult = await _userManager.CreateAsync(user, model.TempPassword);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Player");

            var player = new Player
            {
                Name = model.Name,
                UserId = model.Username,
                Wallet = model.Wallet,
                TotalSlots = model.TotalSlots,
                Notes = model.Notes,
                ProfileImagePath = model.ProfileImagePath,
                IsActive = true,
                MustChangePassword = true
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            _context.ActivityLogs.Add(new ActivityLog
            {
                ActivityType = "Account Created",
                Message = $"{model.Name} account created with username {model.Username}."
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Player account created successfully.";
            return RedirectToAction(nameof(CreatePlayerAccount));
        }

        public async Task<IActionResult> ManageTrading()
        {
            var settings = await _context.GameSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new GameSettings();
                _context.GameSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            ViewBag.IsTradingOpen = settings.IsTradingOpen;
            ViewBag.TradingClosesAtUtc = settings.TradingClosesAtUtc;

            return View(new TradingWindowRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageTrading(TradingWindowRequest model)
        {
            if (model.Days == 0 && model.Hours == 0)
            {
                ModelState.AddModelError("", "Enter at least days or hours.");
                return View(model);
            }

            var settings = await _context.GameSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new GameSettings();
                _context.GameSettings.Add(settings);
            }

            settings.IsTradingOpen = true;
            settings.TradingClosesAtUtc = DateTime.UtcNow.AddDays(model.Days).AddHours(model.Hours);

            _context.ActivityLogs.Add(new ActivityLog
            {
                ActivityType = "Trading Window",
                Message = $"Trading opened until {settings.TradingClosesAtUtc:yyyy-MM-dd HH:mm} UTC."
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Trading window opened.";
            return RedirectToAction(nameof(ManageTrading));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StopTrading()
        {
            var settings = await _context.GameSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new GameSettings();
                _context.GameSettings.Add(settings);
            }

            settings.IsTradingOpen = false;
            settings.TradingClosesAtUtc = null;

            _context.ActivityLogs.Add(new ActivityLog
            {
                ActivityType = "Trading Window",
                Message = "Trading was manually locked by admin."
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Trading stopped.";
            return RedirectToAction(nameof(ManageTrading));
        }

        public IActionResult AccountTools()
        {
            return View(new AdminAccountToolsModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountTools(AdminAccountToolsModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            var player = await _context.Players.FirstOrDefaultAsync(p => p.UserId == model.Username);

            if (!string.IsNullOrWhiteSpace(model.TempPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.TempPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                if (player != null)
                {
                    player.MustChangePassword = true;
                }

                _context.ActivityLogs.Add(new ActivityLog
                {
                    ActivityType = "Admin Account Action",
                    Message = $"Password reset for {model.Username}."
                });
            }

            if (model.ForcePasswordChange && player != null)
            {
                player.MustChangePassword = true;

                _context.ActivityLogs.Add(new ActivityLog
                {
                    ActivityType = "Admin Account Action",
                    Message = $"{model.Username} was forced to change password."
                });
            }

            if (model.PromoteToAdmin)
            {
                if (!await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    await _userManager.AddToRoleAsync(user, "Admin");

                    _context.ActivityLogs.Add(new ActivityLog
                    {
                        ActivityType = "Admin Account Action",
                        Message = $"{model.Username} was promoted to Admin."
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Account tools applied.";
            return RedirectToAction(nameof(AccountTools));
        }
    }
}