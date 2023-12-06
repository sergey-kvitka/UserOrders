namespace ProductService.Helpers;

public static class Extensions
{
    public static IEnumerable<T> Flatten<T>(
        this IEnumerable<T> e,
        Func<T, IEnumerable<T>> func
    )
    {
        var enumerable = e.ToList();
        return enumerable.SelectMany(c => func(c).Flatten(func)).Concat(enumerable);
    }
}