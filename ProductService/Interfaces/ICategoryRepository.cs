using ProductService.Models;

namespace ProductService.Interfaces;

public interface ICategoryRepository
{
    public Category? GetById(Guid id, bool withNested);
    public ICollection<Category> GetAllCategories(bool withNested);
    
}