using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Zupa.Authentication.AuthService.Controllers;

namespace Zupa.Authentication.AuthService.UnitTests.Controllers
{
    public class UserControllerTests
    {
        private readonly UserController _controller;

        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;

        public UserControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(userStoreMock.Object,
                null, null, null, null, null, null, null, null);

            _controller = new UserController(_mockUserManager.Object);
        }

        [Fact(DisplayName = "GIVEN the user exists WHEN requesting a user by email address THEN find the user and return ID")]
        public async Task GetByEmail_UserExists_ReturnsUserId()
        {
            var emailAddress = "bruce.wayne@wayneenterprises.org";
            var userID = Guid.NewGuid().ToString();
            _mockUserManager
                .Setup(_ => _.FindByEmailAsync(emailAddress))
                .ReturnsAsync(new IdentityUser { Id = userID });

            var result = await _controller.FindIdByEmail(emailAddress);

            result.Value.Should().Be(userID);
            _mockUserManager.Verify(_ => _.FindByEmailAsync(emailAddress), Times.Once);
        }

        [Fact(DisplayName = "GIVEN the user does not exist WHEN requesting a user by email address THEN return NotFound")]
        public async Task GetByEmail_NoUserExists_ReturnsNotFound()
        {
            var result = await _controller.FindIdByEmail("alfred.pennyworth@wayneenterprises.org");

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact(DisplayName = "GIVEN the user exists WHEN requesting a user email by id THEN find the user and return email")]
        public async Task GetEmailById_UserExists_ReturnsEmail()
        {
            var emailAddress = "bruce.wayne@wayneenterprises.org";
            var userID = Guid.NewGuid();
            _mockUserManager
                .Setup(_ => _.FindByIdAsync(userID.ToString()))
                .ReturnsAsync(new IdentityUser { Id = userID.ToString(), Email = emailAddress });

            var result = await _controller.FindEmailById(userID);

            result.Value.Should().Be(emailAddress);
            _mockUserManager.Verify(_ => _.FindByIdAsync(userID.ToString()), Times.Once);
        }

        [Fact(DisplayName = "GIVEN the user does not exist WHEN requesting a user by email address THEN return NotFound")]
        public async Task GetEmailById_NoUserExists_ReturnsNotFound()
        {
            var result = await _controller.FindEmailById(Guid.NewGuid());

            result.Result.Should().BeOfType<NotFoundResult>();
        }
    }
}
