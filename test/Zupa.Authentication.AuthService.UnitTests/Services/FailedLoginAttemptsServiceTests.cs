using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Zupa.Authentication.AuthService.Configuration;
using Zupa.Authentication.AuthService.Data;
using Zupa.Authentication.AuthService.Models.Account;
using Zupa.Authentication.AuthService.Services.FailedLoginAttempts;

namespace Zupa.Authentication.AuthService.UnitTests.Services
{
    public class FailedLoginAttemptsServiceTests
    {
        private readonly IFailedLoginAttemptsService _service;
        private readonly Mock<IFailedLoginAttemptsRepository> _mockRepository;

        public FailedLoginAttemptsServiceTests()
        {
            _mockRepository = new Mock<IFailedLoginAttemptsRepository>();
            
            var failedAttemptsSettings = new Mock<IOptions<FailedLoginAttemptsSettings>>();
            failedAttemptsSettings
                .Setup(fa => fa.Value)
                .Returns(new FailedLoginAttemptsSettings() { MaxRequestsCount = 7, TimeIntervalInSeconds = 60});
            
            _service = new FailedLoginAttemptsService(_mockRepository.Object, failedAttemptsSettings.Object);
        }
        
        [Fact(DisplayName = " GIVEN an IpAddress," +
                            " WHEN that IpAddress has too many requests stored in the db," +
                            " THEN TooManyRequestsForIpAddressAsync service method returns true.")]
        public async Task IpAddress_TooManyRequestForAddress_ReturnsTrue()
        {
            const string ipAddress = "1.2.3.4";
            _mockRepository
                .Setup(repo => repo.FindFailedAttemptsForIpAddressAsync(ipAddress, It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(Enumerable.Range(0, 8).Select(failedAttempts => new FailedAttempt(ipAddress, DateTimeOffset.Now)).ToList());
            
            var result = await _service.HasTooManyRequestsForIpAddressAsync(ipAddress);
            result.Should().BeTrue();
        }
        
        [Fact(DisplayName = " GIVEN an IpAddress," +
                            " WHEN that IpAddress hasn't got too many requests stored in the db," +
                            " THEN TooManyRequestsForIpAddressAsync service method returns false.")]
        public async Task IpAddress_NotTooManyRequestForAddress_ReturnsFalse()
        {
            const string ipAddress = "1.2.3.4";
            _mockRepository
                .Setup(repo => repo.FindFailedAttemptsForIpAddressAsync(ipAddress, It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(Enumerable.Range(0, 5).Select(failedAttempts => new FailedAttempt(ipAddress, DateTimeOffset.Now)).ToList());
            
            var result = await _service.HasTooManyRequestsForIpAddressAsync(ipAddress);
            result.Should().BeFalse();
        }
        
        [Fact(DisplayName = " GIVEN an IpAddress," +
                            " WHEN handling a failed login attempt for that ipAddress," +
                            " THEN remove old entries method is called.")]
        public async Task IpAddress_HandleFailedLoginAttempt_RemoveOldEntriesIsCalled()
        {
            await _service.HandleFailedLoginAttempt("1.2.3.4");
            _mockRepository.Verify(r => r.RemoveAnyOldEntries(It.IsAny<string>(), It.IsAny<DateTimeOffset>()),Times.Once);
        }
        
        [Fact(DisplayName = " GIVEN an IpAddress," +
                            " WHEN handling a failed login attempt for that ipAddress," +
                            " THEN persist new failed attempt method is called.")]
        public async Task IpAddress_HandleFailedLoginAttempt_PersistFailedAttemptIsCalled()
        {
            await _service.HandleFailedLoginAttempt("1.2.3.4");
            _mockRepository.Verify(r => r.PersistFailedAttemptAsync(It.IsAny<FailedAttempt>()),Times.Once);
        }
    }
}
