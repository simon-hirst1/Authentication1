using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Zupa.Authentication.AuthService.Data;
using Zupa.Authentication.AuthService.InformationModels;

namespace Zupa.Authentication.AuthService.Controllers
{
    [Authorize("apipolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class WhitelistController : Controller
    {
        private readonly IWhitelistRepository _whitelistRepository;

        public WhitelistController(IWhitelistRepository whitelistRepository)
        {
            _whitelistRepository = whitelistRepository;
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] InviteInformation invite)
        {
            await _whitelistRepository.InsertAsync(invite);
            return Ok();
        }

        [HttpDelete("{inviteId:Guid}")]
        public async Task<ActionResult> Remove(Guid inviteId)
        {
            await _whitelistRepository.RemoveAsync(inviteId.ToString());
            return Ok();
        }
    }
}
