using System;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Zupa.Authentication.AuthService.Models.Account;

namespace Zupa.Authentication.AuthService.Services
{
    public class AccountService
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(
            IIdentityServerInteractionService interaction,
            IHttpContextAccessor httpContextAccessor)
        {
            _interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var viewModel = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            var user = _httpContextAccessor.HttpContext.User;
            if (user?.Identity.IsAuthenticated != true)
            {
                viewModel.ShowLogoutPrompt = false;
                return viewModel;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                viewModel.ShowLogoutPrompt = false;
                return viewModel;
            }
            return viewModel;
        }

        public async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var viewModel = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = logout?.ClientId,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            var user = _httpContextAccessor.HttpContext.User;
            if (user?.Identity.IsAuthenticated == true)
            {
                var idProvider = user.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idProvider != null && idProvider != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await _httpContextAccessor.HttpContext.GetSchemeSupportsSignOutAsync(idProvider);
                    if (providerSupportsSignout)
                    {
                        if (viewModel.LogoutId == null)
                        {
                            viewModel.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }
                        viewModel.ExternalAuthenticationScheme = idProvider;
                    }
                }
            }
            return viewModel;
        }
    }
}
