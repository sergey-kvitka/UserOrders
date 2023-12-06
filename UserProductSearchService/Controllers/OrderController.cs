using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Swashbuckle.AspNetCore.Annotations;
using UserProductSearchService.Dtos;
using UserProductSearchService.Dtos.Order;
using UserProductSearchService.Helpers;

namespace UserProductSearchService.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class OrderController : Controller
{
    private readonly string _userServiceUrl;

    private readonly HttpClient _httpClient = new();

    public OrderController(IConfiguration configuration)
    {
        var serviceUrls = configuration.GetSection("ServiceUrls");

        _userServiceUrl = serviceUrls["UserServiceUrl"]!;
    }

    [HttpPut]
    [SwaggerOperation(Summary = "Оформление (создание) заказа на основе хранящихся в корзине товаров")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(ErrorDto))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Make()
    {
        var token = Request.Headers.Authorization.First() ?? "";

        using var request = new HttpRequestMessage(HttpMethod.Put, $"{_userServiceUrl}/api/Order/Make");
        request.Headers.Add(Constants.Authorization, token);

        using var response = await _httpClient.SendAsync(request);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Получение списка заказов текущего пользователя (можно отсортировать)")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(ICollection<OrderDto>))]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetOrders(
        [SwaggerParameter(Description =
            "Параметры сортировки результата формата \"свойство.порядок свойство.порядок\". " +
            "Примеры: \"productsAmount.asc sum.desc\", \"sum.asc\"")]
        [FromQuery(Name = "orderBy")]
        string? orderOptions
    )
    {
        var token = Request.Headers.Authorization.First() ?? "";

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(QueryHelpers.AddQueryString(
                uri: $"{_userServiceUrl}/api/Order/GetOrders",
                queryString: new Dictionary<string, string?>
                {
                    { "orderBy", orderOptions }
                }
            ))
        );
        request.Headers.Add(Constants.Authorization, token);

        using var response = await _httpClient.SendAsync(request);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }

    [HttpGet("{userId}")]
    [SwaggerOperation(Summary =
        "Получение списка заказов пользователя по его GUID (можно отсортировать) (ДЛЯ АДМИНОВ)")]
    [Authorize(Roles = "ADMIN")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(ICollection<OrderDto>))]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetByUser(
        string userId,
        [SwaggerParameter(Description =
            "Параметры сортировки результата формата \"свойство.порядок свойство.порядок\". " +
            "Примеры: \"productsAmount.asc sum.desc\", \"sum.asc\"")]
        [FromQuery(Name = "orderBy")]
        string? orderOptions
    )
    {
        var token = Request.Headers.Authorization.First() ?? "";

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(QueryHelpers.AddQueryString(
                uri: $"{_userServiceUrl}/api/Order/GetByUser/{userId}",
                queryString: new Dictionary<string, string?>
                {
                    { "orderBy", orderOptions }
                }
            ))
        );
        request.Headers.Add(Constants.Authorization, token);

        using var response = await _httpClient.SendAsync(request);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }

    [HttpGet("{orderId}")]
    [SwaggerOperation(Summary = "Получение одного заказа текущего пользователя по GUID заказа")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(OrderDto))]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetById(string orderId)
    {
        var token = Request.Headers.Authorization.First() ?? "";

        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"{_userServiceUrl}/api/Order/GetById/{orderId}"
        );
        request.Headers.Add(Constants.Authorization, token);

        using var response = await _httpClient.SendAsync(request);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }
}