using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiRobotSimulator.Core
{
    public static class Extensions
    {
        public static IEnumerable<(T, T)> Pairs<T>(this IEnumerable<T> source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1227.md
            return pairsIterator();

            IEnumerable<(T, T)> pairsIterator()
            {
                for (var i = 0; i < source.Count() - 1; i++)
                {
                    for (var j = i + 1; j < source.Count(); j++)
                    {
                        yield return (source.ElementAt(i), source.ElementAt(j));
                    }
                }
            }
        }
    }
}
