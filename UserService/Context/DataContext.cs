using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using UserService.Dtos;
using UserService.Models;

namespace UserService.Context;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new OrderConfiguration());

        builder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        
        builder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        builder.Entity<User>()
            .HasIndex(u => u.Phone)
            .IsUnique();
        
        builder.Entity<Cart>()
            .HasIndex(c => c.UserId)
            .IsUnique();
    }
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        var jsonSerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        builder.Property(e => e.Products).HasConversion(
            convertToProviderExpression: v => JsonConvert.SerializeObject(v,
                jsonSerializerSettings),
            convertFromProviderExpression: v => JsonConvert.DeserializeObject<List<FinalOrderItemDto>>(v,
                jsonSerializerSettings
            ) ?? new List<FinalOrderItemDto>()
        );
    }
}