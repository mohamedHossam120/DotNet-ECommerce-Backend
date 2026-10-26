namespace E_Commerce.Models
{
    public class RefreshToken
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public User User { get; set; } = null!;

        public string TokenHash { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? RevokedAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public bool IsRevoked => RevokedAtUtc.HasValue;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    }
}
