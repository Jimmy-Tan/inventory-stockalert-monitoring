using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Project.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }

        [BindNever]
        public Category? Category { get; set; } // ✅ Sekarang navigasi nullable

        public int Quantity { get; set; }
        public int MinimumStock { get; set; }

        [BindNever]
        public ICollection<SupplierProduct> SupplierProducts { get; set; } = new List<SupplierProduct>();
        public ICollection<StockPerSupplier> StockPerSuppliers { get; set; } = new List<StockPerSupplier>();

    }
}
