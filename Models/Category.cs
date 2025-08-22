using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding; // ⬅ WAJIB!

namespace Project.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [BindNever] // 🚫 Biar Binder TIDAK ikutan binding navigasi balik ke Product
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
