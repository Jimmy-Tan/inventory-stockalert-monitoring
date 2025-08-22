using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Controllers
{
    public class StoresController : Controller
    {
        private readonly WarehouseDbContext _context;

        public StoresController(WarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index() => View(await _context.Stores.ToListAsync());

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Store store)
        {
            if (ModelState.IsValid)
            {
                _context.Add(store);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(store);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var store = await _context.Stores.FindAsync(id);
            if (store == null) return NotFound();

            return View(store);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Store store)
        {
            if (id != store.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(store);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(store);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == id);
            if (store == null) return NotFound();

            return View(store);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store != null)
            {
                _context.Stores.Remove(store);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var store = await _context.Stores
                .Include(s => s.Transactions) // Kalau mau lihat transaksi toko
                .FirstOrDefaultAsync(m => m.Id == id);

            if (store == null) return NotFound();

            return View(store);
        }
    }
}
