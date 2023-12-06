namespace UserService.Dtos;

public class OrderDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Sum { get; set; }
    public int ProductsAmount { get; set; }
    public List<FinalOrderItemDto> Products { set; get; }
}