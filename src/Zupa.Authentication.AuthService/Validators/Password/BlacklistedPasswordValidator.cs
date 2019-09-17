using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Zupa.Authentication.AuthService.AppInsights;
using Zupa.Authentication.AuthService.Configuration;
using Zupa.Authentication.AuthService.Services.Password;

namespace Zupa.Authentication.AuthService.Validators.Password
{
    public class BlacklistedPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : IdentityUser
    {
        private readonly IPwnedPasswordsService _blacklistService;
        private readonly int _passwordThreshold;
        private readonly ITrackTelemetry _trackTelemetry;

        public BlacklistedPasswordValidator(
            IPwnedPasswordsService service,
            IOptions<BlacklistedPasswordsSettings> blacklistOptions,
            ITrackTelemetry trackTelemetry)
        {
            var options = blacklistOptions?.Value ?? throw new ArgumentNullException(nameof(blacklistOptions));

            _blacklistService = service;
            _passwordThreshold = options.ThresholdLimit;
            _trackTelemetry = trackTelemetry;

        }

        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            var hashedPassword = HashPasswordToSHA1(password);
            Dictionary<string, int> matchedHashes;

            try
            {
                matchedHashes = await _blacklistService.GetBlacklistedHashedPasswords(hashedPassword.Substring(0, 5));
            }
            catch (ApiCallFailedException)
            {
                _trackTelemetry.TrackEvent(EventName.SetPassword, EventType.VulnerablePassword, user.Id);
                return IdentityResult.Success;
            }

            if (matchedHashes.TryGetValue(hashedPassword.Substring(5), out int timesCompromised) && timesCompromised > _passwordThreshold)
                return IdentityResult.Failed(
                    new IdentityError()
                    {
                        Code = nameof(password),
                        Description = "The password you have entered is known to be vulnerable."
                    }
                );
            return IdentityResult.Success;
        }

        private string HashPasswordToSHA1(string passwordToHash)
        {
            var hashBytes = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(passwordToHash));

            var stringBuilder = new StringBuilder();
            foreach (var hashByte in hashBytes)
            {
                stringBuilder.AppendFormat("{0:X2}", hashByte);
            }

            return stringBuilder.ToString();
        }
    }
}
