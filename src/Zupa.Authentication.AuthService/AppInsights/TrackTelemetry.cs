using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;

namespace Zupa.Authentication.AuthService.AppInsights
{
    public class TrackTelemetry : ITrackTelemetry
    {
        private readonly TelemetryClient _telemetryClient;

        public TrackTelemetry(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        private void Track(string eventName, string eventType, string eventData)
        {
            _telemetryClient.TrackEvent(eventName, new Dictionary<string, string>
            {
                {eventType, eventData}
            });
        }

        public void TrackEvent(EventName eventName, EventType eventType, EventStatus eventStatus)
        {
            Track(eventName.ToString(), eventType.ToString(), eventStatus.ToString());
        }

        public void TrackEvent(EventName eventName, EventType eventType, Providers externalProvider)
        {
            Track(eventName.ToString(), eventType.ToString(), externalProvider.ToString());
        }

        public void TrackEvent(EventName eventName, EventType eventType, string customEventData)
        {
            Track(eventName.ToString(), eventType.ToString(), customEventData);
        }

        public void TrackException(Exception exception)
        {
            _telemetryClient.TrackException(exception);
        }
    }
}