namespace UserProductSearchService.Dtos.Order;

public class FinalOrderItemDto
{
    public Guid ProductId { get; set; }
    public decimal Price { get; set; }
    public int Amount { get; set; }
    public decimal TotalItemPrice { get; set; }
}