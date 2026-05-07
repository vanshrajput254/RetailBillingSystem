using Microsoft.EntityFrameworkCore;
using RetailBillingSystem.Models;
using System.Security.Cryptography;
using System.Text;

namespace RetailBillingSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SKU).IsUnique();

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceNumber).IsUnique();

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Customer)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Invoice)
                .WithMany(i => i.Items)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Product)
                .WithMany(p => p.InvoiceItems)
                .HasForeignKey(ii => ii.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, SKU = "SKU001", ProductName = "Basmati Rice 5kg", Category = "Groceries", Rate = 350, GSTRate = 5, StockQuantity = 150 },
                new Product { ProductId = 2, SKU = "SKU002", ProductName = "Tata Salt 1kg", Category = "Groceries", Rate = 22, GSTRate = 5, StockQuantity = 8 },
                new Product { ProductId = 3, SKU = "SKU003", ProductName = "Surf Excel 1kg", Category = "Household", Rate = 185, GSTRate = 18, StockQuantity = 60 },
                new Product { ProductId = 4, SKU = "SKU004", ProductName = "Colgate Toothpaste", Category = "Personal Care", Rate = 95, GSTRate = 12, StockQuantity = 45 },
                new Product { ProductId = 5, SKU = "SKU005", ProductName = "Ariel Detergent 2kg", Category = "Household", Rate = 310, GSTRate = 18, StockQuantity = 5 }
            );

            // Seed Customers
            modelBuilder.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, Name = "Ravi Kumar", Phone = "9876543210", Email = "ravi@example.com", City = "Bengaluru", State = "Karnataka", GSTIN = "29ASHSS1234R1Z1" },
                new Customer { CustomerId = 2, Name = "Meena Traders", Phone = "9123456789", Email = "meena@traders.com", City = "Mysuru", State = "Karnataka", GSTIN = "29BBBCA1557C1Z1" },
                new Customer { CustomerId = 3, Name = "Sunita Retail", Phone = "9000000001", City = "Hubli", State = "Karnataka" }
            );

            // Seed default admin user — password: Admin@123
            modelBuilder.Entity<AppUser>().HasData(
                new AppUser
                {
                    UserId = 1,
                    Username = "admin",
                    PasswordHash = HashPassword("Admin@123"),
                    FullName = "Store Admin",
                    Role = "Admin",
                    IsActive = true
                }
            );
        }

        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(
                Encoding.UTF8.GetBytes(password + "RetailBill_Salt_2024")
            );
            return Convert.ToBase64String(bytes);
        }
    }
}
