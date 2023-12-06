using Microsoft.EntityFrameworkCore;
using UserService.Context;
using UserService.Interfaces;
using UserService.Models;

namespace UserService.Repositories;

public class UserRepository : IUserRepository
{
    private static readonly List<string> OrderOptions = new() { "asc", "desc" };

    private readonly DataContext _context;
    private readonly IRoleRepository _roleRepository;

    public UserRepository(DataContext context, IRoleRepository roleRepository)
    {
        _context = context;
        _roleRepository = roleRepository;
    }

    public ICollection<User> GetUsers(string? usernameLike, string? orderOptions)
    {
        var users = _context.Users
            .Include(u => u.Roles)
            .Where(u => usernameLike == null || u.Username.ToLower().Contains(usernameLike.Trim().ToLower()))
            .OrderBy(u => u.Username)
            .ToList();

        return OrderByFunctionFromString(users, orderOptions);
    }

    public ICollection<User> GetUsersByRole(Role role)
    {
        return _context.Users.Where(u => u.Roles.Contains(role)).ToList();
    }

    public User? GetByUsername(string username)
    {
        return _context.Users
            .Include(u => u.Roles)
            .FirstOrDefault(u => u.Username == username);
    }

    public User? GetById(Guid userId)
    {
        return _context.Users
            .Include(u => u.Roles)
            .FirstOrDefault(u => u.Id == userId);
    }

    public void RegisterUser(User user)
    {
        if (!user.Roles.Any())
        {
            var customerRole = _roleRepository.GetRoleByName(Role.DefaultCustomerRole);
            if (customerRole != null) user.Roles.Add(customerRole);
        }

        _context.Users.Add(user);
        _context.SaveChanges();
    }

    private static ICollection<User> OrderByFunctionFromString(ICollection<User> users, string? orderOptions)
    {
        if (orderOptions == null || users.Count < 2)
        {
            return users;
        }

        var actions = orderOptions.Trim().Split(" ")
            .Select(s => s.Trim().Split("."))
            .Where(p => p.Length == 2 && OrderOptions.Contains(p.Last().ToLower()));

        var orderedUsers = users.OrderBy(u => u.Username);
        var wasOrdered = false;

        foreach (var action in actions)
        {
            var property = action.First();

            var selection = SelectionFunctionByPropName(property);
            if (selection == null) continue;

            var option = action.Last();
            orderedUsers = option == OrderOptions.First()
                ? wasOrdered
                    ? orderedUsers.ThenBy(u => selection(u))
                    : orderedUsers.OrderBy(u => selection(u))
                : wasOrdered
                    ? orderedUsers.ThenByDescending(u => selection(u))
                    : orderedUsers.OrderByDescending(u => selection(u));
            wasOrdered = true;
        }

        return orderedUsers.ToList();
    }

    private static Func<User, dynamic?>? SelectionFunctionByPropName(string property)
    {
        return property.ToLower() switch
        {
            "id" => u => u.Id,
            "username" => u => u.Username,
            "firstname" => u => u.FirstName,
            "lastname" => u => u.LastName,
            "middlename" => u => u.MiddleName,
            "email" => u => u.Email,
            "phone" => u => u.Phone,
            _ => null
        };
    }
}