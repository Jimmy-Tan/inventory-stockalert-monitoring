using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;  // WAJIB!

namespace Project.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Contact { get; set; }

        [BindNever] // WAJIB! Supaya Binder tidak maksa validasi
        public ICollection<SupplierProduct> SupplierProducts { get; set; } = new List<SupplierProduct>();
        public ICollection<StockPerSupplier> StockPerSuppliers { get; set; } = new List<StockPerSupplier>();

    }
}


