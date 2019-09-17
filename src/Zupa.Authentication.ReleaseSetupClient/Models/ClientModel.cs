using System.Collections.Generic;

namespace Zupa.Authentication.ReleaseSetupClient.Models
{
    public class ClientModel
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public IEnumerable<string> ClientSecrets { get; set; }
        public IEnumerable<string> RedirectUris { get; set; }
        public IEnumerable<string> PostLogoutRedirectUris { get; set; }
        public IEnumerable<string> AllowedCorsOrigins { get; set; }
        public IEnumerable<string> AllowedScopes { get; set; }
        public IEnumerable<string> GrantTypes { get; set; }
    }
}
