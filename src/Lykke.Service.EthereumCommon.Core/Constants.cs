namespace Lykke.Service.EthereumCommon.Core
{
    public static class Constants
    {
        public const int AssetAccuracy = 18;
        
        public const string AssetId = "ETH";
        
        #if DEBUG
        
        public const string BuildConfigurationMessage = "Is DEBUG";

        public const bool IsDebug = true;

        #else

        public const string BuildConfigurationMessage = "Is Release";
    
        public const bool IsDebug = false;
    
        #endif
    }
}
