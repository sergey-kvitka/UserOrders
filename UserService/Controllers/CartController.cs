using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UserService.Dtos;
using UserService.Interfaces;
using UserService.Models;

namespace UserService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CartController : Controller
{
    private readonly ICartRepository _cartRepository;
    private readonly IUserRepository _userRepository;

    private readonly IMapper _mapper;

    private readonly string _productServiceUrl;

    private readonly HttpClient _httpClient = new();

    public CartController(
        ICartRepository cartRepository,
        IUserRepository userRepository,
        IMapper mapper,
        IConfiguration configuration
    )
    {
        _cartRepository = cartRepository;
        _userRepository = userRepository;
        _mapper = mapper;

        var serviceUrls = configuration.GetSection("ServiceUrls");

        _productServiceUrl = serviceUrls["ProductServiceUrl"]!;
    }

    [HttpGet("[action]")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(CartDto))]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public IActionResult Get()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var user = username == null ? null : _userRepository.GetByUsername(username);
        if (user == null) return NoContent();

        var cart = _cartRepository.GetByUsername(user.Username);
        return cart == null
            ? NoContent()
            : Ok(_mapper.Map<CartDto>(cart));
    }

    [HttpGet("[action]/{userId}")]
    [Authorize(Roles = "ADMIN")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(CartDto))]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public IActionResult GetByUser(string userId)
    {
        Guid userGuid;
        try
        {
            userGuid = new Guid(userId);
        }
        catch (Exception)
        {
            return NoContent();
        }

        var user = _userRepository.GetById(userGuid);
        if (user == null) return NoContent();

        var cart = _cartRepository.GetByUsername(user.Username);
        return cart == null
            ? NoContent()
            : Ok(_mapper.Map<CartDto>(cart));
    }

    [HttpPut("Item/[action]")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(ErrorDto))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] Guid productId)
    {
        if (!ErrorDto.ValidateModelAndGetErrorDto(ModelState, out var errorDto)) return BadRequest(errorDto);

        var productTask = GetProduct(productId.ToString());

        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var user = username == null ? null : _userRepository.GetByUsername(username);
        if (user == null) throw new Exception("Unable to find user by JWT token");

        var cart = _cartRepository.GetByUsername(user.Username);
        if (cart == null) throw new Exception("Unable to access user's cart");

        var product = await productTask;
        if (product == null)
            return BadRequest(new ErrorDto
            {
                Error = nameof(ArgumentException),
                Message =
                    $"Product can't be added to cart: unable to find product with such id ({productId.ToString()})"
            });
        if (product.StorageAmount < 1)
            return BadRequest(new ErrorDto
            {
                Error = nameof(ArgumentException),
                Message = "Product can't be added to cart: out of stock"
            });

        var items = _cartRepository.GetItemsByCartId(cart.Id);
        if (items.Any(i => productId == i.ProductId))
            return BadRequest(new ErrorDto
            {
                Error = nameof(ArgumentException),
                Message = "Product already exists in cart"
            });

        var cartItem = new CartItem
        {
            ProductId = productId,
            Amount = 1
        };
        _cartRepository.SaveItemByCartId(cartItem, cart.Id);
        return Ok();
    }

    [HttpPut("Item/[action]")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(ErrorDto))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> ChangeAmount([FromBody] ChangeAmountDto changeAmountDto)
    {
        if (!ErrorDto.ValidateModelAndGetErrorDto(ModelState, out var errorDto)) return BadRequest(errorDto);

        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var user = username == null ? null : _userRepository.GetByUsername(username);
        if (user == null) throw new Exception("Unable to find user by JWT token");

        var cart = _cartRepository.GetByUsername(user.Username);
        if (cart == null) throw new Exception("Unable to access user's cart");

        var items = _cartRepository.GetItemsByCartId(cart.Id);
        var itemToChange = items.FirstOrDefault(i => changeAmountDto.cartItemId == i.Id);
        if (itemToChange == null)
            return BadRequest(new ErrorDto
            {
                Error = nameof(ArgumentException),
                Message = $"""Item with guid="{changeAmountDto.cartItemId}" not found in your cart"""
            });

        var product = await GetProduct(itemToChange.ProductId.ToString());
        if (product == null)
        {
            _cartRepository.DeleteItemById(itemToChange.Id);
            return BadRequest(new ErrorDto
            {
                Error = nameof(ArgumentException),
                Message = "Product specified in this cart item was not found"
            });
        }

        if (changeAmountDto.Amount + itemToChange.Amount <= 0)
        {
            _cartRepository.DeleteItemById(itemToChange.Id);
            return Ok();
        }

        if (changeAmountDto.Amount + itemToChange.Amount > product.StorageAmount)
            return BadRequest(new ErrorDto
            {
                Error = nameof(ArgumentException),
                Message = "Product amount in cart item can't be more than product storage amount"
            });

        _cartRepository.SetAmountByItemId(itemToChange.Id, changeAmountDto.Amount, doAddition: true);
        return Ok();
    }

    [HttpDelete("Item/[action]/{itemId}")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(ErrorDto))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public IActionResult Delete(string itemId)
    {
        if (!ErrorDto.ValidateModelAndGetErrorDto(ModelState, out var errorDto)) return BadRequest(errorDto);

        Guid itemGuid;
        try
        {
            itemGuid = new Guid(itemId);
        }
        catch (Exception)
        {
            return BadRequest(new ErrorDto
            {
                Error = nameof(ArgumentException),
                Message = $"""Item with guid="{itemId}" not found in your cart""",
            });
        }

        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var user = username == null ? null : _userRepository.GetByUsername(username);
        if (user == null) throw new Exception("Unable to find user by JWT token");

        var cart = _cartRepository.GetByUsername(user.Username);
        if (cart == null) throw new Exception("Unable to access user's cart");

        var items = _cartRepository.GetItemsByCartId(cart.Id);
        if (items.All(i => itemGuid != i.Id))
            return BadRequest(new ErrorDto
            {
                Error = nameof(ArgumentException),
                Message = $"""Item with guid="{itemId}" not found in your cart""",
            });

        _cartRepository.DeleteItemById(itemGuid);
        return Ok();
    }

    [HttpDelete("[action]")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public IActionResult Clear()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var user = username == null ? null : _userRepository.GetByUsername(username);
        if (user == null) throw new Exception("Unable to find user by JWT token");

        var cart = _cartRepository.GetByUsername(user.Username);
        if (cart == null) throw new Exception("Unable to access user's cart");

        _cartRepository.ClearById(cart.Id);
        return Ok();
    }

    private async Task<ProductDto?> GetProduct(string productId)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"{_productServiceUrl}/api/Product/GetById/{productId}"
        );

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        return !response.IsSuccessStatusCode
            ? null
            : JsonConvert.DeserializeObject<ProductDto>(json);
    }
}