using UserService.Context;
using UserService.Interfaces;
using UserService.Models;

namespace UserService.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly DataContext _context;

    public RoleRepository(DataContext context)
    {
        _context = context;
    }
    
    public Role? GetRoleByName(string name)
    {
        return _context.Roles.FirstOrDefault(r => r.Name == name);
    }

    public ICollection<Role> GetRoles()
    {
        return _context.Roles.ToList();
    }
}