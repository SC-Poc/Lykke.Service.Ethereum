using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using JetBrains.Annotations;
using Lykke.Service.EthereumWorker.Core.IntervalTree;

namespace Lykke.Service.EthereumWorker.Core.Domain
{
    public sealed class BlockchainIndexationState : IEnumerable<BlocksIntervalIndexationState>
    {
        private readonly IntervalTree<BigInteger, bool> _stateTree;

        
        private BlockchainIndexationState()
        {
            _stateTree = new IntervalTree<BigInteger, bool>();
        }

        public static BlockchainIndexationState Create()
        {
            return new BlockchainIndexationState();
        }
        
        public static BlockchainIndexationState Restore(
            [NotNull] IEnumerable<BlocksIntervalIndexationState> indexationStates)
        {
            if (indexationStates == null)
            {
                throw new ArgumentNullException(nameof(indexationStates));
            }

            var intervalValues = indexationStates
                .OrderBy(x => x.From)
                .Select(x =>
                (
                    Interval: new Interval<BigInteger>(x.From, x.To),
                    Value: x.IsIndexed
                ))
                .ToList();

            if (intervalValues.Any() && intervalValues[0].Interval.From != 0)
            {
                throw new ArgumentException
                (
                    "Indexation state of the genesis block has not been found."
                );
            }

            for (var i = 1; i < intervalValues.Count; i++)
            {
                var a = intervalValues[i - 1];
                var b = intervalValues[i];

                if (b.Interval.From - a.Interval.To != 1)
                {
                    throw new ArgumentException
                    (
                        $"Specified intervals [{a.Interval} and {b.Interval}] are not sequential or overlap."
                    );
                }

                if (a.Value == b.Value)
                {
                    throw new ArgumentException
                    (
                        $"Specified intervals [{a.Interval} and {b.Interval}] have same indexation state [{a.Value}]."
                    );
                }
            }
            
            
            var blockchainState = new BlockchainIndexationState();
            
            blockchainState._stateTree.AddRange(intervalValues);
            
            return blockchainState;
        }

        
        public IEnumerator<BlocksIntervalIndexationState> GetEnumerator()
        {
            return _stateTree
                .Select(x => new BlocksIntervalIndexationState
                (
                    from: x.Interval.From,
                    to: x.Interval.To,
                    isIndexed: x.Value    
                ))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        [NotNull]
        public IEnumerable<BigInteger> GetNonIndexedBlockNumbers()
        {
            foreach (var intervalValue in _stateTree.Reverse())
            {
                var intervalIsIndexed = intervalValue.Value;
                
                if (!intervalIsIndexed)
                {
                    var from = intervalValue.Interval.From;
                    var to = intervalValue.Interval.To;
                
                    for (var i = to; i >= from; i--)
                    {
                        yield return i;
                    }
                }
            }
        }
        
        public bool TryToUpdateBestBlock(
            BigInteger bestBlockNumber)
        {
            if (bestBlockNumber < 0)
            {
                throw new ArgumentException
                (
                    "Should be greater or equal to zero.",
                    nameof(bestBlockNumber)
                );
            }
            
            var currentBestIntervalValue = _stateTree.LastOrDefault();
            
            BigInteger bestIntervalFrom;
            BigInteger bestIntervalTo;
            
            if (currentBestIntervalValue != null)
            {
                var currentBestInterval = currentBestIntervalValue.Interval;
                
                if (currentBestInterval.To < bestBlockNumber)
                {
                    var currentBestIntervalIndexed = currentBestIntervalValue.Value;
                
                    if (currentBestIntervalIndexed)
                    {
                        bestIntervalFrom = currentBestInterval.To + 1;
                        bestIntervalTo = bestBlockNumber;
                    }
                    else
                    {
                        bestIntervalFrom = currentBestInterval.From;
                        bestIntervalTo = bestBlockNumber;
                    
                        _stateTree.Remove(currentBestInterval);
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                bestIntervalFrom = 0;
                bestIntervalTo = bestBlockNumber;
            }
            
            _stateTree.Add
            (
                new Interval<BigInteger>(bestIntervalFrom, bestIntervalTo),
                false
            );

            return true;
        }

        public bool TryToMarkBlockAsIndexed(BigInteger blockNumber)
        {
            if (blockNumber < 0)
            {
                throw new ArgumentException
                (
                    "Should be greater or equal to zero.",
                    nameof(blockNumber)
                );
            }
            
            TryToUpdateBestBlock(blockNumber);

            
            var intervalState = _stateTree.Find(blockNumber);
            var intervalIndexed = intervalState.Value;
            
            
            if (!intervalIndexed)
            {
                MarkBlockAsIndexed(intervalState.Interval, blockNumber);

                return true;
            }
            else
            {
                return false;
            }
        }

        private void MarkBlockAsIndexed(Interval<BigInteger> interval, BigInteger blockNumber)
        {
            _stateTree.Remove(interval);

            if (blockNumber == interval.From && blockNumber == interval.To)
            {
                MarkLastBlockInIntervalAsIndexed(blockNumber);
            }
            else if (blockNumber == interval.From)
            {
                MarkLeftBlockAsIndexed(interval, blockNumber);
            }
            else if (blockNumber == interval.To)
            {
                MarkRightBlockAsIndexed(interval, blockNumber);
            }
            else
            {
                MarkMiddleBlockAsIndexed(interval, blockNumber);
            }
        }
        
        private void MarkLeftBlockAsIndexed(Interval<BigInteger> interval, BigInteger blockNumber)
        {
            var previousIntervalState = _stateTree.Find(blockNumber - 1);
            
            if (previousIntervalState != null && previousIntervalState.Value)
            {
                var previousIntervalFrom = previousIntervalState.Interval.From;
                
                _stateTree.Remove(previousIntervalState.Interval);
             
                _stateTree.AddRange
                (
                    new []
                    {
                        (new Interval<BigInteger>(previousIntervalFrom, blockNumber), true),
                        (new Interval<BigInteger>(blockNumber + 1, interval.To), false)
                    }
                );
            }
            else
            {
                _stateTree.AddRange
                (
                    new []
                    {
                        (new Interval<BigInteger>(blockNumber, blockNumber), true),
                        (new Interval<BigInteger>(blockNumber + 1, interval.To), false)
                    }
                );
            }
        }
        
        private void MarkRightBlockAsIndexed(Interval<BigInteger> interval, BigInteger blockNumber)
        {
            var nextIntervalState = _stateTree.Find(blockNumber + 1);
            
            if (nextIntervalState != null && nextIntervalState.Value)
            {
                var nextIntervalTo = nextIntervalState.Interval.To;
                
                _stateTree.Remove(nextIntervalState.Interval);
                
                _stateTree.AddRange
                (
                    new []
                    {
                        (new Interval<BigInteger>(interval.From, blockNumber - 1), false),
                        (new Interval<BigInteger>(blockNumber, nextIntervalTo), true)
                    }
                );
            }
            else
            {
                _stateTree.AddRange
                (
                    new []
                    {
                        (new Interval<BigInteger>(interval.From, blockNumber - 1), false),
                        (new Interval<BigInteger>(blockNumber, blockNumber), true)
                    }
                );
            }
        }

        private void MarkLastBlockInIntervalAsIndexed(BigInteger blockNumber)
        {
            var previousInterval = _stateTree.Find(blockNumber - 1)?.Interval;
            var nextInterval = _stateTree.Find(blockNumber + 1)?.Interval;

            var from = blockNumber;
            var to = blockNumber;
            
            if (previousInterval.HasValue)
            {
                _stateTree.Remove(previousInterval.Value);

                from = previousInterval.Value.From;
            }

            if (nextInterval.HasValue)
            {
                _stateTree.Remove(nextInterval.Value);

                to = nextInterval.Value.To;
            }
            
            _stateTree.Add(new Interval<BigInteger>(from, to), true);
        }
        
        private void MarkMiddleBlockAsIndexed(Interval<BigInteger> interval, BigInteger blockNumber)
        {
            _stateTree.AddRange
            (
                new []
                {
                    (new Interval<BigInteger>(interval.From, blockNumber - 1), false),
                    (new Interval<BigInteger>(blockNumber, blockNumber), true),
                    (new Interval<BigInteger>(blockNumber + 1, interval.To), false)
                }
            );
        }
    }
}
