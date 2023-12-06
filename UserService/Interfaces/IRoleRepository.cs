using UserService.Models;

namespace UserService.Interfaces;

public interface IRoleRepository
{
    public Role? GetRoleByName(string name);

    public ICollection<Role> GetRoles();
}