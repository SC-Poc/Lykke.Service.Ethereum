using System;

namespace Lykke.Service.EthereumCommon.Core.Telemetry
{
    public interface IEventTracker : IDisposable
    {
        void TrackFailure(
            string failedEventName);
        
        void SetMetric(
            string name,
            double value);

        void SetProperty(
            string name,
            string value);
        
        void SetProperty(
            string name,
            object value);
    }
}
