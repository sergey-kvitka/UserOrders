using Microsoft.EntityFrameworkCore;
using UserService.Context;
using UserService.Interfaces;
using UserService.Models;

namespace UserService.Repositories;

public class OrderRepository : IOrderRepository
{
    private static readonly List<string> OrderOptions = new() { "asc", "desc" };

    private readonly DataContext _context;

    public OrderRepository(DataContext context)
    {
        _context = context;
    }

    public void MakeOrder(Order order, Guid userId)
    {
        var user = _context.Users
            .Include(u => u.Orders)
            .Include(u => u.Cart)
            .ThenInclude(c => c!.CartItems)
            .FirstOrDefault(u => userId == u.Id);
        if (user == null) return;

        user.Orders.Add(order);
        if (user.Cart != null)
            user.Cart.CartItems = new List<CartItem>();

        _context.SaveChanges();
    }

    public ICollection<Order> GetByUserId(Guid userId, string? orderOptions)
    {
        var user = _context.Users
            .Include(u => u.Orders)
            .FirstOrDefault(u => userId == u.Id);

        return user == null
            ? new List<Order>()
            : OrderByFunctionFromString(user.Orders, orderOptions);
    }

    private static ICollection<Order> OrderByFunctionFromString(ICollection<Order> orders, string? orderOptions)
    {
        if (orderOptions == null || orders.Count < 2)
        {
            return orders;
        }

        var actions = orderOptions.Trim().Split(" ")
            .Select(s => s.Trim().Split("."))
            .Where(p => p.Length == 2 && OrderOptions.Contains(p.Last().ToLower()));

        var orderedOrders = orders.OrderByDescending(o => o.Date);
        var wasOrdered = false;

        foreach (var action in actions)
        {
            var property = action.First();

            var selection = SelectionFunctionByPropName(property);
            if (selection == null) continue;

            var option = action.Last();
            orderedOrders = option == OrderOptions.First()
                ? wasOrdered
                    ? orderedOrders.ThenBy(o => selection(o))
                    : orderedOrders.OrderBy(o => selection(o))
                : wasOrdered
                    ? orderedOrders.ThenByDescending(o => selection(o))
                    : orderedOrders.OrderByDescending(o => selection(o));
            wasOrdered = true;
        }

        return orderedOrders.ToList();
    }

    private static Func<Order, dynamic?>? SelectionFunctionByPropName(string property)
    {
        return property.ToLower() switch
        {
            "id" => o => o.Id,
            "date" => o => o.Date,
            "sum" => o => o.Sum,
            "productsamount" => o => o.ProductsAmount,
            _ => null
        };
    }
}