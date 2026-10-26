using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOs.Products;

public class CreateProductDto
{
    [Required, StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Range(1, 3)]
    public int Status { get; set; } = 1;

    [Range(1, long.MaxValue)]
    public long CategoryId { get; set; }
}