using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zupa.Authentication.AuthService.Services.Password
{
    public interface IPwnedPasswordsService
    {
       Task<Dictionary<string, int>> GetBlacklistedHashedPasswords(string password);
    }
}
