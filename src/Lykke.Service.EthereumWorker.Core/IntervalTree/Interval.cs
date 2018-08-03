using System;
using JetBrains.Annotations;

namespace Lykke.Service.EthereumWorker.Core.IntervalTree
{
    [PublicAPI]
    public struct Interval<T> : IComparable<Interval<T>>, IComparable<T>
        where T : struct, IComparable<T>
    {
        public Interval(T from, T to)
        {
            if (from.CompareTo(to) > 0)
            {
                throw new ArgumentException($"{nameof(from)} should be lower or equal to {nameof(to)}.");
            }
                
            From = from;
            To = to;
        }
            
            
        public T From { get; }
            
        public T To { get; }
            
        
        public int CompareTo(T other)
        {
            if (To.CompareTo(other) < 0)
            {
                return -1;
            }

            if (From.CompareTo(other) > 0)
            {
                return 1;
            }
            
            return 0;
        }
        
        public int CompareTo(Interval<T> other)
        {
            if (From.CompareTo(other.From) < 0)
            {
                return -1;
            }
            
            if (From.CompareTo(other.From) > 0)
            {
                return 1;
            }
            
            if (To.CompareTo(other.To) < 0)
            {
                return 1;
            }
            
            if (To.CompareTo(other.To) > 0)
            {
                return -1;
            }
            
            return 0;
        }
            
        public bool Contains(T point)
        {
            return From.CompareTo(point) <= 0
                && To.CompareTo(point)   >= 0;
        }
            
        public bool Contains(Interval<T> other)
        {
            return From.CompareTo(other.From) <= 0
                && To.CompareTo(other.To)     >= 0;
        }
            
        public bool Overlaps(Interval<T> other)
        {
            return From.CompareTo(other.To) <= 0 
                && To.CompareTo(other.From) >= 0;
        }

        public override string ToString()
        {
            return $"{From}..{To}";
        }
    }
}
