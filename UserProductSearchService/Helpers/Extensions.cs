using Microsoft.AspNetCore.Mvc;

namespace UserProductSearchService.Helpers;

public static class Extensions
{
    public static async Task<ContentResult> ToContentResultAsync(
        this HttpResponseMessage responseMessage, string contentType
    )
    {
        return new ContentResult
        {
            StatusCode = (int)responseMessage.StatusCode,
            Content = await responseMessage.Content.ReadAsStringAsync(),
            ContentType = contentType
        };
    }
}