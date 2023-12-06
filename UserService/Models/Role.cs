namespace UserService.Models;

public class Role : IEquatable<Role>
{
    public static readonly string DefaultCustomerRole = "CUSTOMER";
    public static readonly string DefaultAdminRole = "ADMIN";
    
    public Guid Id { get; set; }
    public string Name { get; set; }

    // * Many to many
    public ICollection<User> Users { get; set; } = new List<User>();

    public bool Equals(Role? other)
    {
        return other != null && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Role role && Id == role.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}