using E_Commerce.Data.Repositories.Interfaces;
using E_Commerce.DTOs.Products;
using E_Commerce.Models;
using E_Commerce.Models.Enums;

namespace E_Commerce.Services.Products;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
    {
        ValidateStatus(dto.Status);

        var sku = dto.SKU.Trim();

        if (await _productRepository.ExistsBySkuAsync(sku))
        {
            throw new InvalidOperationException(
                "A product with this SKU already exists.");
        }

        if (!await _productRepository.CategoryExistsAsync(dto.CategoryId))
        {
            throw new KeyNotFoundException("Category not found.");
        }

        var product = new Product
        {
            SKU = sku,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            Status = (ProductStatus)dto.Status,
            CategoryId = dto.CategoryId,
            CreatedAtUtc = DateTime.UtcNow
        };

        // Keep stock and status consistent.
        if (product.StockQuantity == 0 &&
            product.Status == ProductStatus.Active)
        {
            product.Status = ProductStatus.OutOfStock;
        }

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        var created = await _productRepository.GetByIdAsync(product.Id);

        if (created is null)
            throw new InvalidOperationException("Product was created but could not be loaded.");

        return MapToDto(created);
    }

    public async Task<List<ProductResponseDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(MapToDto).ToList();
    }

    public async Task<ProductPagedResponseDto> GetPagedAsync(
        ProductQueryDto query)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var result = await _productRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            query.Search,
            query.CategoryId);

        return new ProductPagedResponseDto
        {
            Items = result.Products.Select(MapToDto).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = result.TotalCount,
            TotalPages = (int)Math.Ceiling(
                result.TotalCount / (double)pageSize)
        };
    }

    public async Task<ProductResponseDto> GetByIdAsync(long id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
            throw new KeyNotFoundException("Product not found.");

        return MapToDto(product);
    }

    public async Task<ProductResponseDto> UpdateAsync(
        long id,
        UpdateProductDto dto)
    {
        ValidateStatus(dto.Status);

        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
            throw new KeyNotFoundException("Product not found.");

        if (!await _productRepository.CategoryExistsAsync(dto.CategoryId))
            throw new KeyNotFoundException("Category not found.");

        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim();
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.CategoryId = dto.CategoryId;
        product.Status = (ProductStatus)dto.Status;
        product.UpdatedAtUtc = DateTime.UtcNow;

        if (product.StockQuantity == 0 &&
            product.Status == ProductStatus.Active)
        {
            product.Status = ProductStatus.OutOfStock;
        }
        else if (product.StockQuantity > 0 &&
                 product.Status == ProductStatus.OutOfStock)
        {
            product.Status = ProductStatus.Active;
        }

        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync();

        // إعادة الجلب لضمان تحميل اسم القسم الجديد بدقة لو تم تغييره
        var updated = await _productRepository.GetByIdAsync(id);

        if (updated is null)
            throw new InvalidOperationException("Product was updated but could not be loaded.");

        return MapToDto(updated);
    }

    public async Task DeleteAsync(long id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
            throw new KeyNotFoundException("Product not found.");

        _productRepository.Delete(product);
        await _productRepository.SaveChangesAsync();
    }

    private static void ValidateStatus(int status)
    {
        if (!Enum.IsDefined(typeof(ProductStatus), status))
        {
            throw new ArgumentException("Invalid product status.");
        }
    }

    private static ProductResponseDto MapToDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Status = (int)product.Status,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty
        };
    }
}