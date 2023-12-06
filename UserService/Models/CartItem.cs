namespace UserService.Models;

public class CartItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Amount { get; set; }

    // * One to many
    public Cart Cart { get; set; }
}