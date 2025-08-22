using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly WarehouseDbContext _context;

        public HomeController(ILogger<HomeController> logger, WarehouseDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            // Ambil stok rendah
            var lowStockProducts = await _context.Products
    .Where(p => p.Quantity <= p.MinimumStock)
    .Include(p => p.SupplierProducts)
        .ThenInclude(sp => sp.Supplier)
    .ToListAsync();


            ViewBag.LowStockProducts = lowStockProducts;

            // Ambil riwayat barang masuk (QtyIn > 0) 10 terbaru
            var recentIn = await _context.Transactions
                .Where(t => t.QtyIn > 0)
                .Include(t => t.Product)
                .Include(t => t.Supplier)
                .OrderByDescending(t => t.Date)
                .Take(10)
                .Select(t => new Dashboard
                {
                    SupplierName = t.Supplier != null ? t.Supplier.Name : "-",
                    ProductName = t.Product != null ? t.Product.Name : "-",
                    QtyIn = t.QtyIn,
                    Date = t.Date
                })
                .ToListAsync();

            return View(recentIn); // dikirim ke @model IEnumerable<Dashboard>
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
