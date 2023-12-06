using ProductService.Context;
using ProductService.Models;

namespace ProductService;

public class Seed
{
    private readonly DataContext _context;

    public Seed(DataContext context)
    {
        _context = context;
    }

    public void SeedDataContext()
    {
        if (_context.Products.Any() || _context.Categories.Any())
        {
            Console.WriteLine("Data already exists in DB. No need to initialize data.");
            return;
        }

        var a = new Category { Name = "Овощи и фрукты" };
        var e = new Category { Name = "Грибы", ParentCategory = a };
        var f = new Category { Name = "Фрукты", ParentCategory = a };
        var g = new Category { Name = "Овощи", ParentCategory = a };
        var m = new Category { Name = "Квашения, соления, овощные закуски соленые", ParentCategory = g };
        var h = new Category { Name = "Ягоды", ParentCategory = a };
        var n = new Category { Name = "Свежие ягоды", ParentCategory = h };
        var b = new Category { Name = "Бакалея" };
        var i = new Category { Name = "Макароны", ParentCategory = b };
        var j = new Category { Name = "Крупы", ParentCategory = b };
        var k = new Category { Name = "Заправки, соусы", ParentCategory = b };
        var o = new Category { Name = "Растительные масла", ParentCategory = k };
        var c = new Category { Name = "Замороженая продукция" };
        var l = new Category { Name = "Полуфабрикаты", Description = "Из мяса/овощей/птицы", ParentCategory = c };
        var d = new Category { Name = "Напитки", Description = "Вода, лимонады, соки и т.д." };

        _context.Categories.AddRange(a, b, c, d, e, f, g, h, i, j, k, l, m, n, o);

        e.Products = new List<Product> { RandomProduct("Опята"), RandomProduct("Шампиньоны консервированные") };
        f.Products = new List<Product> { RandomProduct("Яблоки"), RandomProduct("Бананы") };
        m.Products = new List<Product> { RandomProduct("Квашеная капуста"), RandomProduct("Солёные огурцы") };
        i.Products = new List<Product> { RandomProduct("Макароны спирали"), RandomProduct("Спагетти") };
        j.Products = new List<Product> { RandomProduct("Гречка"), RandomProduct("Пшено") };
        o.Products = new List<Product> { RandomProduct("Масло подсолнечное"), RandomProduct("Масло оливковое") };
        l.Products = new List<Product> { RandomProduct("Блины с мясом"), RandomProduct("Наггетсы") };
        d.Products = new List<Product> { RandomProduct("Лимонад дюшес"), RandomProduct("Вода газированная") };

        _context.SaveChanges();

        Console.WriteLine("DB data initialization completed successfully!");
    }

    private static Product RandomProduct(string name)
    {
        var r = new Random();
        return new Product
        {
            Name = name,
            Price = r.Next(50, 500),
            StorageAmount = r.Next(20, 100),
            Weight = r.Next(100, 1000),
            Calories = r.Next(20, 400),
            Proteins = r.Next(5, 30),
            Fats = r.Next(5, 30),
            Carbohydrates = r.Next(5, 30)
        };
    }
}