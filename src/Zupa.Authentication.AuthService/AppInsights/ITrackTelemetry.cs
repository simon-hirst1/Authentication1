using System;

namespace Zupa.Authentication.AuthService.AppInsights
{
    public interface ITrackTelemetry
    {
        void TrackEvent(EventName eventName, EventType eventType, EventStatus eventEventStatus);

        void TrackEvent(EventName eventName, EventType eventType, Providers externalProvider);

        void TrackEvent(EventName eventName, EventType eventType, string customEventData);

        void TrackException(Exception exception);
    }
}