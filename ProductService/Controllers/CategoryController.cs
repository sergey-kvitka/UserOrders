using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProductService.Dtos;
using ProductService.Interfaces;

namespace ProductService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : Controller
{
    private readonly ICategoryRepository _categoryRepository;

    private readonly IMapper _mapper;

    public CategoryController(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    [HttpGet("{categoryId}")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(CategoryDto))]
    [ProducesResponseType(204)]
    public IActionResult GetCategoryById(
        string categoryId,
        [FromQuery(Name = "nested")] bool? nested
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

        var category = _categoryRepository.GetById(categoryGuid, nested ?? false);
        return category == null
            ? NoContent()
            : Ok(_mapper.Map<CategoryDto>(category));
    }

    [HttpGet("[action]")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(ICollection<CategoryDto>))]
    [ProducesResponseType(204)]
    public IActionResult All([FromQuery(Name = "nested")] bool? nested)
    {
        var categories = _categoryRepository.GetAllCategories(nested ?? false);
        return categories.Any()
            ? Ok(_mapper.Map<List<CategoryDto>>(categories))
            : NoContent();
    }
}