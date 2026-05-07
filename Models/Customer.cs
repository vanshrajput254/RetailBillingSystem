using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RetailBillingSystem.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? GSTIN { get; set; }

        [MaxLength(15)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(10)]
        public string? PinCode { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation - JsonIgnore to prevent circular reference
        [JsonIgnore]
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

        // Computed properties
        [NotMapped]
        public int TotalInvoices => Invoices?.Count ?? 0;

        [NotMapped]
        public decimal TotalBilled => Invoices?.Sum(i => i.GrandTotal) ?? 0;

        [NotMapped]
        public decimal BalanceDue => Invoices?.Where(i => i.Status != InvoiceStatus.Paid)
                                                .Sum(i => i.GrandTotal) ?? 0;
    }
}
