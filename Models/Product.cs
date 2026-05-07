using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetailBillingSystem.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required, MaxLength(20)]
        public string SKU { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Rate { get; set; }

        // GST Rate: 0, 5, 12, 18, 28
        public decimal GSTRate { get; set; }

        public int StockQuantity { get; set; }

        public int LowStockThreshold { get; set; } = 10;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}
