using System;
using System.Linq;

namespace OtelV3.Models
{
    public class CustomerModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public DateTime? LastVisitDate { get; set; }
        public string LastVisitStatus { get; set; } = string.Empty; // "Check-out yapıldı", "Şu an konaklıyor"
        public bool IsVip { get; set; }
        public bool IsBlacklisted { get; set; }
        public int VisitCount { get; set; }
        public string? AvatarUrl { get; set; }

        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name)) return "??";
                var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 1) return parts[0][..1].ToUpper();
                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            }
        }

        // Helper boolean for Avatar display logic
        public bool HasAvatar => !string.IsNullOrEmpty(AvatarUrl);
    }
}
