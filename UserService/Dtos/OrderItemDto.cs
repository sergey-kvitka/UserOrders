using UserService.Models;

namespace UserService.Dtos;

public class OrderItemDto
{
    public OrderItemDto()
    {
    }

    public OrderItemDto(CartItem cartItem)
    {
        ProductId = cartItem.ProductId.ToString();
        Amount = cartItem.Amount;
    }

    public string ProductId { get; set; }
    public int Amount { get; set; }
}