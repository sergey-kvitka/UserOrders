using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProductService.Dtos;
using ProductService.Interfaces;

namespace ProductService.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ProductController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ProductController(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(ICollection<ProductDto>))]
    [ProducesResponseType(204)]
    public IActionResult All(
        [FromQuery(Name = "nameLike")] string? nameLike,
        [FromQuery(Name = "orderBy")] string? orderOptions
    )
    {
        var products = _productRepository.GetAllProducts(nameLike, orderOptions);
        return products.Count == 0
            ? NoContent()
            : Ok(_mapper.Map<List<ProductDto>>(products));
    }

    [HttpGet("{categoryId}")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(ICollection<ProductDto>))]
    [ProducesResponseType(204)]
    public IActionResult GetByCategory(
        string categoryId,
        [FromQuery(Name = "nameLike")] string? nameLike,
        [FromQuery(Name = "orderBy")] string? orderOptions
    )
    {
        Guid categoryGuid;
        try
        {
            categoryGuid = new Guid(categoryId);
        }
        catch (Exception)
        {
            return NoContent();
        }

        var products = _productRepository.GetByCategoryId(categoryGuid, nameLike, orderOptions);
        return products.Count == 0
            ? NoContent()
            : Ok(_mapper.Map<List<ProductDto>>(products));
    }

    [HttpGet("{productId}")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(ProductDto))]
    [ProducesResponseType(204)]
    public IActionResult GetById(string productId)
    {
        Guid productGuid;
        try
        {
            productGuid = new Guid(productId);
        }
        catch (Exception)
        {
            return NoContent();
        }

        var product = _productRepository.GetById(productGuid);
        return product == null
            ? NoContent()
            : Ok(_mapper.Map<ProductDto>(product));
    }

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(ICollection<FinalOrderItemDto>))]
    [ProducesResponseType(400, Type = typeof(ErrorDto))]
    public IActionResult MakeOrder([FromBody] ICollection<OrderItemDto> orderItemDtos)
    {   
        if (!ErrorDto.ValidateModelAndGetErrorDto(ModelState, out var errorDto))
            return BadRequest(errorDto);

        ICollection<FinalOrderItemDto> finalOrderItems;
        try
        {
            finalOrderItems = _productRepository.MakeOrder(orderItemDtos);
        }
        catch (Exception e)
        {
            return BadRequest(new ErrorDto
            {
                Error = e.GetType().Name,
                Message = e.Message
            });
        }

        return Ok(finalOrderItems);
    }
}