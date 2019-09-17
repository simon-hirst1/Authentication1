using System.Collections.Generic;
using ApiResource = IdentityServer4.EntityFramework.Entities.ApiResource;
using Client = IdentityServer4.EntityFramework.Entities.Client;
using IdentityResourceEntity = IdentityServer4.EntityFramework.Entities.IdentityResource;
using IdentityResourceModel = IdentityServer4.Models.IdentityResource;

namespace Zupa.Authentication.AuthService.Models.Admin
{
    public class AdminViewModel
    {
        public IEnumerable<Client> Clients { get; set; }
        public IEnumerable<ApiResource> ApiResources { get; set; }
        public IEnumerable<IdentityResourceEntity> RegisteredIdentityResources { get; set; }
        public IEnumerable<IdentityResourceModel> IdentityResources { get; set; }
        public bool IsRemoveButtonVisibleForClients { get; set; }
        public bool IsRemoveButtonVisibleForApiResources { get; set; }
    }
}
