namespace E_Commerce.DTOs.Products;

public class ProductResponseDto
{
    public long Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int Status { get; set; }
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}