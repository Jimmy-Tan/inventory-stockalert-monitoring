using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Project.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public int? SupplierId { get; set; }   // ✅ WAJIB nullable!

        [BindNever]
        public Supplier? Supplier { get; set; }

        public int ProductId { get; set; }

        [BindNever]
        public Product? Product { get; set; }

        public int QtyIn { get; set; }
        public int QtyOut { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public int? StoreId { get; set; }   // ✅ Tambah ini
        public Store? Store { get; set; }

    }
}
