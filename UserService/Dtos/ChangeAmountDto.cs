namespace UserService.Dtos;

public class ChangeAmountDto
{
    public Guid cartItemId { get; set; }
    public int Amount { get; set; }
}