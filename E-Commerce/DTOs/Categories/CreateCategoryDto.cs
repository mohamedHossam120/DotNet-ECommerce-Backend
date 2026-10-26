using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOs.Categories;

public class CreateCategoryDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, long.MaxValue)]
    public long? ParentCategoryId { get; set; }
}