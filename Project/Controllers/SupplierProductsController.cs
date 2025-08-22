using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Controllers
{
    public class SupplierProductsController : Controller
    {
        private readonly WarehouseDbContext _context;

        public SupplierProductsController(WarehouseDbContext context)
        {
            _context = context;
        }

        // GET: SupplierProducts/Create
        public IActionResult Create(int id)
        {
            var existingProductIds = _context.SupplierProducts
                .Where(sp => sp.SupplierId == id)
                .Select(sp => sp.ProductId)
                .ToList();

            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => !existingProductIds.Contains(p.Id)),
                "Id", "Name"
            );

            return View(new SupplierProduct { SupplierId = id });
        }

        // POST: SupplierProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SupplierId,ProductId")] SupplierProduct supplierProduct)
        {
            if (ModelState.IsValid)
            {
                var exists = _context.SupplierProducts
                    .Any(sp => sp.SupplierId == supplierProduct.SupplierId && sp.ProductId == supplierProduct.ProductId);

                if (!exists)
                {
                    _context.Add(supplierProduct);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("Details", "Suppliers", new { id = supplierProduct.SupplierId });
            }

            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", supplierProduct.ProductId);
            return View(supplierProduct);
        }

        // GET: SupplierProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var supplierProduct = await _context.SupplierProducts
                .Include(sp => sp.Supplier)
                .Include(sp => sp.Product)
                .FirstOrDefaultAsync(sp => sp.Id == id);

            if (supplierProduct == null) return NotFound();

            return View(supplierProduct);
        }


        // POST: SupplierProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplierProduct = await _context.SupplierProducts.FindAsync(id);
            if (supplierProduct != null)
            {
                _context.SupplierProducts.Remove(supplierProduct);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Suppliers", new { id = supplierProduct.SupplierId });
        }
    }
}
