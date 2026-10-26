using E_Commerce.Data.Repositories.Interfaces;
using E_Commerce.DTOs.Categories;
using E_Commerce.Models;

namespace E_Commerce.Services.Categories;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto)
    {
        // 1. فحص تكرار الاسم أولاً (منعا لتكرار الاسم نهائياً)
        var allCategories = await _categoryRepository.GetAllAsync();
        if (allCategories.Any(c => c.Name.Equals(dto.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Category name already exists.");
        }

        if (dto.ParentCategoryId.HasValue)
        {
            if (!await _categoryRepository.ExistsAsync(dto.ParentCategoryId.Value))
                throw new KeyNotFoundException("Parent category not found.");
        }

        var category = new Category
        {
            Name = dto.Name.Trim(),
            ParentCategoryId = dto.ParentCategoryId,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        var created = await _categoryRepository.GetByIdAsync(category.Id);

        return MapToDto(created ?? category);
    }

    public async Task<List<CategoryResponseDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();

        return categories.Select(MapToDto).ToList();
    }

    public async Task<CategoryResponseDto> GetByIdAsync(long id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category is null)
            throw new KeyNotFoundException("Category not found.");

        return MapToDto(category);
    }

    public async Task<CategoryResponseDto> UpdateAsync(long id, UpdateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category is null)
            throw new KeyNotFoundException("Category not found.");

        var allCategories = await _categoryRepository.GetAllAsync();
        if (allCategories.Any(c => c.Id != id && c.Name.Equals(dto.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Category name already exists.");
        }

        if (dto.ParentCategoryId == id)
            throw new InvalidOperationException("A category cannot be its own parent.");

        if (dto.ParentCategoryId.HasValue)
        {
            if (!await _categoryRepository.ExistsAsync(dto.ParentCategoryId.Value))
                throw new KeyNotFoundException("Parent category not found.");

            var descendants = await GetDescendantIdsAsync(id);

            if (descendants.Contains(dto.ParentCategoryId.Value))
            {
                throw new InvalidOperationException("A category cannot be moved under one of its descendants.");
            }
        }

        category.Name = dto.Name.Trim();
        category.ParentCategoryId = dto.ParentCategoryId;
        category.UpdatedAtUtc = DateTime.UtcNow;

        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();

        var updated = await _categoryRepository.GetByIdAsync(id);

        return MapToDto(updated ?? category);
    }

    public async Task DeleteAsync(long id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category is null)
            throw new KeyNotFoundException("Category not found.");

        _categoryRepository.Delete(category);
        await _categoryRepository.SaveChangesAsync();
    }

    private async Task<HashSet<long>> GetDescendantIdsAsync(long categoryId)
    {
        var allCategories = await _categoryRepository.GetAllAsync();
        var descendants = new HashSet<long>();
        var pending = new Stack<long>();

        pending.Push(categoryId);

        while (pending.Count > 0)
        {
            var currentId = pending.Pop();

            foreach (var child in allCategories.Where(c => c.ParentCategoryId == currentId))
            {
                if (descendants.Add(child.Id))
                    pending.Push(child.Id);
            }
        }

        return descendants;
    }

    private static CategoryResponseDto MapToDto(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name
        };
    }
}