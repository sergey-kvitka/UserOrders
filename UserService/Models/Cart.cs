namespace UserService.Models;

public class Cart
{
    public Guid Id { get; set; }

    // * One to one
    public Guid UserId { get; set; }
    public User Customer { get; set; }
    
    // * Many to one
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}