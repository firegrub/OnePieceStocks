using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnePieceStocks.Data;
using OnePieceStocks.Models;

namespace OnePieceStocks.Controllers
{
    public class RulesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rules = await _context.RulePages.FirstOrDefaultAsync();

            if (rules == null)
            {
                rules = new RulePage
                {
                    Content = "No rules have been added yet."
                };
                _context.RulePages.Add(rules);
                await _context.SaveChangesAsync();
            }

            return View(rules);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit()
        {
            var rules = await _context.RulePages.FirstOrDefaultAsync();

            if (rules == null)
            {
                rules = new RulePage
                {
                    Content = ""
                };
                _context.RulePages.Add(rules);
                await _context.SaveChangesAsync();
            }

            return View(rules);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(RulePage model)
        {
            var rules = await _context.RulePages.FirstOrDefaultAsync();

            if (rules == null)
            {
                rules = new RulePage();
                _context.RulePages.Add(rules);
            }

            rules.Content = model.Content;
            rules.LastUpdated = DateTime.UtcNow;

            _context.ActivityLogs.Add(new ActivityLog
            {
                ActivityType = "Rules Update",
                Message = "Rules page was updated."
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Rules updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}