using UserService.Models;

namespace UserService.Interfaces;

public interface IOrderRepository
{
    public void MakeOrder(Order order, Guid userId);
    ICollection<Order> GetByUserId(Guid userId, string? orderOptions);
}