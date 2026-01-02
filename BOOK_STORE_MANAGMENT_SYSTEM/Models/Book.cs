using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BOOK_STORE_MANAGMENT_SYSTEM.Models
{
    public class Book
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]//id will now auto increment
        public int Id {  get; set; }
        [Required]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Title can contain only letters and spaces.")]
        public string  Title { get; set; } = string.Empty; //validations only letters
        [Required]
        public string Category { get; set; } = String.Empty;
        [Required]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Title can contain only letters and spaces.")]

        public string Author {  get; set; } = String.Empty; // validations only valid name (remaining)
        [Required]
        [Range(1, 10000)]
        public decimal Price { get; set; } 
        [Required]
        [Range(0, 1000)]
        public int Quantity { get; set; } 
    }
}
