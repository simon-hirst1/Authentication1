using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Sodium;
using System;
using Zupa.Authentication.AuthService.Configuration;

namespace Zupa.Authentication.AuthService.Security
{
    public class ArgonHash<TUser> : PasswordHasher<TUser> where TUser : class
    {
        private readonly int _operationsLimit;
        private readonly int _memoryLimit;

        public ArgonHash(IOptions<PasswordHashingConfiguration> passwordHashingConfiguration)
        {
            if (passwordHashingConfiguration.Value == null)
                throw new ArgumentNullException(nameof(passwordHashingConfiguration));

            _operationsLimit = passwordHashingConfiguration.Value.OperationsLimit;
            _memoryLimit = passwordHashingConfiguration.Value.MemoryLimit;
        }

        public override string HashPassword(TUser user, string password)
        {
            return PasswordHash.ArgonHashString(password, _operationsLimit, _memoryLimit);
        }

        public override PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
        {
            return PasswordHash.ArgonHashStringVerify(hashedPassword, providedPassword) ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
        }
    }
}
