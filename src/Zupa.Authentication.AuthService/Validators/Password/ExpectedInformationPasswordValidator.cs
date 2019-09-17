using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Zupa.Authentication.AuthService.Validators.Password
{
    public class ExpectedInformationPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : IdentityUser
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            if (password == user.UserName || password == user.Email)
            {
                return IdentityResult.Failed(
                   new IdentityError()
                   {
                       Code = nameof(password),
                       Description = "The password must not match your personal information."
                   }
               );
            }

            return IdentityResult.Success;
        }
    }
}
