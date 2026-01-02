using System.ComponentModel.DataAnnotations;

namespace BOOK_STORE_MANAGMENT_SYSTEM.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        
        public string Role {  get; set; } //Admin or customer

     
    }
}
