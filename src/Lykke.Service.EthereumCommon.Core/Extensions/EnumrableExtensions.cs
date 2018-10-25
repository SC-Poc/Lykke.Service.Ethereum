using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Lykke.Service.EthereumCommon.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static ImmutableArray<TResult> SelectImmutableArray<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector)
        {
            return source
                .Select(selector)
                .ToImmutableArray();
        }
    }
}
