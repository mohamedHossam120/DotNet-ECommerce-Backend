using E_Commerce.Data.Repositories.Interfaces;
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(long id)
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .OrderBy(c => c.Id)
            .ToListAsync();
    }

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
    }

    public void Update(Category category)
    {
        _context.Categories.Update(category);
    }

    public void Delete(Category category)
    {
        category.IsDeleted = true;
        category.DeletedAtUtc = DateTime.UtcNow;
        category.UpdatedAtUtc = DateTime.UtcNow;

        _context.Categories.Update(category);
    }

    public Task<bool> ExistsAsync(long id)
    {
        return _context.Categories.AnyAsync(c => c.Id == id);
    }

    public Task<bool> HasProductsAsync(long categoryId)
    {
        return _context.Products
            .AnyAsync(p => p.CategoryId == categoryId);
    }

    public Task<bool> HasChildrenAsync(long categoryId)
    {
        return _context.Categories
            .AnyAsync(c => c.ParentCategoryId == categoryId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}