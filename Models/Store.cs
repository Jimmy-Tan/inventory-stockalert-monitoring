using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Store
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nama Toko wajib diisi")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Alamat Toko wajib diisi")]
        public string Address { get; set; } = "";

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
