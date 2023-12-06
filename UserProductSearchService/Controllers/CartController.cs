using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using UserProductSearchService.Dtos;
using UserProductSearchService.Dtos.Cart;
using UserProductSearchService.Helpers;

namespace UserProductSearchService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CartController : Controller
{
    private readonly string _userServiceUrl;

    private readonly HttpClient _httpClient = new();

    public CartController(IConfiguration configuration)
    {
        var serviceUrls = configuration.GetSection("ServiceUrls");

        _userServiceUrl = serviceUrls["UserServiceUrl"]!;
    }

    [HttpGet("[action]")]
    [SwaggerOperation(Summary = "Получение корзины текущего пользователя")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(CartDto))]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Get()
    {
        var token = Request.Headers.Authorization.First() ?? "";

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_userServiceUrl}/api/Cart/Get");
        request.Headers.Add(Constants.Authorization, token);

        using var response = await _httpClient.SendAsync(request);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }

    [HttpGet("[action]/{userId}")]
    [SwaggerOperation(Summary = "Получение корзины пользователя по его GUID (ДЛЯ АДМИНОВ)")]
    [Authorize(Roles = "ADMIN")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(CartDto))]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetByUser(string userId)
    {
        var token = Request.Headers.Authorization.First() ?? "";

        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"{_userServiceUrl}/api/Cart/GetByUser/{userId}"
        );
        request.Headers.Add(Constants.Authorization, token);

        using var response = await _httpClient.SendAsync(request);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }

    [HttpPut("Item/[action]")]
    [SwaggerOperation(Summary = "Добавление в корзину продукта по его GUID")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(ErrorDto))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] Guid productId)
    {
        var token = Request.Headers.Authorization.First() ?? "";

        using var request = new HttpRequestMessage(HttpMethod.Put, $"{_userServiceUrl}/api/Cart/Item/Create");
        request.Content = JsonContent.Create(productId, new MediaTypeHeaderValue(Constants.ApplicationJson));
        request.Headers.Add(Constants.Authorization, token);

        using var response = await _httpClient.SendAsync(request);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }

    [HttpPut("Item/[action]")]
    [SwaggerOperation(Summary = "Изменение количества определённого продукта в корзине")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(ErrorDto))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> ChangeAmount([FromBody] ChangeAmountDto changeAmountDto)
    {
        var token = Request.Headers.Authorization.First() ?? "";

        using var request = new HttpRequestMessage(
            HttpMethod.Put, $"{_userServiceUrl}/api/Cart/Item/ChangeAmount"
        );
        request.Content = JsonContent.Create(changeAmountDto, new MediaTypeHeaderValue(Constants.ApplicationJson));
        ;

        request.Headers.Add(Constants.Authorization, token);

        using var response = await _httpClient.SendAsync(request);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }

    [HttpDelete("Item/[action]/{itemId}")]
    [SwaggerOperation(Summary = "Удаление позиции из корзины по ее GUID")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(ErrorDto))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Delete(string itemId)
    {
        var token = Request.Headers.Authorization.First() ?? "";

        using var request = new HttpRequestMessage(
            HttpMethod.Delete, $"{_userServiceUrl}/api/Cart/Item/Delete/{itemId}"
        );
        request.Headers.Add(Constants.Authorization, token);

        using var response = await _httpClient.SendAsync(request);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }

    [HttpDelete("[action]")]
    [SwaggerOperation(Summary = "Очистка корзины текущего пользователя")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Clear()
    {
        var token = Request.Headers.Authorization.First() ?? "";

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{_userServiceUrl}/api/Cart/Clear");
        request.Headers.Add(Constants.Authorization, token);

        using var response = await _httpClient.SendAsync(request);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }
}