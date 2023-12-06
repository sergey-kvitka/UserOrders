using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Swashbuckle.AspNetCore.Annotations;
using UserProductSearchService.Dtos.Product;
using UserProductSearchService.Helpers;

namespace UserProductSearchService.Controllers;

[Route("api/[controller]")]
[AllowAnonymous]
[ApiController]
public class ProductController : Controller
{
    private readonly string _productServiceUrl;

    private readonly HttpClient _httpClient = new();

    public ProductController(IConfiguration configuration)
    {
        var serviceUrls = configuration.GetSection("ServiceUrls");

        _productServiceUrl = serviceUrls["ProductServiceUrl"]!;
    }

    [HttpGet("[action]")]
    [SwaggerOperation(Summary = "Получение списка всех продуктов (можно отфильтровать и отсортировать)")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(ICollection<ProductDto>))]
    [ProducesResponseType(204)]
    public async Task<IActionResult> All(
        [SwaggerParameter(Description =
            "Будут получены только продукты с наличием данной подстроки в названии продуктов")]
        [FromQuery(Name = "nameLike")] string? nameLike,
        [SwaggerParameter(Description =
            "Параметры сортировки результата формата \"свойство.порядок свойство.порядок\". " +
            "Примеры: \"price.asc calories.desc\", \"calories.asc\"")]
        [FromQuery(Name = "orderBy")] string? orderOptions
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(QueryHelpers.AddQueryString(
                uri: $"{_productServiceUrl}/api/Product/All",
                queryString: new Dictionary<string, string?>
                {
                    { "nameLike", nameLike }, { "orderBy", orderOptions }
                }
            ))
        );

        using var response = await _httpClient.SendAsync(request);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }

    [HttpGet("[action]/{categoryId}")]
    [SwaggerOperation(Summary = "Получение списка продуктов по GUID категории (можно отфильтровать и отсортировать)")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(ICollection<ProductDto>))]
    [ProducesResponseType(204)]
    public async Task<IActionResult> GetByCategory(
        string categoryId,
        [SwaggerParameter(Description =
            "Будут получены только продукты с наличием данной подстроки в названии продуктов")]
        [FromQuery(Name = "nameLike")] string? nameLike,
        [SwaggerParameter(Description =
            "Параметры сортировки результата формата \"свойство.порядок свойство.порядок\". " +
            "Примеры: \"price.asc calories.desc\", \"calories.asc\"")]
        [FromQuery(Name = "orderBy")] string? orderOptions
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(QueryHelpers.AddQueryString(
                uri: $"{_productServiceUrl}/api/Product/GetByCategory/{categoryId}",
                queryString: new Dictionary<string, string?>
                {
                    { "nameLike", nameLike }, { "orderBy", orderOptions }
                }
            ))
        );

        using var response = await _httpClient.SendAsync(request);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }

    [HttpGet("Category/[action]")]
    [SwaggerOperation(Summary = "Получение списка всех категорий")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(ICollection<CategoryDto>))]
    [ProducesResponseType(204)]
    public async Task<IActionResult> All(
        [SwaggerParameter(Description = "Если true, будет также получено вложенное дерево подкатегорий")]
        [FromQuery(Name = "nested")] bool? nested
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(QueryHelpers.AddQueryString(
                uri: $"{_productServiceUrl}/api/Category/All",
                queryString: new Dictionary<string, string?>
                {
                    { "nested", (nested ?? false).ToString() }
                }
            ))
        );

        using var response = await _httpClient.SendAsync(request);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }

    [HttpGet("[action]/{categoryId}")]
    [SwaggerOperation(Summary = "Получение категории по ее GUID")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(CategoryDto))]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Category(
        string categoryId,
        [SwaggerParameter(Description = "Если true, будет также получено вложенное дерево подкатегорий")]
        [FromQuery(Name = "nested")] bool? nested
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(QueryHelpers.AddQueryString(
                uri: $"{_productServiceUrl}/api/Category/{categoryId}",
                queryString: new Dictionary<string, string?>
                {
                    { "nested", (nested ?? false).ToString() }
                }
            ))
        );

        using var response = await _httpClient.SendAsync(request);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }
}