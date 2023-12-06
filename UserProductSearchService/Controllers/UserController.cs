using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Swashbuckle.AspNetCore.Annotations;
using UserProductSearchService.Dtos.User;
using UserProductSearchService.Helpers;

namespace UserProductSearchService.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class UserController : Controller
{
    private readonly string _userServiceUrl;

    private readonly HttpClient _httpClient = new();

    public UserController(IConfiguration configuration)
    {
        var serviceUrls = configuration.GetSection("ServiceUrls");

        _userServiceUrl = serviceUrls["UserServiceUrl"]!;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Получение данных профиля текущего пользователя")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(UserProfileDto))]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Profile()
    {
        var token = Request.Headers.Authorization.First() ?? "";

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_userServiceUrl}/api/User/Profile");
        request.Headers.Add(Constants.Authorization, token);

        using var response = await _httpClient.SendAsync(request);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }

    [HttpGet]
    [SwaggerOperation(Summary =
        "Получение данных профиля пользователя по его GUID (можно отфильтровать и отсротировать) (ДЛЯ АДМИНОВ)")]
    [Authorize(Roles = "ADMIN")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(ICollection<UserProfileDto>))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetUsers(
        [SwaggerParameter(Description = "Будут получены только пользователи с наличием данной подстроки в username")]
        [FromQuery(Name = "usernameLike")] string? usernameLike,
        [SwaggerParameter(Description =
            "Параметры сортировки результата формата \"свойство.порядок свойство.порядок\". " +
            "Примеры: \"name.asc email.desc\", \"username.asc\"")]
        [FromQuery(Name = "orderBy")] string? orderOptions
    )
    {
        var token = Request.Headers.Authorization.First() ?? "";

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(QueryHelpers.AddQueryString(
                uri: $"{_userServiceUrl}/api/User/GetUsers",
                queryString: new Dictionary<string, string?>
                {
                    { "usernameLike", usernameLike }, { "orderBy", orderOptions }
                }
            ))
        );
        request.Headers.Add(Constants.Authorization, token);

        using var response = await _httpClient.SendAsync(request);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }
}