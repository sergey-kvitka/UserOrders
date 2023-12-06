namespace UserService.Dtos;

public class CartDto
{
    public Guid UserId { get; set; }
    public ICollection<CartItemDto> CartItems { get; set; }
}