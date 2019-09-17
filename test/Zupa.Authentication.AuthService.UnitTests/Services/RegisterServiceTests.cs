using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Zupa.Authentication.AuthService.Services.Registration;
using Zupa.Libraries.ServiceBus.ServiceBusClient;

namespace Zupa.Authentication.AuthService.UnitTests.Services
{
    public class RegisterServiceTests
    {
        [Theory(DisplayName = "When the method parameters are invalid an ArgumentException is thrown")]
        [MemberData(nameof(GetParameters))]
        public async Task InitiateDataRequestAsync_InvalidParameters_ThrowsArgumentException(Guid userId, string userName, string email, string invalidParameter)
        {
            var serviceBusMock = new Mock<IServiceBusClient<ITopicClient>>();
            var registerService = new RegisterService(serviceBusMock.Object);
            var exception = await Record.ExceptionAsync(() => registerService.SendUserRegistrationAsync(userId, userName, email));

            exception.Should().BeOfType<ArgumentException>();
            exception.Message.Should().Contain(invalidParameter);
        }

        [Fact(DisplayName = "When RegisteredUserAsync is called with valid parameters then call SendAsync")]
        public async Task InitiateDataRequestAsync_ValidParameters_CallsSendAsync()
        {
            var serviceBusMock = new Mock<IServiceBusClient<ITopicClient>>();
            var registerService = new RegisterService(serviceBusMock.Object);

            await registerService.SendUserRegistrationAsync(Guid.NewGuid(), "ValidUserName", "valid@email.com");
            serviceBusMock.Verify(r => r.SendAsync(It.IsAny<Message>()),Times.Once);
        }

        public static IEnumerable<object[]> GetParameters()
        {
            yield return new object[]
            {
                Guid.Empty,
                "TestUserName",
                "test@zupa.co.uk",
                "userId"
            };
            yield return new object[]
            {
                Guid.NewGuid(),
                "",
                "test@zupa.co.uk",
                "userName"
            };
            yield return new object[]
            {
                 Guid.NewGuid(),
                "TestUserName",
                "",
                "email"
            };
            yield return new object[]
            {
                Guid.NewGuid(),
                " ",
                "test@zupa.co.uk",
                "userName"
            };
            yield return new object[]
            {
                 Guid.NewGuid(),
                "TestUserName",
                " ",
                "email"
            };      
        }
    }
}
