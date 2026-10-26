using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOs.Products;

public class ProductQueryDto
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    [Range(1, long.MaxValue)]
    public long? CategoryId { get; set; }
}