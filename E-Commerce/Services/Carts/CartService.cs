
using E_Commerce.DTOs.Carts;
using E_Commerce.Models;
using E_Commerce.Models.Enums;
using E_Commerce.Repositories.Interfaces;

namespace E_Commerce.Services.Carts;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;

    public CartService(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<CartResponseDto> CreateCartAsync(long userId)
    {
        var existingCart =
            await _cartRepository.GetByUserIdAsync(userId);

        if (existingCart is not null)
            return MapToResponse(existingCart);

        var cart = new Cart
        {
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            IsDeleted = false,
            Items = new List<CartItem>()
        };

        await _cartRepository.AddCartAsync(cart);
        await _cartRepository.SaveChangesAsync();

        return MapToResponse(cart);
    }

    public async Task<CartResponseDto> GetCartAsync(
        long userId,
        long cartId)
    {
        var cart = await GetUserCartAsync(userId, cartId);

        return MapToResponse(cart);
    }

    public async Task<CartResponseDto> AddToCartAsync(
        long userId,
        long cartId,
        AddToCartDto dto)
    {
        if (dto.Quantity <= 0)
            throw new InvalidOperationException(
                "Quantity must be greater than zero.");

        var cart = await GetUserCartAsync(userId, cartId);

        var product =
            await _cartRepository.GetProductAsync(dto.ProductId);

        if (product is null || product.IsDeleted)
            throw new KeyNotFoundException("Product not found.");

        if (product.Status != ProductStatus.Active)
            throw new InvalidOperationException(
                "Product is not available.");

        if (product.StockQuantity < dto.Quantity)
            throw new InvalidOperationException(
                "Insufficient stock.");

        var cartItem = await _cartRepository.GetCartItemAsync(
            cart.Id,
            dto.ProductId);

        if (cartItem is not null)
        {
            var newQuantity = cartItem.Quantity + dto.Quantity;

            if (newQuantity > product.StockQuantity)
                throw new InvalidOperationException(
                    "Requested quantity exceeds available stock.");

            cartItem.Quantity = newQuantity;

            _cartRepository.UpdateCartItem(cartItem);
        }
        else
        {
            var deletedCartItem =
                await _cartRepository.GetDeletedCartItemAsync(
                    cart.Id,
                    dto.ProductId);

            if (deletedCartItem is not null)
            {
                deletedCartItem.IsDeleted = false;
                deletedCartItem.Quantity = dto.Quantity;

                _cartRepository.UpdateCartItem(deletedCartItem);
            }
            else
            {
                var newCartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = dto.Quantity,
                    IsDeleted = false
                };

                await _cartRepository.AddCartItemAsync(newCartItem);
            }
        }

        await _cartRepository.SaveChangesAsync();

        var updatedCart = await GetUserCartAsync(userId, cartId);

        return MapToResponse(updatedCart);
    }

    public async Task<CartResponseDto> UpdateQuantityAsync(
        long userId,
        long cartId,
        long productId,
        int quantity)
    {
        if (quantity <= 0)
            throw new InvalidOperationException(
                "Quantity must be greater than zero.");

        var cart = await GetUserCartAsync(userId, cartId);

        var product =
            await _cartRepository.GetProductAsync(productId);

        if (product is null || product.IsDeleted)
            throw new KeyNotFoundException("Product not found.");

        if (product.Status != ProductStatus.Active)
            throw new InvalidOperationException(
                "Product is not available.");

        if (quantity > product.StockQuantity)
            throw new InvalidOperationException(
                "Requested quantity exceeds available stock.");

        var cartItem = await _cartRepository.GetCartItemAsync(
            cart.Id,
            productId);

        if (cartItem is null)
            throw new KeyNotFoundException(
                "Product was not found in the cart.");

        cartItem.Quantity = quantity;

        _cartRepository.UpdateCartItem(cartItem);

        await _cartRepository.SaveChangesAsync();

        var updatedCart = await GetUserCartAsync(userId, cartId);

        return MapToResponse(updatedCart);
    }

    public async Task RemoveFromCartAsync(
        long userId,
        long cartId,
        long productId)
    {
        var cart = await GetUserCartAsync(userId, cartId);

        var cartItem = await _cartRepository.GetCartItemAsync(
            cart.Id,
            productId);

        if (cartItem is null)
            throw new KeyNotFoundException(
                "Product was not found in the cart.");

        _cartRepository.DeleteCartItem(cartItem);

        await _cartRepository.SaveChangesAsync();
    }

    public async Task ClearCartAsync(long userId, long cartId)
    {
        var cart = await GetUserCartAsync(userId, cartId);

        foreach (var item in cart.Items.Where(i => !i.IsDeleted))
        {
            _cartRepository.DeleteCartItem(item);
        }

        await _cartRepository.SaveChangesAsync();
    }

    public async Task DeleteCartAsync(long userId, long cartId)
    {
        var cart = await GetUserCartAsync(userId, cartId);

        _cartRepository.DeleteCart(cart);

        await _cartRepository.SaveChangesAsync();
    }

    private async Task<Cart> GetUserCartAsync(
        long userId,
        long cartId)
    {
        var cart = await _cartRepository.GetByIdAsync(cartId);

        if (cart is null ||
            cart.IsDeleted ||
            cart.UserId != userId)
        {
            throw new KeyNotFoundException(
                "Cart not found for the current user.");
        }

        return cart;
    }

    private static CartResponseDto MapToResponse(Cart cart)
    {
        return new CartResponseDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items
                .Where(item => !item.IsDeleted)
                .Select(item => new CartItemResponseDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? string.Empty,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product?.Price ?? 0
                })
                .ToList()
        };
    }
}
