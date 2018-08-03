namespace Lykke.Service.EthereumWorker.Core.IntervalTree
{
    public partial class IntervalTree<TKey, TValue>
    {
        private sealed class Node : IIntervalValuePair<TKey, TValue>
        {
            public int Balance { get; set; }
            
            public Interval<TKey> Interval { get; set; }
            
            public Node Left { get; set; }
        
            public Node Parent { get; set; }
            
            public Node Right { get; set; }
            
            public TValue Value { get; set; }
        }
    }
}
