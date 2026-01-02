using System.ComponentModel.DataAnnotations;

namespace BOOK_STORE_MANAGMENT_SYSTEM.Models
{ 
    public class Bill
    {
        public int Id { get; set; }

        public DateTime BillingDate { get; set; }
        [Required]
        public String CustomerName { get; set; } = String.Empty;

        //one bill can have many bill items:
        public List<BillItem> Items { get; set; } = new List<BillItem>();
        public decimal GrandTotal => Items.Sum(i => i.BookPrice * i.Quantity);

    }
}
