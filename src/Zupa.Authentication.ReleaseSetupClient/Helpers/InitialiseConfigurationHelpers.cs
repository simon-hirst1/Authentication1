using IdentityServer4.Models;
using System;
using System.Linq;
using Zupa.Authentication.ReleaseSetupClient.Models;

namespace Zupa.Authentication.ReleaseSetupClient.Helpers
{
    internal class InitialiseConfigurationHelpers
    {
        internal static Client MapClient(ClientModel model) =>
            new Client
            {
                ClientId = model.ClientId,
                ClientName = model.ClientName,
                AllowedGrantTypes = model.GrantTypes.ToArray(),
                AllowAccessTokensViaBrowser = true,
                RequireConsent = false,
                ClientSecrets = model.ClientSecrets?.Select(secret => new Secret(secret.Sha256())).ToArray(),
                RedirectUris = model.RedirectUris?.ToArray(),
                PostLogoutRedirectUris = model.PostLogoutRedirectUris?.ToArray(),
                AllowedCorsOrigins = model.AllowedCorsOrigins?.ToArray(),
                AllowedScopes = model.AllowedScopes.ToArray()
            };


        internal static ApiResource MapApiResource(ApiResourceModel model) =>
            new ApiResource(model.Name, model.DisplayName);

        internal static IdentityResource MapIdentityResource(string resourceType)
        {
            switch (resourceType.ToLowerInvariant())
            {
                case "address":
                    return new IdentityResources.Address();
                case "email":
                    return new IdentityResources.Email();
                case "openid":
                    return new IdentityResources.OpenId();
                case "phone":
                    return new IdentityResources.Phone();
                case "profile":
                    return new IdentityResources.Profile();
                default:
                    throw new ArgumentException(
                        $"IdentityResource {resourceType} not recognised.",
                        nameof(resourceType)
                    );
            }
        }
    }
}
