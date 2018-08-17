using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Lykke.Service.EthereumCommon.Core.Telemetry
{
    public class EventTracker : IEventTracker
    {
        private readonly EventTelemetry _eventTelemetry;
        private readonly Stopwatch _stopwatch;
        private readonly TelemetryClient _telemetryClient;
        
        public EventTracker(
            string eventName,
            TelemetryClient telemetryClient)
        {
            _eventTelemetry = new EventTelemetry(eventName);
            _stopwatch = Stopwatch.StartNew();
            _telemetryClient = telemetryClient;
        }
        
        public void Dispose()
        {
            _stopwatch.Stop();

            SetMetric("duration", _stopwatch.ElapsedMilliseconds);
            
            _telemetryClient.TrackEvent(_eventTelemetry);
        }

        public void TrackFailure(string failedEventName)
        {
            _eventTelemetry.Name = failedEventName;
        }

        public void SetMetric(string name, double value)
        {
            _eventTelemetry.Metrics[name] = value;
        }

        public void SetProperty(string name, string value)
        {
            _eventTelemetry.Properties[name] = value;
        }
    }
}
