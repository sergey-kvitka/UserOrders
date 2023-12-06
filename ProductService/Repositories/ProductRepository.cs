using Microsoft.EntityFrameworkCore;
using ProductService.Context;
using ProductService.Dtos;
using ProductService.Helpers;
using ProductService.Interfaces;
using ProductService.Models;

namespace ProductService.Repositories;

public class ProductRepository : IProductRepository
{
    private static readonly List<string> OrderOptions = new() { "asc", "desc" };

    private readonly DataContext _context;
    private readonly ICategoryRepository _categoryRepository;

    public ProductRepository(DataContext context, ICategoryRepository categoryRepository)
    {
        _context = context;
        _categoryRepository = categoryRepository;
    }

    public ICollection<Product> GetAllProducts(
        string? nameLike,
        string? orderOptions
    )
    {
        var products = _context.Products
            .Where(p => nameLike == null || p.Name.ToLower().Contains(nameLike.Trim().ToLower()))
            .ToList();
        return OrderByFunctionFromString(products, orderOptions);
    }

    public ICollection<Product> GetByCategoryId(Guid categoryId, string? nameLike, string? orderOptions)
    {
        var category = _categoryRepository.GetById(categoryId, true);
        if (category == null) return new List<Product>();

        var categoryIds = new[] { category }
            .Flatten(c => c.SubCategories)
            .Select(c => c.Id);

        var products = _context.Products
            .Include(p => p.Category)
            .Where(p => categoryIds.Contains(p.Category.Id))
            .Where(p => nameLike == null || p.Name.ToLower().Contains(nameLike.Trim().ToLower()))
            .ToList();
        return OrderByFunctionFromString(products, orderOptions);
    }

    public ICollection<FinalOrderItemDto> MakeOrder(IEnumerable<OrderItemDto> orderItemDtos)
    {
        var pairs = orderItemDtos
            .DistinctBy(i => i.ProductId)
            .ToDictionary(i => new Guid(i.ProductId), i => i.Amount);

        var productIds = new List<Guid>(pairs.Keys);
        var products = _context.Products
            .Where(p => productIds.Contains(p.Id));

        var order = new List<FinalOrderItemDto>();

        foreach (var product in products)
        {
            var orderAmount = pairs[product.Id];
            switch (orderAmount)
            {
                case < 0:
                    throw new ArgumentException("Product amount in cart can't be less than 0");
                case 0:
                    continue;
            }

            var oldStorageAmount = product.StorageAmount;
            if (orderAmount > oldStorageAmount)
                throw new ArgumentException("Product amount in cart can't be more than storage amount");

            product.StorageAmount -= orderAmount;

            order.Add(new FinalOrderItemDto
            {
                ProductId = product.Id,
                Price = product.Price,
                Amount = orderAmount,
                TotalItemPrice = product.Price * orderAmount
            });
        }

        _context.SaveChanges();
        return order;
    }

    public Product? GetById(Guid productId)
    {
        return _context.Products.FirstOrDefault(p => p.Id == productId);
    }

    private static ICollection<Product> OrderByFunctionFromString(ICollection<Product> products, string? orderOptions)
    {
        if (orderOptions == null || products.Count < 2)
        {
            return products;
        }

        var actions = orderOptions.Trim().Split(" ")
            .Select(s => s.Trim().Split("."))
            .Where(p => p.Length == 2 && OrderOptions.Contains(p.Last().ToLower()));

        var orderedProducts = products.OrderBy(p => p.Id);
        var wasOrdered = false;

        foreach (var action in actions)
        {
            var property = action.First();

            var selection = SelectionFunctionByPropName(property);
            if (selection == null) continue;

            var option = action.Last();
            orderedProducts = option == OrderOptions.First()
                ? wasOrdered
                    ? orderedProducts.ThenBy(p => selection(p))
                    : orderedProducts.OrderBy(p => selection(p))
                : wasOrdered
                    ? orderedProducts.ThenByDescending(p => selection(p))
                    : orderedProducts.OrderByDescending(p => selection(p));
            wasOrdered = true;
        }

        return orderedProducts.ToList();
    }

    private static Func<Product, dynamic?>? SelectionFunctionByPropName(string property)
    {
        return property.ToLower() switch
        {
            "id" => p => p.Id,
            "name" => p => p.Name,
            "price" => p => p.Price,
            "description" => p => p.Description,
            "compound" => p => p.Compound,
            "storageamount" => p => p.StorageAmount,
            "weight" => p => p.Weight,
            "calories" => p => p.Calories,
            "proteins" => p => p.Proteins,
            "fats" => p => p.Fats,
            "carbohydrates" => p => p.Carbohydrates,
            _ => null
        };
    }
}