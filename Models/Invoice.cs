using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetailBillingSystem.Models
{
    public enum InvoiceStatus { Pending, Paid, Overdue, Cancelled }
    public enum GSTType { CGST_SGST, IGST }

    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [Required, MaxLength(20)]
        public string InvoiceNumber { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public DateTime InvoiceDate { get; set; } = DateTime.Today;
        public DateTime? DueDate { get; set; }

        public GSTType GSTType { get; set; } = GSTType.CGST_SGST;

        [MaxLength(15)]
        public string? BuyerGSTIN { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }           // Before discount

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDiscount { get; set; }      // Total discount amount

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxableAmount { get; set; }      // After discount, before GST

        [Column(TypeName = "decimal(18,2)")]
        public decimal CGSTAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SGSTAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal IGSTAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalGST { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrandTotal { get; set; }

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }

    public class InvoiceItem
    {
        [Key]
        public int InvoiceItemId { get; set; }

        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Required, MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Rate { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercent { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal GSTRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxableAmount { get; set; }      // (Rate * Qty) - Discount

        [Column(TypeName = "decimal(18,2)")]
        public decimal GSTAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }          // TaxableAmount + GSTAmount
    }
}
