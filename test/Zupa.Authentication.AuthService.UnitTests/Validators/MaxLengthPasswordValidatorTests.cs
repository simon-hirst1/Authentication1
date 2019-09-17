using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Zupa.Authentication.AuthService.Configuration;
using Zupa.Authentication.AuthService.Validators.Password;

namespace Zupa.Authentication.AuthService.UnitTests.Validators
{
    public class MaxLengthPasswordValidatorTests
    {
        public readonly MaxLengthPasswordValidator<IdentityUser> _validator;

        public MaxLengthPasswordValidatorTests()
        {
            var maxPasswordLength = Options.Create(new PasswordRulesSettings
            {
                MaxLength = 10
            });

            _validator = new MaxLengthPasswordValidator<IdentityUser>(maxPasswordLength);
        }

        [Fact(DisplayName = "GIVEN a password is validated, WHEN that password is less than the maximum length count, THEN Success is returned.")]
        public async Task ValidatePassword_IsLessThanMax_SuccessReturned()
        {
            var userManager = GetMockUserManager();
            var user = new IdentityUser();
            IdentityResult result = await _validator.ValidateAsync(userManager.Object, user, "qwerty");

            result.Succeeded.Should().BeTrue();
        }

        [Fact(DisplayName = "GIVEN a password is validated, WHEN that password is greater than the maximum length count, THEN Failed is returned.")]
        public async Task ValidatePassword_IsGreaterThanMax_FailedReturned()
        {
            var userManager = GetMockUserManager();
            var user = new IdentityUser();
            IdentityResult result = await _validator.ValidateAsync(userManager.Object, user, "qwertyuiopasd");

            result.Succeeded.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
        }

        private Mock<UserManager<IdentityUser>> GetMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
        }
    }
}
