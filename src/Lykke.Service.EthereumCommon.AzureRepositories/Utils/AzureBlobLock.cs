using System;
using System.Threading.Tasks;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Lykke.Service.EthereumCommon.AzureRepositories.Utils
{
    public class AzureBlobLock : IDistributedLock
    {
        private readonly CloudBlobContainer _container;
        private readonly string _key;
        private readonly int _lockDuration;


        private CloudBlockBlob _lockBlob;
        
        
        private AzureBlobLock(
            string connectionString,
            string container,
            string key,
            int lockDuration = 15)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            
            _container = blobClient.GetContainerReference(container);
            _key = key;
            _lockDuration = lockDuration;
        }
        
        public static IDistributedLock Create(
            IReloadingManager<string> connectionStringManager, 
            string container,
            string key,
            int lockDuration = 15)
        {
            return new ReloadingConnectionStringOnFailureAzureLockDecorator
            (
                async reload => await CreateAsync(connectionStringManager, reload, container, key, lockDuration)
            );
        }

        private static async Task<IDistributedLock> CreateAsync(
            IReloadingManager<string> connectionStringManager,
            bool reloadConnectionString,
            string container,
            string key,
            int lockDuration)
        {
            var @lock = new AzureBlobLock
            (
                reloadConnectionString ? await connectionStringManager.Reload() : connectionStringManager.CurrentValue,
                container,
                key,
                lockDuration
            );

            await @lock.InitializeAsync();

            return @lock;
        }

        private async Task InitializeAsync()
        {
            await _container.CreateIfNotExistsAsync();

            _lockBlob = _container.GetBlockBlobReference(_key);

            if (!await _lockBlob.ExistsAsync())
            {
                await _lockBlob.UploadTextAsync(string.Empty);
            }
        }
        
        public async Task<IDisposable> WaitAsync()
        {
            while (true)
            {
                try
                {
                    var leaseId = await _lockBlob.AcquireLeaseAsync
                    (
                        _lockDuration != -1 ? TimeSpan.FromSeconds(_lockDuration) : default(TimeSpan?)
                    );

                    return new LockReleaser(_lockBlob, leaseId);
                }
                catch (StorageException e) when (e.RequestInformation.HttpStatusCode == StatusCodes.Status409Conflict)
                {
                    await Task.Delay(1000);
                }
            }
        }
        
        private class LockReleaser : IDisposable
        {
            private readonly string _leaseId;
            private readonly CloudBlockBlob _lockBlob;
            
            
            public LockReleaser(
                CloudBlockBlob lockBlob,
                string leaseId)
            {
                _leaseId = leaseId;
                _lockBlob = lockBlob;
            }
            
            
            public void Dispose()
            {
                _lockBlob.ReleaseLeaseAsync(new AccessCondition
                {
                    LeaseId = _leaseId
                }).Wait();
            }
        }
    }
}
