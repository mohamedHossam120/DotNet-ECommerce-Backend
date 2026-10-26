using E_Commerce.Models.Enums;

namespace E_Commerce.DTOs.Orders;

public class OrderResponseDto
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    public string ShippingAddressLine1 { get; set; } = string.Empty;

    public string? ShippingAddressLine2 { get; set; }

    public string ShippingCity { get; set; } = string.Empty;

    public string ShippingCountry { get; set; } = string.Empty;

    public string? ShippingPostalCode { get; set; }

    public string? ShippingPhoneNumber { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public List<OrderItemResponseDto> Items { get; set; } = new();
}