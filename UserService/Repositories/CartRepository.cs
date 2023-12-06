using Microsoft.EntityFrameworkCore;
using UserService.Context;
using UserService.Interfaces;
using UserService.Models;

namespace UserService.Repositories;

public class CartRepository : ICartRepository
{
    private readonly DataContext _context;
    private readonly IUserRepository _userRepository;

    public CartRepository(DataContext context, IUserRepository userRepository)
    {
        _context = context;
        _userRepository = userRepository;
    }

    public Cart? GetByUsername(string username)
    {
        var user = _userRepository.GetByUsername(username);
        if (user == null) return null;
        var cart = _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefault(c => user.Id.Equals(c.UserId));
        if (cart != null) return cart;

        cart = new Cart { UserId = user.Id, Customer = user };
        _context.Carts.Add(cart);
        _context.SaveChanges();

        return cart;
    }

    public void ClearById(Guid cartId)
    {
        var cart = _context.Carts.FirstOrDefault(c => cartId == c.Id);
        if (cart == null) return;

        cart.CartItems = new List<CartItem>();
        _context.SaveChanges();
    }

    public void SaveItemByCartId(CartItem cartItem, Guid cartId)
    {
        var cart = _context.Carts.FirstOrDefault(c => cartId == c.Id);
        if (cart == null) return;

        cart.CartItems.Add(cartItem);
        _context.SaveChanges();
    }

    public IEnumerable<CartItem> GetItemsByCartId(Guid cartId)
    {
        return _context.Carts
                   .Include(c => c.CartItems)
                   .FirstOrDefault(c => cartId == c.Id)?
                   .CartItems
               ?? new List<CartItem>();
    }

    public void DeleteItemById(Guid itemId)
    {
        var cart = _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefault(c => c.CartItems.Any(i => itemId == i.Id));
        if (cart == null) return;

        cart.CartItems = cart.CartItems.Where(i => itemId != i.Id).ToList();
        _context.SaveChanges();
    }

    public void SetAmountByItemId(Guid itemId, int amount, bool doAddition)
    {
        var item = _context.CartItems.FirstOrDefault(i => itemId == i.Id);
        if (item == null) return;

        item.Amount = doAddition
            ? item.Amount + amount
            : amount;
        _context.SaveChanges();
    }
}