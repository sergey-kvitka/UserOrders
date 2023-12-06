using UserService.Dtos;

namespace UserService.Models;

public class Order
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Sum { get; set; }
    public int ProductsAmount { get; set; }
    public List<FinalOrderItemDto> Products { set; get; }

    // * One to many
    public User Client { get; set; }
}