using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Zupa.Authentication.AuthService.ExternalProviders
{
    public static class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddFacebookAuthenticationIfExists(this AuthenticationBuilder builder, FacebookAuthentication authentication)
        {
            if (!string.IsNullOrEmpty(authentication?.FacebookKey))
            {
                builder.AddFacebook("Facebook", options =>
                {
                    options.AppId = authentication.FacebookKey;
                    options.AppSecret = authentication.FacebookSecret;
                });
            }
          
            return builder;
        }

        public static AuthenticationBuilder AddGoogleAuthenticationIfExists(this AuthenticationBuilder builder, GoogleAuthentication authentication)
        {
            if (!string.IsNullOrEmpty(authentication?.GoogleKey))
            {
                builder.AddGoogle("Google", options =>
                {
                    options.ClientId = authentication.GoogleKey;
                    options.ClientSecret = authentication.GoogleSecret;
                });
            }

            return builder;
        }

        public static AuthenticationBuilder AddTwitterAuthenticationIfExists(this AuthenticationBuilder builder, TwitterAuthentication authentication)
        {
            if (!string.IsNullOrEmpty(authentication?.TwitterKey))
            {
                builder.AddTwitter("Twitter", options =>
                {
                    options.ConsumerKey = authentication.TwitterKey;
                    options.ConsumerSecret = authentication.TwitterSecret;
                });
            }

            return builder;
        }
    }
}
