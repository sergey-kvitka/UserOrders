using UserService.Context;
using UserService.Models;
using static BCrypt.Net.BCrypt;

namespace UserService;

public class Seed
{
    private readonly DataContext _context;

    public Seed(DataContext context)
    {
        _context = context;
    }

    public void SeedDataContext()
    {
        if (_context.Users.Any() || _context.Roles.Any())
        {
            Console.WriteLine("Data already exists in DB. No need to initialize data.");
            return;
        }

        var customer = new Role { Name = Role.DefaultCustomerRole };
        var admin = new Role { Name = Role.DefaultAdminRole };

        _context.Roles.AddRange(customer, admin);

        var s = new User
        {
            Username = "sergey",
            FirstName = "Квитка",
            LastName = "Сергей",
            MiddleName = "Алексеевич",
            Password = HashPassword("helloworld123"),
            Email = "sergey_kvitka75@mail.ru",
            Phone = "+79997775511",
            Roles = new List<Role> { customer, admin }
        };
        var a = new User
        {
            Username = "andrew",
            FirstName = "Андреев",
            LastName = "Андрей",
            MiddleName = "Андреевич",
            Password = HashPassword("helloworld123"),
            Email = "andrew@mail.ru",
            Phone = "+78889995588",
            Roles = new List<Role> { customer }
        };
        var i = new User
        {
            Username = "ivan",
            FirstName = "Иванов",
            LastName = "Иван",
            MiddleName = "Иванович",
            Password = HashPassword("helloworld123"),
            Email = "ivan@mail.ru",
            Phone = "+79997775566",
            Roles = new List<Role> { customer }
        };

        _context.Users.AddRange(s, a, i);

        _context.SaveChanges();
        
        Console.WriteLine("DB data initialization completed successfully!");
    }
}