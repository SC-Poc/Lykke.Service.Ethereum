using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FluentAssertions;
using Lykke.Service.EthereumWorker.Core.Domain;
using MessagePack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lykke.Service.EthereumWorker.Tests.Core.Domain
{
    [TestClass]
    public class BlockchainIndexationStateTests
    {
        [TestMethod]
        public void Create()
        {
            var state = BlockchainIndexationState.Create();

            state
                .Should().BeEmpty();
        }
        
        [TestMethod]
        public void Restore()
        {
            var intervals = new[]
            {
                new BlocksIntervalIndexationState(0, 5, true),
                new BlocksIntervalIndexationState(6, 9, false)
            };
            
            var state = BlockchainIndexationState.Restore(intervals);

            // ReSharper disable once CoVariantArrayConversion
            state
                .Should().BeEquivalentTo(intervals);
        }
        
        [TestMethod]
        public void Restore__Empty_List_Passed__Empty_State_Restored()
        {
            var state = BlockchainIndexationState.Restore(Enumerable.Empty<BlocksIntervalIndexationState>());

            state
                .Should().BeEmpty();
        }

        [TestMethod]
        public void Restore__Null_Passed__ArgumentNullException_Thrown()
        {
            Action restore = () =>
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                BlockchainIndexationState.Restore(null);
            };

            restore
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Restore__No_Genesis_Block_Passed__ArgumentException_Thrown()
        {
            Action restore = () =>
            {
                BlockchainIndexationState.Restore(new []
                {
                    new BlocksIntervalIndexationState(1, 2, false), 
                });
            };

            restore
                .Should().Throw<ArgumentException>()
                    .WithMessage("Indexation state of the genesis block has not been found.");
        }
        
        [TestMethod]
        public void Restore__Overlapping_Intervals_Passed__ArgumentException_Thrown()
        {
            Action restore = () =>
            {
                BlockchainIndexationState.Restore(new []
                {
                    new BlocksIntervalIndexationState(0, 5, false),
                    new BlocksIntervalIndexationState(5, 9, true)
                });
            };

            restore
                .Should().Throw<ArgumentException>()
                    .WithMessage($"Specified intervals [0..5 and 5..9] are not sequential or overlap.");
        }
        
        [TestMethod]
        public void Restore__Non_Sequential_Intervals_Passed__ArgumentException_Thrown()
        {
            Action restore = () =>
            {
                BlockchainIndexationState.Restore(new []
                {
                    new BlocksIntervalIndexationState(0, 4, false),
                    new BlocksIntervalIndexationState(6, 9, true)
                });
            };

            restore
                .Should().Throw<ArgumentException>()
                    .WithMessage("Specified intervals [0..4 and 6..9] are not sequential or overlap.");
        }
        
        [TestMethod]
        public void Restore__Non_Merged_Intervals_Passed__ArgumentException_Thrown()
        {
            Action restore = () =>
            {
                BlockchainIndexationState.Restore(new []
                {
                    new BlocksIntervalIndexationState(0, 5, true),
                    new BlocksIntervalIndexationState(6, 9, true)
                });
            };

            restore
                .Should().Throw<ArgumentException>()
                    .WithMessage("Specified intervals [0..5 and 6..9] have same indexation state [true].");
        }

        [TestMethod]
        public void GetNonIndexedBlockNumbers()
        {
            var state = BlockchainIndexationState.Restore(new[]
            {
                new BlocksIntervalIndexationState(0, 2, false),
                new BlocksIntervalIndexationState(3, 6, true),
                new BlocksIntervalIndexationState(7, 9, false)
            });

            state.GetNonIndexedBlockNumbers()
                .Should().BeEquivalentTo(new BigInteger[] { 9, 8, 7, 2, 1, 0 });
        }
        
        [TestMethod]
        public void TryToUpdateBestBlock__Lower_Then_Zero_Block_Numer_Provided__ArgumentExceptionThrown()
        {
            var state = BlockchainIndexationState.Create();

            state.Invoking(x => x.TryToUpdateBestBlock(-1))
                .Should().Throw<ArgumentException>();
        }
        
        [TestMethod]
        public void TryToUpdateBestBlock__Non_Best_Block_Provided()
        {
            var state = BlockchainIndexationState.Restore(new[]
            {
                new BlocksIntervalIndexationState(0, 5, true)
            });

            state.TryToUpdateBestBlock(3)
                .Should().BeFalse();

            state.Last().To
                .Should().Be(5);
        }
        
        [TestMethod]
        public void TryToUpdateBestBlock__New_Best_Block_Provided()
        {
            var state = BlockchainIndexationState.Restore(new[]
            {
                new BlocksIntervalIndexationState(0, 5, true)
            });

            state.TryToUpdateBestBlock(7)
                .Should().BeTrue();

            state.Last().To
                .Should().Be(7);
        }
        
        [TestMethod]
        public void TryToUpdateBestBlock__State_Is_Empty()
        {
            var state = BlockchainIndexationState.Create();

            state.TryToUpdateBestBlock(1)
                .Should().BeTrue();

            state.Single().To
                .Should().Be(1);
        }

        [TestMethod]
        public void TryToMarkBlockAsIndexed()
        {
            var state = BlockchainIndexationState.Create();

            state.TryToUpdateBestBlock(99);

            state.TryToMarkBlockAsIndexed(42);

            state
                .Should().BeEquivalentTo
                (
                    new BlocksIntervalIndexationState(0,  41, false),
                    new BlocksIntervalIndexationState(42, 42, true),
                    new BlocksIntervalIndexationState(43, 99, false)
                );
            
            state.TryToMarkBlockAsIndexed(43);
            
            state
                .Should().BeEquivalentTo
                (
                    new BlocksIntervalIndexationState(0,  41,  false),
                    new BlocksIntervalIndexationState(42, 43, true),
                    new BlocksIntervalIndexationState(44, 99, false)
                );
            
            state.TryToMarkBlockAsIndexed(41);
            
            state
                .Should().BeEquivalentTo
                (
                    new BlocksIntervalIndexationState(0, 40,  false),
                    new BlocksIntervalIndexationState(41, 43, true),
                    new BlocksIntervalIndexationState(44, 99, false)
                );
            
            state.TryToMarkBlockAsIndexed(0);
            
            state
                .Should().BeEquivalentTo
                (
                    new BlocksIntervalIndexationState(0,  0,  true),
                    new BlocksIntervalIndexationState(1,  40, false),
                    new BlocksIntervalIndexationState(41, 43, true),
                    new BlocksIntervalIndexationState(44, 99, false)
                );
            
            state.TryToMarkBlockAsIndexed(99);
            
            state
                .Should().BeEquivalentTo
                (
                    new BlocksIntervalIndexationState(0,  0,  true),
                    new BlocksIntervalIndexationState(1,  40, false),
                    new BlocksIntervalIndexationState(41, 43, true),
                    new BlocksIntervalIndexationState(44, 98, false),
                    new BlocksIntervalIndexationState(99, 99, true)
                );

            for (var i = 0; i <= 99; i++)
            {
                if (i >= 1 && i <= 40 || i >= 44 && i <= 98)
                {
                    state.TryToMarkBlockAsIndexed(i);
                }
            }

            state
                .Should().BeEquivalentTo
                (
                    new BlocksIntervalIndexationState(0, 99, true)
                );
        }
        
        [TestMethod]
        public void TryToMarkBlockAsIndexed__Lower_Then_Zero_Block_Numer_Provided__ArgumentExceptionThrown()
        {
            var state = BlockchainIndexationState.Create();

            state.Invoking(x => x.TryToMarkBlockAsIndexed(-1))
                .Should().Throw<ArgumentException>();
        }
        
        [TestMethod]
        public void TryToMarkBlockAsIndexed__New_Best_Block_Provided__Best_Block_Updated()
        {
            var state = BlockchainIndexationState.Create();

            state.TryToMarkBlockAsIndexed(5);

            state.Last().To
                .Should().Be(5);
        }

        [TestMethod]
        public void Serialize_And_Deserialize_With_MessagePack()
        {
            var state = BlockchainIndexationState.Restore(new[]
            {
                new BlocksIntervalIndexationState(0, 2, false),
                new BlocksIntervalIndexationState(3, 6, true),
                new BlocksIntervalIndexationState(7, 9, false)
            });
            
            MessagePackSerializer
                .Deserialize<IEnumerable<BlocksIntervalIndexationState>>
                    (MessagePackSerializer.Serialize((IEnumerable<BlocksIntervalIndexationState>) state))
                .Should().BeEquivalentTo(state);
        }
    }
}
