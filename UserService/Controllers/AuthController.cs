using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserService.Dtos;
using UserService.Interfaces;
using UserService.Models;
using static BCrypt.Net.BCrypt;

namespace UserService.Controllers;

[Route("api/[controller]/[action]")]
[AllowAnonymous]
[ApiController]
public class AuthController : Controller
{
    private readonly IUserRepository _userRepository;

    private readonly IConfiguration _configuration;

    public AuthController(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(TokenDto))]
    [ProducesResponseType(400, Type = typeof(ErrorDto))]
    public IActionResult Register([FromBody] RegistrationDto registrationDto)
    {
        if (!ErrorDto.ValidateModelAndGetErrorDto(ModelState, out var errorDto)) return BadRequest(errorDto);

        try
        {
            _userRepository.RegisterUser(new User
            {
                Username = registrationDto.Username,
                FirstName = registrationDto.FirstName,
                LastName = registrationDto.LastName,
                MiddleName = registrationDto.MiddleName,
                Password = HashPassword(registrationDto.Password),
                Email = registrationDto.Email,
                Phone = registrationDto.Phone
            });
        }
        catch (DbUpdateException e)
        {
            var exception = HandleDbUpdateException(e);
            return BadRequest(new ErrorDto
            {
                Error = exception.GetType().Name,
                Message = exception.Message
            });
        }
        catch (Exception e)
        {
            return BadRequest(new ErrorDto
            {
                Error = e.GetType().Name,
                Message = e.Message
            });
        }

        return LoginAction(new LoginDto(registrationDto));
    }

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(200, Type = typeof(TokenDto))]
    [ProducesResponseType(400, Type = typeof(ErrorDto))]
    public IActionResult Login([FromBody] LoginDto loginDto)
    {
        return ErrorDto.ValidateModelAndGetErrorDto(ModelState, out var errorDto)
            ? LoginAction(loginDto)
            : BadRequest(errorDto);
    }

    private IActionResult LoginAction(LoginDto loginDto)
    {
        var user = _userRepository.GetByUsername(loginDto.Username);

        var wrongNameOrPass = BadRequest(new ErrorDto
            { Error = "ArgumentException", Message = "Wrong username or password" });

        if (user == null)
            return wrongNameOrPass;
        if (!Verify(loginDto.Password, user.Password))
            return wrongNameOrPass;

        var username = user.Username;
        var claims = user.Roles
            .Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role.Name))
            .Concat(new[] { new Claim(ClaimsIdentity.DefaultNameClaimType, username) })
            .ToList();

        if (!claims.Any()) claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, "CUSTOMER"));

        var claimsIdentity = new ClaimsIdentity(
            claims, "Token",
            ClaimsIdentity.DefaultNameClaimType,
            ClaimsIdentity.DefaultRoleClaimType
        );

        var now = DateTime.UtcNow;
        var jwt = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            notBefore: now,
            claims: claimsIdentity.Claims,
            expires: now.Add(TimeSpan.FromMinutes(double.Parse(_configuration["JwtSettings:Lifetime"]!))),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]!)),
                SecurityAlgorithms.HmacSha256
            )
        );

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt)!;

        return Ok(new TokenDto
        {
            Username = username,
            Token = encodedJwt
        });
    }

    private static Exception HandleDbUpdateException(DbUpdateException dbu)
    {
        var builder = new StringBuilder("A DbUpdateException was caught while saving changes. ");

        try
        {
            foreach (var result in dbu.Entries)
            {
                builder.Append($"Type: {result.Entity.GetType().Name} was part of the problem. ");
            }
        }
        catch (Exception e)
        {
            builder.Append("Error parsing DbUpdateException: " + e);
        }

        var message = builder.ToString();
        return new Exception(message, dbu);
    }
}