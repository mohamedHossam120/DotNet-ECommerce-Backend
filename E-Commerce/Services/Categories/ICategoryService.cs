using E_Commerce.DTOs.Categories;

namespace E_Commerce.Services.Categories;

public interface ICategoryService
{
    Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto);
    Task<List<CategoryResponseDto>> GetAllAsync();
    Task<CategoryResponseDto> GetByIdAsync(long id);
    Task<CategoryResponseDto> UpdateAsync(long id, UpdateCategoryDto dto);
    Task DeleteAsync(long id);
}