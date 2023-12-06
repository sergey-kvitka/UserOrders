namespace UserProductSearchService.Dtos.Cart;

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public uint Amount { get; set; }
}