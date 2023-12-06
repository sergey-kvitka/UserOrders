namespace UserProductSearchService.Dtos;

public class ErrorDto
{
    public string Error { get; set; } = "";
    public string Message { get; set; } = "";
    public List<SimpleErrorDto> OtherErrors { get; set; } = new();
}

public struct SimpleErrorDto
{
    public string Error { get; init; }
    public string Message { get; init; }
}