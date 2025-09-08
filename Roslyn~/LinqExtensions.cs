using System.Collections.Generic;
using System.Linq;

namespace Ksi.Roslyn;

public static class LinqExtensions
{
    public static IEnumerable<T> SelectNonNull<T>(this IEnumerable<T?> source) where T : struct
    {
        return source.Where(x => x.HasValue).Select(x => x!.Value);
    }
}