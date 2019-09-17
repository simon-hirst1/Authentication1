using System.Collections.Generic;
using System.Threading.Tasks;
using Zupa.Authentication.AuthService.InformationModels;
using Zupa.Authentication.AuthService.Models.Entity;

namespace Zupa.Authentication.AuthService.Data
{
    public interface IWhitelistRepository
    {
        Task<IEnumerable<WhitelistEntity>> FindAllByEmailAsync(string emailaddress);
        Task InsertAsync(InviteInformation invite);
        Task RemoveAsync(string inviteId);
    }
}
