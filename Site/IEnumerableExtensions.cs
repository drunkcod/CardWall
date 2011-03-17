using System.Collections.Generic;
using System.Linq;

namespace CardWall
{
    static class IEnumerableExtensions
    {
        public static IEnumerable<int> Scale(this IEnumerable<int> source, int sourceOffset, int sourceRange, int targetOffset, int targetRange) {
            return source.Select(x => targetRange * x / sourceRange + targetOffset - sourceOffset); 
        }
    }
}