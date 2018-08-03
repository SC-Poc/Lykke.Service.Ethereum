using System.Numerics;
using Lykke.AzureStorage.Tables.Entity.Metamodel;
using Lykke.AzureStorage.Tables.Entity.Metamodel.Providers;
using Lykke.Service.EthereumCommon.AzureRepositories.Serializers;
using Lykke.Service.EthereumCommon.Core;

namespace Lykke.Service.EthereumCommon.AzureRepositories
{
    public abstract class RepositoryBase
    {
        private static readonly object InitLock = new object();
        
        private static bool _initialized;
        
        
        
        protected RepositoryBase()
        {
            Initialize();
        }

        private static void Initialize()
        {
            lock (InitLock)
            {
                if (!_initialized)
                {
                    var provider = new CompositeMetamodelProvider()
                        .AddProvider
                        (
                            new AnnotationsBasedMetamodelProvider()
                        )
                        .AddProvider
                        (
                            new ConventionBasedMetamodelProvider()
                                .AddTypeSerializerRule
                                (
                                    t => t == typeof(BigInteger),
                                    s => new BigIntegerSerializer()
                                )
                        );

                    EntityMetamodel.Configure(provider);

                    _initialized = true;
                }
            }
        }
    }
}
