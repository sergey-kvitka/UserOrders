namespace UserProductSearchService.Dtos.Cart;

public class ChangeAmountDto
{
    public Guid cartItemId { get; set; }
    public int Amount { get; set; }
}