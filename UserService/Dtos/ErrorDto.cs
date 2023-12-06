using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UserService.Dtos;

public class ErrorDto
{
    public string Error { get; set; } = "";
    public string Message { get; set; } = "";
    public List<SimpleErrorDto> OtherErrors { get; set; } = new();


    public static bool ValidateModelAndGetErrorDto(ModelStateDictionary modelState, out ErrorDto? errorDto)
    {
        if (modelState.IsValid)
        {
            errorDto = null;
            return true;
        }

        var errors = modelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => new SimpleErrorDto
            {
                Error = e.Exception?.GetType().Name ?? nameof(Exception),
                Message = e.ErrorMessage
            })
            .OrderByDescending(e => e.Error == "")
            .ThenByDescending(e => e.Message == "")
            .ToList();

        if (errors.Count == 0) errorDto = new ErrorDto { Error = "", Message = "" };
        errorDto = new ErrorDto
        {
            Error = errors[0].Error,
            Message = errors[0].Message,
            OtherErrors = errors.GetRange(1, errors.Count - 1)
        };

        return false;
    }
}

public class SimpleErrorDto
{
    public string Error { get; init; }
    public string Message { get; init; }
}