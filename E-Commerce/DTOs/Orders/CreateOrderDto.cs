using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOs.Orders;

public class CreateOrderDto
{
    [Required, MaxLength(200)]
    public string ShippingAddressLine1 { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ShippingAddressLine2 { get; set; }

    [Required, MaxLength(100)]
    public string ShippingCity { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ShippingCountry { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? ShippingPostalCode { get; set; }

    [Phone, MaxLength(30)]
    public string? ShippingPhoneNumber { get; set; }
}