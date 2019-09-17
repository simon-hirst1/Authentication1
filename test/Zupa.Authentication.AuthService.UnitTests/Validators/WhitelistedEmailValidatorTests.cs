using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Zupa.Authentication.AuthService.Data;
using Zupa.Authentication.AuthService.Models.Entity;
using Zupa.Authentication.AuthService.Validators.User;

namespace Zupa.Authentication.AuthService.UnitTests.Validators
{
    public class WhitelistedEmailValidatorTests
    {
        private readonly WhitelistedEmailValidator<IdentityUser> _validator;
        private readonly Mock<IWhitelistRepository> _mockWhitelistRepository;

        public WhitelistedEmailValidatorTests()
        {
            _mockWhitelistRepository = new Mock<IWhitelistRepository>();

            _validator = new WhitelistedEmailValidator<IdentityUser>(_mockWhitelistRepository.Object);
        }

        [Fact(DisplayName = "GIVEN a valid email, "
            + "WHEN that email is from the zupa domain, "
            + "THEN return validation successful.")]
        public async Task ValidateAsync_EmailIsZupaDomain_SuccessReturned()
        {
            var result = await _validator.ValidateAsync(
                GetMockUserManager().Object,
                new IdentityUser
                {
                    Email = "sir.gui@zupa.co.uk"
                });

            result.Succeeded.Should().BeTrue();
        }

        [Fact(DisplayName = "GIVEN a valid email, "
            + "WHEN that email is whitelisted, "
            + "THEN return validation successful.")]
        public async Task ValidateAsync_EmailIsWhitelisted_SuccessReturned()
        {
            _mockWhitelistRepository
                .Setup(_ => _.FindAllByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<WhitelistEntity>() { new WhitelistEntity() });

            var result = await _validator.ValidateAsync(
                GetMockUserManager().Object,
                new IdentityUser
                {
                    Email = "batman@wayneenterprises.org"
                });

            result.Succeeded.Should().BeTrue();
        }

        [Fact(DisplayName = "GIVEN a valid email, "
            + "WHEN that email is note whitelisted, "
            + "THEN return validation failed.")]
        public async Task ValidateAsync_EmailIsNotWhitelisted_FailedReturned()
        {
            _mockWhitelistRepository
                .Setup(_ => _.FindAllByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(Enumerable.Empty<WhitelistEntity>());

            var result = await _validator.ValidateAsync(
                GetMockUserManager().Object,
                new IdentityUser
                {
                    Email = "bruce.wayne@wayneenterprises.org"
                });

            result.Succeeded.Should().BeFalse();
        }

        private Mock<UserManager<IdentityUser>> GetMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
        }
    }
}
