using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using UserProductSearchService.Dtos;
using UserProductSearchService.Dtos.Auth;
using UserProductSearchService.Helpers;

namespace UserProductSearchService.Controllers;

[Route("api/[controller]/[action]")]
[AllowAnonymous]
[ApiController]
public class AuthController : Controller
{
    private readonly string _userServiceUrl;

    private readonly HttpClient _httpClient = new();

    public AuthController(IConfiguration configuration)
    {
        var serviceUrls = configuration.GetSection("ServiceUrls");

        _userServiceUrl = serviceUrls["UserServiceUrl"]!;
    }
    
    [HttpPost]
    [SwaggerOperation(Summary = "Регистрация пользователя. Возвращает JWT-токен")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(TokenDto))]
    [ProducesResponseType(400, Type = typeof(ErrorDto))]
    public async Task<IActionResult> Register([FromBody] RegistrationDto registrationDto)
    {
        using var httpContent = new ByteArrayContent(
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(registrationDto))
        );
        httpContent.Headers.ContentType = new MediaTypeHeaderValue(Constants.ApplicationJson);

        using var response = await _httpClient.PostAsync($"{_userServiceUrl}/api/Auth/Register", httpContent);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Авторизация пользователя. Возвращает JWT-токен")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(TokenDto))]
    [ProducesResponseType(400, Type = typeof(ErrorDto))]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        using var httpContent = new ByteArrayContent(
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(loginDto))
        );
        httpContent.Headers.ContentType = new MediaTypeHeaderValue(Constants.ApplicationJson);

        using var response = await _httpClient.PostAsync($"{_userServiceUrl}/api/Auth/Login", httpContent);
        return await response.ToContentResultAsync(Constants.ApplicationJson);
    }
}