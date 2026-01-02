using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BOOK_STORE_MANAGMENT_SYSTEM.Models
{
    public class BillItem
    {
        [Key]
        public int Id { get; set; }
        //Foreign key
        public int BillId { get; set; }
        public Bill Bill { get; set; } = null!; //for compiler to ignore null
  
        [Required]
        public string BookName { get; set; } = String.Empty;

        [Range(1, 10000)]
        public decimal BookPrice { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; }

        public decimal Total => BookPrice * Quantity; //billitem for one item's total , bill => overall items purchased

    }
}
