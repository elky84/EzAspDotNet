using System;
using System.Collections.Generic;
using System.Text;

namespace EzAspDotNet.Util
{
    public static class CollectionUtil
    {
        public static HashSet<T> ToHashSet<T>(
            this IEnumerable<T> source,
            IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(source, comparer);
        }
    }
}
