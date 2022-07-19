using System.Collections.Generic;
using System.Linq;

namespace Betreten_Verboten.Components.Base
{
    public static class LINQExtensions
    {
        public static T Random<T>(this IEnumerable<T> element) where T : class => element.ElementAt(Nez.Random.Range(0, element.Count()));
    }
}
