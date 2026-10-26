namespace E_Commerce.DTOs.Products;

public class ProductPagedResponseDto
{
    public List<ProductResponseDto> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}