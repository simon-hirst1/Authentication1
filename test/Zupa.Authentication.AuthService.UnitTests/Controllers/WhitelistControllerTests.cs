using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Zupa.Authentication.AuthService.Controllers;
using Zupa.Authentication.AuthService.Data;
using Zupa.Authentication.AuthService.InformationModels;

namespace Zupa.Authentication.AuthService.UnitTests.Controllers
{
    public class WhitelistControllerTests
    {
        private readonly WhitelistController _controller;

        private readonly Mock<IWhitelistRepository> _mockWhitelistRepository;

        public WhitelistControllerTests()
        {
            _mockWhitelistRepository = new Mock<IWhitelistRepository>();

            _controller = new WhitelistController(_mockWhitelistRepository.Object);
        }

        [Fact(DisplayName = "GIVEN valid invite information WHEN requesting to add to whitelist THEN insert the email address entry")]
        public async Task Add_ValidInviteInformation_InsertWhitelistEntry()
        {
            var emailAddress = "bruce.wayne@wayneenterprises.org";
            var inviteId = Guid.NewGuid();
            var invite = new InviteInformation(inviteId, emailAddress);

            await _controller.Add(invite);

            _mockWhitelistRepository.Verify(_ => _.InsertAsync(invite), Times.Once);
        }

        [Fact(DisplayName = "GIVEN a valid invite information WHEN requesting to delete from whitelist THEN delete the email address entry")]
        public async Task Delete_ValidInviteInformation_DeleteWhitelistEntry()
        {
            var inviteId = Guid.NewGuid();

            await _controller.Remove(inviteId);

            _mockWhitelistRepository.Verify(_ => _.RemoveAsync(inviteId.ToString()), Times.Once);
        }
    }
}
