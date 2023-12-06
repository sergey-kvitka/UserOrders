using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Dtos;
using UserService.Interfaces;

namespace UserService.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class UserController : Controller
{
    private readonly IUserRepository _userRepository;

    private readonly IMapper _mapper;

    public UserController(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(ICollection<UserProfileDto>))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public IActionResult GetUsers(
        [FromQuery(Name = "usernameLike")] string? usernameLike,
        [FromQuery(Name = "orderBy")] string? orderOptions
    )
    {
        return Ok(_mapper.Map<List<UserProfileDto>>(_userRepository.GetUsers(usernameLike, orderOptions)));
    }

    [HttpGet]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(UserProfileDto))]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public IActionResult Profile()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;

        var user = username == null ? null : _userRepository.GetByUsername(username);
        if (user == null) return NoContent();

        return Ok(_mapper.Map<UserProfileDto>(user));
    }
}