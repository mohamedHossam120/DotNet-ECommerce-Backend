using E_Commerce.Models;

namespace E_Commerce.Repositories.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(long cartId);
    Task<Cart?> GetByUserIdAsync(long userId);

    Task<Product?> GetProductAsync(long productId);

    Task<CartItem?> GetCartItemAsync(
        long cartId,
        long productId);

    Task<CartItem?> GetDeletedCartItemAsync(
        long cartId,
        long productId);

    Task AddCartAsync(Cart cart);
    Task AddCartItemAsync(CartItem cartItem);

    void UpdateCartItem(CartItem cartItem);

    void DeleteCartItem(CartItem cartItem);
    void DeleteCart(Cart cart);

    Task SaveChangesAsync();
}