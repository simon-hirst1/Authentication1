using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Zupa.Authentication.AuthService.AppInsights;

namespace Zupa.Authentication.AuthService.UnitTests
{
    public class AppInsightsRequestTrackingMiddlewareTests
    {
        public static IEnumerable<object[]> GetInvalidTestDataForConstructor()
        {
            yield return new object[] { "next", null, Mock.Of<ITrackTelemetry>() };
            yield return new object[] { "trackTelemetry", Mock.Of<RequestDelegate>(), null };
        }

        [Fact(DisplayName = "When the middleware catches an exception then the response status code is 500.")]
        public async Task Invoke_ExceptionIsCaught_SetsResponseStatusCodeTo500()
        {
            var response = new Mock<HttpResponse>();
            var next = new RequestDelegate(ctx => throw new Exception("testing"));

            var middleware = new AppInsightsRequestTrackingMiddleware(next, Mock.Of<ITrackTelemetry>());
            await middleware.Invoke(Mock.Of<HttpContext>(c => c.Response == response.Object));

            response.VerifySet(rsp => rsp.StatusCode = StatusCodes.Status500InternalServerError, Times.Once());
        }

        [Fact(DisplayName = "When the constructor is called with valid arguments, then it should not throw an ArguentNullException")]
        public void Constructor_ArgumentIsNotNull_ExceptionIsNotThrown()
        {
            var exception = Record.Exception(() => new AppInsightsRequestTrackingMiddleware(Mock.Of<RequestDelegate>(), Mock.Of<ITrackTelemetry>()));
            Assert.Null(exception);
        }

        [Theory(DisplayName = "When the constructor is called with invalid arguments, then an ArgumentNullException should be thrown.")]
        [MemberData(nameof(GetInvalidTestDataForConstructor))]
        public void Constructor_ArgumentIsNull_ExceptionIsThrown(string paramName, RequestDelegate requestDelegate, ITrackTelemetry trackTelemetry)
        {
            Action action = () => new AppInsightsRequestTrackingMiddleware(requestDelegate,trackTelemetry);
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(paramName);
        }
    }
}