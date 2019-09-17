using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Zupa.Authentication.AuthService.Data;

namespace Zupa.Authentication.AuthService.Validators.User
{
    public class WhitelistedEmailValidator<TUser> : IUserValidator<TUser> where TUser : IdentityUser
    {
        private readonly IWhitelistRepository _whitelistRepository;

        public WhitelistedEmailValidator(IWhitelistRepository whitelistRepository)
        {
            _whitelistRepository = whitelistRepository;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
        {
            if (Regex.IsMatch(user.Email, @"^[a-zA-Z0-9._%+\-!#$&'*\/=?^`{|}~]+(@zupa\.co\.uk)$"))
                return IdentityResult.Success;

            var entity = await _whitelistRepository.FindAllByEmailAsync(user.Email);
            if (!entity.Any())
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Code = nameof(user.Email),
                        Description = "Registration is currently limited."
                    });

            return IdentityResult.Success;
        }
    }
}
