using E_Commerce.Models.Enums;

namespace E_Commerce.Models
{
    public class Order
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public User User { get; set; } = null!;

        public OrderStatus Status { get; set; }

        public decimal TotalAmount { get; set; }

        public string ShippingAddressLine1 { get; set; } = string.Empty;

        public string? ShippingAddressLine2 { get; set; }

        public string ShippingCity { get; set; } = string.Empty;

        public string ShippingCountry { get; set; } = string.Empty;

        public string? ShippingPostalCode { get; set; }

        public string? ShippingPhoneNumber { get; set; }

        public ICollection<OrderItem> Items { get; set; }
            = new List<OrderItem>();

        public bool IsDeleted { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }
    }
}
