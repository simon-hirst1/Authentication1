using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Zupa.Authentication.AuthService.Configuration;

namespace Zupa.Authentication.AuthService.Validators.Password
{
    public class MaxLengthPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : IdentityUser
    {
        private readonly int _maxPasswordLength;

        public MaxLengthPasswordValidator(
           IOptions<PasswordRulesSettings> maxPasswordLength
         )
        {
            var options = maxPasswordLength?.Value ?? throw new ArgumentNullException(nameof(maxPasswordLength));

            _maxPasswordLength = options.MaxLength;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            if (password.Length < _maxPasswordLength)
                return IdentityResult.Success;

            return IdentityResult.Failed(
                new IdentityError()
                {
                    Code = nameof(password),
                    Description = $"Passwords must be less than {_maxPasswordLength} characters."
                }
            );
        }
    }
}
