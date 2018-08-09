namespace Lykke.Service.EthereumCommon.Core
{
    public static class Constants
    {
        public const int AssetAccuracy = 18;
        
        #if ETC

        public const string AssetId = "ETC";
    
        public const string BlockchainId = "EthereumClassic";
    
        public const string BlockchainName = "Ethereum Classic";

        #else
        
        public const string AssetId = "ETH";
        
        public const string BlockchainId = "Ethereum";
        
        public const string BlockchainName = "Ethereum";
        
        #endif
        
        
        
        #if DEBUG
        
        public const string BuildConfigurationMessage = "Is DEBUG";

        public const bool IsDebug = true;

        #else

        public const string BuildConfigurationMessage = "Is RELEASE";
    
        public const bool IsDebug = false;
    
        #endif
    }
}
