using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Zupa.Authentication.AuthService.Exceptions;
using Zupa.Authentication.AuthService.Services;

namespace Zupa.Authentication.AuthService.AppInsights
{
    public class AppInsightsRequestTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITrackTelemetry _trackTelemetry;

        public AppInsightsRequestTrackingMiddleware(RequestDelegate next, ITrackTelemetry trackTelemetry)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _trackTelemetry = trackTelemetry ?? throw new ArgumentNullException(nameof(trackTelemetry));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                HandleException(context, exception);
                _trackTelemetry.TrackException(exception);
            }
        }

        private static void HandleException(HttpContext context, Exception exception)
        {
            switch (exception)
            {
                case FailedAttemptExistsException _:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    break;
                case IpAddressNotFoundException _:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }
        }
    }
}
