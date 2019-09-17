using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Zupa.Authentication.AuthService.Validators.Password;

namespace Zupa.Authentication.AuthService.UnitTests.Validators
{
    public class ExpectedInformationPasswordValidatorTests
    {
        private readonly ExpectedInformationPasswordValidator<IdentityUser> _validator;

        public ExpectedInformationPasswordValidatorTests()
        {
            _validator = new ExpectedInformationPasswordValidator<IdentityUser>();
        }

        [Fact(DisplayName = "GIVEN a password is validated, WHEN that password is the same as the username, THEN Failed is returned.")]
        public async Task ValidatePassword_PasswordSameAsUsername_FailedReturned()
        {
            var userManager = GetMockUserManager();
            var user = new IdentityUser("qwerty");
            IdentityResult result = await _validator.ValidateAsync(userManager.Object, user, "qwerty");

            result.Succeeded.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
        }

        [Fact(DisplayName = "GIVEN a password is validated, WHEN that password is the same as the email, THEN Failed is returned.")]
        public async Task ValidatePassword_PasswordSameAsEmail_FailedReturned()
        {
            var userManager = GetMockUserManager();
            var user = new IdentityUser
            {
                Email = "qwerty"
            };
            IdentityResult result = await _validator.ValidateAsync(userManager.Object, user, "qwerty");

            result.Succeeded.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
        }

        [Fact(DisplayName = "GIVEN a password is validated, WHEN that password is NOT the same as the email or username, THEN Success is returned.")]
        public async Task ValidatePassword_PasswordNotEmailOrUsername_SuccessReturned()
        {
            var userManager = GetMockUserManager();
            var user = new IdentityUser
            {
                UserName = "qwerty1",
                Email = "qwerty"
            };
            IdentityResult result = await _validator.ValidateAsync(userManager.Object, user, "qwerty123");

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
