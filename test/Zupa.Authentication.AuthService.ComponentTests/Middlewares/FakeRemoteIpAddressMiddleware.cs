using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace Zupa.Authentication.AuthService.ComponentTests.Middlewares
{
    public class FakeRemoteIpAddressMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IPAddress _fakeIpAddress = IPAddress.Parse(TestConstants.FakeIpAddress);

        public FakeRemoteIpAddressMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Connection.RemoteIpAddress = _fakeIpAddress;

            await _next(httpContext);
        }
    }
}
