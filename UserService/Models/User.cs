namespace UserService.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? MiddleName { get; set; }
    public string Password { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // * One to one
    public Cart? Cart { get; set; }
    
    // * Many to one
    public ICollection<Order> Orders { get; set; } = new List<Order>();

    // * Many to many
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}