namespace E_Commerce.DTOs.Categories;

public class CategoryResponseDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
}