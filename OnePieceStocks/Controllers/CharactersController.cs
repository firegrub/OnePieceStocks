using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnePieceStocks.Data;
using OnePieceStocks.Models;

namespace OnePieceStocks.Controllers
{
    public class CharactersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CharactersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Characters.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var character = await _context.Characters
                .Include(c => c.PriceHistories)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (character == null)
            {
                return NotFound();
            }

            ViewBag.PriceHistoryLabels = character.PriceHistories
                .OrderBy(p => p.Date)
                .Select(p => p.Date.ToLocalTime().ToString("g"))
                .ToList();

            ViewBag.PriceHistoryValues = character.PriceHistories
                .OrderBy(p => p.Date)
                .Select(p => p.NewPrice)
                .ToList();

            return View(character);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Character character)
        {
            if (ModelState.IsValid)
            {
                _context.Add(character);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(character);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var character = await _context.Characters.FindAsync(id);
            if (character == null)
            {
                return NotFound();
            }

            return View(character);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Character character)
        {
            if (id != character.Id)
            {
                return NotFound();
            }

            var existingCharacter = await _context.Characters.FindAsync(id);
            if (existingCharacter == null)
            {
                return NotFound();
            }

            existingCharacter.Name = character.Name;
            existingCharacter.ImagePath = character.ImagePath;
            existingCharacter.CurrentPrice = character.CurrentPrice;
            existingCharacter.Bounty = character.Bounty;
            existingCharacter.LifetimeIncreaseAmount = character.LifetimeIncreaseAmount;
            existingCharacter.LifetimeDecreaseAmount = character.LifetimeDecreaseAmount;
            existingCharacter.WeeklyIncreaseAmount = character.WeeklyIncreaseAmount;
            existingCharacter.WeeklyDecreaseAmount = character.WeeklyDecreaseAmount;
            existingCharacter.LifetimeIncreaseCount = character.LifetimeIncreaseCount;
            existingCharacter.LifetimeDecreaseCount = character.LifetimeDecreaseCount;
            existingCharacter.WeeklyIncreaseCount = character.WeeklyIncreaseCount;
            existingCharacter.WeeklyDecreaseCount = character.WeeklyDecreaseCount;
            existingCharacter.Notes = character.Notes;
            existingCharacter.IsDead = character.IsDead;
            existingCharacter.IsActive = character.IsActive;

            if (ModelState.IsValid)
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(character);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var character = await _context.Characters
                .FirstOrDefaultAsync(m => m.Id == id);

            if (character == null)
            {
                return NotFound();
            }

            return View(character);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var character = await _context.Characters.FindAsync(id);
            if (character != null)
            {
                _context.Characters.Remove(character);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}