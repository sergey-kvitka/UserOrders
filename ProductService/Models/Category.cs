using System.ComponentModel.DataAnnotations;

namespace ProductService.Models;

public class Category
{
    public Guid Id { get; set; }
    
    [MaxLength(60)]
    public string Name { get; set; }
    
    public string? Description { get; set; }

    // * One to many
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    // * Many to one
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
}   