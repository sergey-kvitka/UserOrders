namespace UserService.Dtos;

public class LoginDto
{
    public LoginDto(RegistrationDto registrationDto)
    {
        Username = registrationDto.Username;
        Password = registrationDto.Password;
    }

    public LoginDto()
    {
    }

    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}