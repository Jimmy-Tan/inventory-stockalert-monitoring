    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using Project.Models;
using ClosedXML.Excel; 

namespace Project.Controllers
    {
        public class TransactionsController : Controller
        {
            private readonly WarehouseDbContext _context;

            public TransactionsController(WarehouseDbContext context)
            {
                _context = context;
            }

            // GET: Transactions
            public async Task<IActionResult> Index()
            {
                var transactions = _context.Transactions
                    .Include(t => t.Product)
                    .Include(t => t.Supplier);
                return View(await transactions.ToListAsync());
            }

            // GET: Transactions/Details/5
            public async Task<IActionResult> Details(int? id)
            {
                if (id == null) return NotFound();

                var transaction = await _context.Transactions
                    .Include(t => t.Product)
                    .Include(t => t.Supplier)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (transaction == null) return NotFound();

                return View(transaction);
            }

            // GET: Transactions/Create
            public IActionResult Create()
            {
                ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");
                ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name");
                return View();
            }
        // GET: Transactions/CreateBarangKeluar
        // GET: Transactions/CreateBarangKeluar
        public IActionResult CreateBarangKeluar(int? supplierId)
        {
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", supplierId);
            ViewBag.SelectedSupplierId = supplierId;

            if (supplierId.HasValue)
            {
                var productIds = _context.SupplierProducts
                    .Where(sp => sp.SupplierId == supplierId)
                    .Select(sp => sp.ProductId)
                    .ToList();

                var products = _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToList();

                ViewData["ProductId"] = new SelectList(products, "Id", "Name");
            }
            else
            {
                ViewData["ProductId"] = new SelectList(new List<Product>(), "Id", "Name");
            }

            return View();
        }


        // POST: Transactions/Create
        [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> CreateBarangKeluar([Bind("SupplierId,ProductId,QtyOut")] Transaction transaction)
            {
                if (transaction.QtyOut <= 0)
                {
                    ModelState.AddModelError("", "Jumlah keluar harus lebih dari 0.");
                }

                if (ModelState.IsValid)
                {
                    var product = await _context.Products.FindAsync(transaction.ProductId);
                    if (product == null)
                    {
                        ModelState.AddModelError("", "Produk tidak ditemukan.");
                        goto ViewError;
                    }

                    if (product.Quantity < transaction.QtyOut)
                    {
                        ModelState.AddModelError("", "Stok produk global tidak cukup.");
                        goto ViewError;
                    }

                    if (transaction.SupplierId == null || transaction.SupplierId == 0)
                    {
                        ModelState.AddModelError("", "Pilih supplier untuk barang keluar.");
                        goto ViewError;
                    }

                    var stockPerSupplier = await _context.StockPerSuppliers
                        .FirstOrDefaultAsync(s =>
                            s.SupplierId == transaction.SupplierId && s.ProductId == transaction.ProductId);

                    if (stockPerSupplier == null || stockPerSupplier.Quantity < transaction.QtyOut)
                    {
                        ModelState.AddModelError("", "Stok supplier tidak cukup atau belum tercatat.");
                        goto ViewError;
                    }

                    product.Quantity -= transaction.QtyOut;
                    stockPerSupplier.Quantity -= transaction.QtyOut;

                    transaction.QtyIn = 0;
                    transaction.Date = DateTime.Now;

                    _context.Add(transaction);
                    _context.Update(product);
                    _context.Update(stockPerSupplier);

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

            ViewError:
                ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", transaction.ProductId);
                ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", transaction.SupplierId);
                return View(transaction);
            }


            // ✅ INLINE TRANSACTION dari Supplier Details
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> CreateInline(int SupplierId, int ProductId, int QtyIn)
            {
                if (QtyIn <= 0) return BadRequest("Qty harus lebih dari 0.");

                var transaction = new Transaction
                {
                    SupplierId = SupplierId,
                    ProductId = ProductId,
                    QtyIn = QtyIn,
                    QtyOut = 0,
                    Date = DateTime.Now
                };

                var product = await _context.Products.FindAsync(ProductId);
                if (product == null) return BadRequest("Produk tidak ditemukan.");

                product.Quantity += QtyIn;

                // ✅ Update StockPerSupplier juga
                var stockPerSupplier = await _context.StockPerSuppliers
                    .FirstOrDefaultAsync(s => s.SupplierId == SupplierId && s.ProductId == ProductId);

                if (stockPerSupplier == null)
                {
                    stockPerSupplier = new StockPerSupplier
                    {
                        SupplierId = SupplierId,
                        ProductId = ProductId,
                        Quantity = QtyIn
                    };
                    _context.StockPerSuppliers.Add(stockPerSupplier);
                }
                else
                {
                    stockPerSupplier.Quantity += QtyIn;
                }

                _context.Add(transaction);
                _context.Update(product);

                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Suppliers", new { id = SupplierId });
            }

            // GET: Transactions/Edit/5
            public async Task<IActionResult> Edit(int? id)
            {
                if (id == null) return NotFound();

                var transaction = await _context.Transactions.FindAsync(id);
                if (transaction == null) return NotFound();

                ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", transaction.ProductId);
                ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", transaction.SupplierId);
                return View(transaction);
            }

            // POST: Transactions/Edit/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, [Bind("Id,SupplierId,ProductId,QtyIn,QtyOut,Date")] Transaction transaction)
            {
                if (id != transaction.Id) return NotFound();

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(transaction);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!TransactionExists(transaction.Id)) return NotFound();
                        else throw;
                    }
                    return RedirectToAction(nameof(Index));
                }

                ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", transaction.ProductId);
                ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", transaction.SupplierId);
                return View(transaction);
            }

            // GET: Transactions/Delete/5
            public async Task<IActionResult> Delete(int? id)
            {
                if (id == null) return NotFound();

                var transaction = await _context.Transactions
                    .Include(t => t.Product)
                    .Include(t => t.Supplier)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (transaction == null) return NotFound();

                return View(transaction);
            }

            // POST: Transactions/Delete/5
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var transaction = await _context.Transactions.FindAsync(id);
                if (transaction != null)
                {
                    _context.Transactions.Remove(transaction);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
        // GET: Transactions/DeliveryHistory
        public async Task<IActionResult> DeliveryHistory(DateTime? startDate, DateTime? endDate, int? productId, int? supplierId, int? storeId)
        {
            var query = _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Supplier)
                .Include(t => t.Store)
                .Where(t => t.QtyOut > 0);

            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(t => t.Date <= endDate.Value);
            if (productId.HasValue)
                query = query.Where(t => t.ProductId == productId.Value);
            if (supplierId.HasValue)
                query = query.Where(t => t.SupplierId == supplierId.Value);
            if (storeId.HasValue)
                query = query.Where(t => t.StoreId == storeId.Value);

            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
            ViewBag.Stores = await _context.Stores.ToListAsync();

            var result = await query.OrderByDescending(t => t.Date).ToListAsync();
            return View(result);
        }

        public async Task<IActionResult> ExportDeliveryHistory(DateTime? startDate, DateTime? endDate, int? productId, int? supplierId, int? storeId)
        {
            var query = _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Supplier)
                .Include(t => t.Store)
                .Where(t => t.QtyOut > 0);

            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(t => t.Date <= endDate.Value);
            if (productId.HasValue)
                query = query.Where(t => t.ProductId == productId.Value);
            if (supplierId.HasValue)
                query = query.Where(t => t.SupplierId == supplierId.Value);
            if (storeId.HasValue)
                query = query.Where(t => t.StoreId == storeId.Value);

            var data = await query.OrderByDescending(t => t.Date).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Riwayat Pengiriman");

            worksheet.Cell(1, 1).Value = "Tanggal";
            worksheet.Cell(1, 2).Value = "Produk";
            worksheet.Cell(1, 3).Value = "Supplier";
            worksheet.Cell(1, 4).Value = "Toko";
            worksheet.Cell(1, 5).Value = "Qty Keluar";

            var headerRange = worksheet.Range(1, 1, 1, 5);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            int row = 2;
            foreach (var t in data)
            {
                worksheet.Cell(row, 1).Value = t.Date.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 2).Value = t.Product?.Name ?? "-";
                worksheet.Cell(row, 3).Value = t.Supplier?.Name ?? "-";
                worksheet.Cell(row, 4).Value = t.Store?.Name ?? "-";
                worksheet.Cell(row, 5).Value = t.QtyOut;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Riwayat_Pengiriman_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(stream, contentType, fileName);
        }


        [HttpGet]
        public async Task<IActionResult> ExportDeliveryHistory(DateTime? startDate, DateTime? endDate, int? productId, int? supplierId)
        {
            var query = _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Supplier)
                .Where(t => t.QtyOut > 0);

            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(t => t.Date <= endDate.Value);
            if (productId.HasValue)
                query = query.Where(t => t.ProductId == productId.Value);
            if (supplierId.HasValue)
                query = query.Where(t => t.SupplierId == supplierId.Value);

            var data = await query.OrderByDescending(t => t.Date).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Riwayat Pengiriman");

            worksheet.Cell(1, 1).Value = "Tanggal";
            worksheet.Cell(1, 2).Value = "Produk";
            worksheet.Cell(1, 3).Value = "Supplier";
            worksheet.Cell(1, 4).Value = "Qty Keluar";

            var headerRange = worksheet.Range(1, 1, 1, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            int row = 2;
            foreach (var t in data)
            {
                worksheet.Cell(row, 1).Value = t.Date.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 2).Value = t.Product?.Name ?? "-";
                worksheet.Cell(row, 3).Value = t.Supplier?.Name ?? "-";
                worksheet.Cell(row, 4).Value = t.QtyOut;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Riwayat_Pengiriman_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(stream, contentType, fileName);
        }
        // ✅ GET FORM: Barang keluar ke Store
        public IActionResult CreateBarangKeluarToStore()
        {
            ViewData["StoreId"] = new SelectList(_context.Stores, "Id", "Name");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name");
            // Awal kosong dulu
            ViewData["ProductId"] = new SelectList(new List<Product>(), "Id", "Name");
            return View();
        }

        // ✅ POST Barang keluar ke Store
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBarangKeluarToStore([Bind("StoreId,SupplierId,ProductId,QtyOut")] Transaction transaction)
        {
            if (transaction.QtyOut <= 0)
            {
                ModelState.AddModelError("", "Jumlah keluar harus lebih dari 0.");
            }

            if (ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(transaction.ProductId);
                if (product == null)
                {
                    ModelState.AddModelError("", "Produk tidak ditemukan.");
                    goto ViewError;
                }

                if (transaction.StoreId == null || transaction.StoreId == 0)
                {
                    ModelState.AddModelError("", "Pilih toko tujuan.");
                    goto ViewError;
                }

                if (transaction.SupplierId == null || transaction.SupplierId == 0)
                {
                    ModelState.AddModelError("", "Pilih supplier asal.");
                    goto ViewError;
                }

                var stockPerSupplier = await _context.StockPerSuppliers
                    .FirstOrDefaultAsync(s =>
                        s.SupplierId == transaction.SupplierId && s.ProductId == transaction.ProductId);

                if (stockPerSupplier == null || stockPerSupplier.Quantity < transaction.QtyOut)
                {
                    ModelState.AddModelError("", "Stok supplier tidak cukup.");
                    goto ViewError;
                }

                // Kurangi stok supplier & global
                stockPerSupplier.Quantity -= transaction.QtyOut;
                product.Quantity -= transaction.QtyOut;

                transaction.QtyIn = 0;
                transaction.Date = DateTime.Now;

                _context.Add(transaction);
                _context.Update(stockPerSupplier);
                _context.Update(product);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

        ViewError:
            ViewData["StoreId"] = new SelectList(_context.Stores, "Id", "Name", transaction.StoreId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", transaction.SupplierId);
            ViewData["ProductId"] = new SelectList(new List<Product>(), "Id", "Name"); // Kosongkan
            return View(transaction);
        }

        // ✅ AJAX ENDPOINT: Get Products by Supplier
        [HttpGet]
        public JsonResult GetProductsBySupplier(int supplierId)
        {
            var productIds = _context.SupplierProducts
                .Where(sp => sp.SupplierId == supplierId)
                .Select(sp => sp.ProductId)
                .ToList();

            var products = _context.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Name })
                .ToList();

            return Json(products);
        }

        private bool TransactionExists(int id)
            {
                return _context.Transactions.Any(e => e.Id == id);
            }
        }
    }
