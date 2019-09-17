using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Zupa.Authentication.AuthService.AppInsights;
using Zupa.Authentication.AuthService.Configuration;
using Zupa.Authentication.AuthService.Services.Password;
using Zupa.Authentication.AuthService.Validators.Password;

namespace Zupa.Authentication.AuthService.UnitTests.Validators
{
    public class BlacklistedPasswordValidatorTests
    {
        public readonly BlacklistedPasswordValidator<IdentityUser> _validator;
        public readonly Mock<IPwnedPasswordsService> _mockPwnedService;

        private readonly string hashedPassword = "B1B3773A05C0ED0176787A4F1574FF0075F7521E"; //qwerty

        public BlacklistedPasswordValidatorTests()
        {
            _mockPwnedService = new Mock<IPwnedPasswordsService>();
            var telemetry = new Mock<ITrackTelemetry>();
            var blacklistedPasswordSettings = Options.Create(new BlacklistedPasswordsSettings
            {
                ThresholdLimit = 15
            });

            _validator = new BlacklistedPasswordValidator<IdentityUser>(_mockPwnedService.Object, blacklistedPasswordSettings, telemetry.Object);
        }

        [Fact(DisplayName = "GIVEN a password is validated, WHEN that password is blacklisted, THEN Failed is returned.")]
        public async Task ValidatePassword_IsBlacklisted_FailedReturned()
        {
            _mockPwnedService.Setup(service => service.GetBlacklistedHashedPasswords(It.IsAny<string>()))
              .ReturnsAsync(new Dictionary<string, int>()
              {
                  { hashedPassword.Substring(5), 234 }
              });

            var userManager = GetMockUserManager();
            var user = new IdentityUser();
            IdentityResult result = await _validator.ValidateAsync(userManager.Object, user, "qwerty");

            result.Succeeded.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
        }

        [Fact(DisplayName = "GIVEN a password is validated, WHEN that password is NOT blacklisted, THEN Success is returned.")]
        public async Task ValidatePassword_IsNotBlacklisted_SuccessReturned()
        {
            _mockPwnedService.Setup(service => service.GetBlacklistedHashedPasswords(It.IsAny<string>()))
              .ReturnsAsync(new Dictionary<string, int>()
              {
                  { "7C4A8D09CA3762AF61E59520943DC26494F8941B", 12 }
              });

            var userManager = GetMockUserManager();
            var user = new IdentityUser();
            IdentityResult result = await _validator.ValidateAsync(userManager.Object, user, "qwerty");

            result.Succeeded.Should().BeTrue();
        }

        [Fact(DisplayName = "GIVEN a password is validated, WHEN that password is blacklisted but with a lower compromised count than the set threshold, THEN Success is returned.")]
        public async Task ValidatePassword_IsBlacklistedWithLowerCountThanThreshold_SuccessReturned()
        {
            _mockPwnedService.Setup(service => service.GetBlacklistedHashedPasswords(It.IsAny<string>()))
              .ReturnsAsync(new Dictionary<string, int>()
              {
                  { hashedPassword.Substring(5), 14 }
              });

            var userManager = GetMockUserManager();
            var user = new IdentityUser();
            IdentityResult result = await _validator.ValidateAsync(userManager.Object, user, "qwerty");

            result.Succeeded.Should().BeTrue();
        }

        [Fact(DisplayName = "GIVEN a password is validated, WHEN the API call failed, THEN Success is returned.")]
        public async Task ValidatePassword_ApiCallFailed_SuccessReturned()
        {
            _mockPwnedService.Setup(service => service.GetBlacklistedHashedPasswords(It.IsAny<string>()))
              .ThrowsAsync(new ApiCallFailedException());

            var userManager = GetMockUserManager();
            var user = new IdentityUser();
            IdentityResult result = await _validator.ValidateAsync(userManager.Object, user, "qwerty");

            result.Succeeded.Should().BeTrue();
        }

        private Mock<UserManager<IdentityUser>> GetMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
        }
    }
}
