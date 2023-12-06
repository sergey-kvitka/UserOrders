using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Models;

public class Product
{
    public Guid Id { get; set; }
    
    [MaxLength(120)] public string Name { get; set; }
    
    [Precision(18, 2)] public decimal Price { get; set; }
    
    public string? Description { get; set; }
    public string? Compound { get; set; }
    public int StorageAmount { get; set; }
    public decimal Weight { get; set; }
    public decimal Calories { get; set; }
    public decimal Proteins { get; set; }
    public decimal Fats { get; set; }
    public decimal Carbohydrates { get; set; }

    // * One to many
    public Category Category { get; set; }
}