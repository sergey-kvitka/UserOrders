using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UserService.Dtos;
using UserService.Interfaces;
using UserService.Models;

namespace UserService.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class OrderController : Controller
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICartRepository _cartRepository;

    private readonly IMapper _mapper;

    private readonly string _productServiceUrl;

    private readonly HttpClient _httpClient = new();

    public OrderController(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        ICartRepository cartRepository,
        IMapper mapper,
        IConfiguration configuration
    )
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _cartRepository = cartRepository;
        _mapper = mapper;

        var serviceUrls = configuration.GetSection("ServiceUrls");

        _productServiceUrl = serviceUrls["ProductServiceUrl"]!;
    }

    [HttpPut]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(ErrorDto))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Make()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var user = username == null ? null : _userRepository.GetByUsername(username);
        if (user == null) throw new Exception("Unable to find user by JWT token");

        var cart = _cartRepository.GetByUsername(user.Username);
        if (cart == null) throw new Exception("Unable to access user's cart");

        var cartItems = _cartRepository.GetItemsByCartId(cart.Id);
        if (!cartItems.Any())
            return BadRequest(new ErrorDto
            {
                Error = nameof(ArgumentException),
                Message = "Unable to make the order: the cart is empty"
            });

        var httpContent = new ByteArrayContent(
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_mapper.Map<List<CartItemDto>>(cartItems)))
        );
        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await _httpClient.PostAsync($"{_productServiceUrl}/api/Product/MakeOrder", httpContent);
        var json = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var finalOrderItems = JsonConvert.DeserializeObject<ICollection<FinalOrderItemDto>>(json);
            if (finalOrderItems == null)
                return BadRequest(new ErrorDto
                {
                    Error = nameof(ArgumentException),
                    Message = "Unable to make the order"
                });

            decimal sum = 0;
            var productsAmount = 0;
            foreach (var orderItem in finalOrderItems)
            {
                sum += orderItem.TotalItemPrice;
                productsAmount += orderItem.Amount;
            }

            var order = new Order
            {
                Date = DateTime.Now,
                Sum = sum,
                ProductsAmount = productsAmount,
                Products = finalOrderItems.ToList()
            };
            _orderRepository.MakeOrder(order, user.Id);
            return Ok();
        }

        ErrorDto? errorDto = null;
        try
        {
            errorDto = JsonConvert.DeserializeObject<ErrorDto>(json);
        }
        finally
        {
            errorDto ??= new ErrorDto
            {
                Error = nameof(ArgumentException),
                Message = "Unable to make the order"
            };
        }

        return BadRequest(errorDto);
    }

    [HttpGet]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(ICollection<OrderDto>))]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public IActionResult GetOrders([FromQuery(Name = "orderBy")] string? orderOptions)
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var user = username == null ? null : _userRepository.GetByUsername(username);
        if (user == null) throw new Exception("Unable to find user by JWT token");

        var orders = _orderRepository.GetByUserId(user.Id, orderOptions);
        if (!orders.Any()) return NoContent();

        return Ok(_mapper.Map<List<OrderDto>>(orders));
    }

    [HttpGet("{userId}")]
    [Authorize(Roles = "ADMIN")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(ICollection<OrderDto>))]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public IActionResult GetByUser(
        string userId,
        [FromQuery(Name = "orderBy")] string? orderOptions
    )
    {
        Guid userGuid;
        try
        {
            userGuid = new Guid(userId);
        }
        catch
        {
            return NoContent();
        }

        var user = _userRepository.GetById(userGuid);
        if (user == null) return NoContent();

        var orders = _orderRepository.GetByUserId(user.Id, orderOptions);
        if (!orders.Any()) return NoContent();

        return Ok(_mapper.Map<List<OrderDto>>(orders));
    }

    [HttpGet("{orderId}")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(OrderDto))]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public IActionResult GetById(string orderId)
    {
        Guid orderGuid;
        try
        {
            orderGuid = new Guid(orderId);
        }
        catch
        {
            return NoContent();
        }

        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var user = username == null ? null : _userRepository.GetByUsername(username);
        if (user == null) throw new Exception("Unable to find user by JWT token");

        var order = _orderRepository
            .GetByUserId(user.Id, null)
            .FirstOrDefault(o => orderGuid == o.Id);

        return order == null
            ? NoContent()
            : Ok(_mapper.Map<OrderDto>(order));
    }
}