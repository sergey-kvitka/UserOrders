using System.Linq.Expressions;
using ProductService.Dtos;
using ProductService.Models;

namespace ProductService.Interfaces;

public interface IProductRepository
{
    public ICollection<Product> GetAllProducts(string? nameLike, string? orderOptions);

    public ICollection<Product> GetByCategoryId(Guid categoryId, string? nameLike, string? orderOptions);

    public ICollection<FinalOrderItemDto> MakeOrder(IEnumerable<OrderItemDto> orderItemDtos);
    Product? GetById(Guid productId);
}