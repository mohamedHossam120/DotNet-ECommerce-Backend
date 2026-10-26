
using System.Security.Claims;
using E_Commerce.DTOs.Carts;
using E_Commerce.Services.Carts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartsController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartsController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpPost]
    public async Task<ActionResult<CartResponseDto>> CreateCart()
    {
        try
        {
            var userId = GetUserId();
            var cart = await _cartService.CreateCartAsync(userId);

            return Ok(cart);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{cartId:long}")]
    public async Task<ActionResult<CartResponseDto>> GetCart(long cartId)
    {
        try
        {
            var userId = GetUserId();
            var cart = await _cartService.GetCartAsync(userId, cartId);

            return Ok(cart);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{cartId:long}/items")]
    public async Task<ActionResult<CartResponseDto>> AddToCart(
        long cartId,
        [FromBody] AddToCartDto dto)
    {
        try
        {
            var userId = GetUserId();

            var cart = await _cartService.AddToCartAsync(
                userId, cartId, dto);

            return Ok(cart);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{cartId:long}/items/{productId:long}")]
    public async Task<ActionResult<CartResponseDto>> UpdateQuantity(
        long cartId,
        long productId,
        [FromBody] UpdateCartItemDto dto)
    {
        try
        {
            var userId = GetUserId();

            var cart = await _cartService.UpdateQuantityAsync(
                userId,
                cartId,
                productId,
                dto.Quantity);

            return Ok(cart);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{cartId:long}/items/{productId:long}")]
    public async Task<IActionResult> RemoveFromCart(
        long cartId,
        long productId)
    {
        try
        {
            var userId = GetUserId();

            await _cartService.RemoveFromCartAsync(
                userId, cartId, productId);

            return Ok(new
            {
                message = "Product removed from cart successfully."
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

   
    [HttpDelete("{cartId:long}/items")]
    public async Task<IActionResult> ClearCart(long cartId)
    {
        try
        {
            var userId = GetUserId();

            await _cartService.ClearCartAsync(userId, cartId);

            return Ok(new
            {
                message = "Cart cleared successfully."
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    
    [HttpDelete("{cartId:long}")]
    public async Task<IActionResult> DeleteCart(long cartId)
    {
        try
        {
            var userId = GetUserId();

            await _cartService.DeleteCartAsync(userId, cartId);

            return Ok(new
            {
                message = "Cart deleted successfully."
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private long GetUserId()
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!long.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException(
                "User ID was not found or is invalid in the token.");
        }

        return userId;
    }
}
