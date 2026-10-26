using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOs.Carts;

public class UpdateCartItemDto
{
    [Range(1, 1000)]
    public int Quantity { get; set; }
}
