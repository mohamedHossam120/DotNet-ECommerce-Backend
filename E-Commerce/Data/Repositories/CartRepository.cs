using E_Commerce.Models;
using E_Commerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Data.Repositories;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByIdAsync(long cartId)
    {
        return await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.Id == cartId);
    }

    public async Task<Cart?> GetByUserIdAsync(long userId)
    {
        return await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Product?> GetProductAsync(long productId)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);
    }

    public async Task<CartItem?> GetCartItemAsync(
        long cartId,
        long productId)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(i =>
                i.CartId == cartId &&
                i.ProductId == productId);
    }

    public async Task<CartItem?> GetDeletedCartItemAsync(
        long cartId,
        long productId)
    {
        return await _context.CartItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i =>
                i.CartId == cartId &&
                i.ProductId == productId &&
                i.IsDeleted);
    }

    public async Task AddCartAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
    }

    public async Task AddCartItemAsync(CartItem cartItem)
    {
        await _context.CartItems.AddAsync(cartItem);
    }

    public void UpdateCartItem(CartItem cartItem)
    {
        _context.CartItems.Update(cartItem);
    }

    public void DeleteCartItem(CartItem cartItem)
    {
        cartItem.IsDeleted = true;
    }

    public void DeleteCart(Cart cart)
    {
        cart.IsDeleted = true;

        foreach (var item in cart.Items.Where(i => !i.IsDeleted))
        {
            item.IsDeleted = true;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}