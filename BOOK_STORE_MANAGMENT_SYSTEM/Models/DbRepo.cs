using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BOOK_STORE_MANAGMENT_SYSTEM.Models
{
    public class DbRepo : DbContext
    {
        public DbRepo(DbContextOptions<DbRepo> options) : base(options) { }


        public DbSet<Cart> Carts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BillItem> BillItems { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<User> Users { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) // AVOID Pending model changes warning
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var admin = new User
            {
                Id = 1,
                UserName = "Admin",
                Role = "Admin",
            };
            var hasher = new PasswordHasher<User>();
            admin.Password = hasher.HashPassword(admin, "Admin123");

            modelBuilder.Entity<User>().HasData(admin);//tells EF that i wnat to work with user table and add info /create admin automiucatlly when run migration
            modelBuilder.Entity<BillItem>()

                        .HasOne(bi => bi.Bill)
                        .WithMany(b => b.Items)
                        .HasForeignKey(bi => bi.BillId) // one bill has  many bill items deleting bill wil delete billitems
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BillItem>().Property(b => b.BookPrice).HasPrecision(18, 2); //max 18 digits and 2 num after point
            modelBuilder.Entity<Book>().Property(b => b.Price).HasPrecision(18, 2);
        }
    }
}
