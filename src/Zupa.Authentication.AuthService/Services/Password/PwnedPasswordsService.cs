using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Zupa.Authentication.AuthService.Configuration;

namespace Zupa.Authentication.AuthService.Services.Password
{
    public class PwnedPasswordsService : IPwnedPasswordsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;

        public PwnedPasswordsService(HttpClient client, IOptions<BlacklistedPasswordsSettings> blacklistOptions)
        {
            var options = blacklistOptions?.Value ?? throw new ArgumentNullException(nameof(blacklistOptions));

            _httpClient = client;
            _endpoint = options.EndpointUri;
        }

        public async Task<Dictionary<string, int>> GetBlacklistedHashedPasswords(string hashedPasswordPrefix)
        {
            var response = await _httpClient.GetAsync($"{_endpoint}{hashedPasswordPrefix}");

            if (!response.IsSuccessStatusCode)
                throw new ApiCallFailedException();

            var passwords = await response.Content.ReadAsStringAsync();
            var hashArray = passwords.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            return hashArray.Select(hash => hash.Split(':')).ToDictionary(
                result => result[0],
                result => { int.TryParse(result[1], out int value); return value; }
            );
        }
    }
}
