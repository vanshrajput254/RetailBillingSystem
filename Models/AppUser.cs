using System.ComponentModel.DataAnnotations;

namespace RetailBillingSystem.Models
{
    public class AppUser
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Role { get; set; } = "Admin";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
