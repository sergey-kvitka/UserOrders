using Microsoft.EntityFrameworkCore;
using ProductService.Context;
using ProductService.Interfaces;
using ProductService.Models;

namespace ProductService.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly DataContext _context;

    public CategoryRepository(DataContext context)
    {
        _context = context;
    }

    public Category? GetById(Guid id, bool withNested)
    {
        var category = _context.Categories.FirstOrDefault(c => c.Id == id);
        if (withNested && category != null) LoadChildren(category);
        return category;
    }

    public ICollection<Category> GetAllCategories(bool withNested)
    {
        var categories = _context.Categories
            .Include(c => c.ParentCategory)
            .Where(c => c.ParentCategory == null)
            .ToList();
        if (!withNested) return categories;

        foreach (var category in categories)
        {
            LoadChildren(category);
        }

        return categories;
    }

    private void LoadChildren(Category category)
    {
        category.SubCategories = _context.Categories
            .Where(c => c.ParentCategoryId == category.Id)
            .ToList();
        foreach (var subCategory in category.SubCategories)
        {
            LoadChildren(subCategory);
        }
    }
}