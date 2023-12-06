using UserService.Models;

namespace UserService.Interfaces;

public interface IUserRepository
{
    ICollection<User> GetUsers(string? usernameLike, string? orderOptions);

    User? GetByUsername(string username);
    
    User? GetById(Guid userId);
    
    void RegisterUser(User user);
}