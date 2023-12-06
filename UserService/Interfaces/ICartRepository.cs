using UserService.Models;

namespace UserService.Interfaces;

public interface ICartRepository
{
    public Cart? GetByUsername(string username);
    void ClearById(Guid cartId);

    void SaveItemByCartId(CartItem cartItem, Guid cartId);
    void SetAmountByItemId(Guid itemId, int amount, bool doAddition);
    void DeleteItemById(Guid itemId);
    IEnumerable<CartItem> GetItemsByCartId(Guid cartId);
}