using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Reflection;
using Xunit;
using Zupa.Authentication.SmokeTests.TestSteps;

namespace Zupa.Authentication.SmokeTests
{
    public class LoginTests : IDisposable
    {
        private readonly ChromeDriver _driver;
        private readonly UserCredentials _credentials;

        public LoginTests()
        {
            _credentials = ReadCredentialsFromConfig();
            var options = new ChromeOptions();
            options.AddArguments("--headless", "--disable-gpu", "--disable-extensions");
            _driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), options);

            var url = Environment.GetEnvironmentVariable("SmokeTestUrl");
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidOperationException(
                    "Cannot open url because the SmokeTestUrl environment variable is not set.");
            OpenUrl(url);
        }

        private UserCredentials ReadCredentialsFromConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("testsettings.json")
                .Build();

            var credentials = new UserCredentials();
            config.GetSection("UserCredentials").Bind(credentials);
            return credentials;
        }

        [Fact(DisplayName = "Given I am not logged in when I click login and enter a valid username and password then I am logged in to Zupa.")]

        public void Login_ValidCredentials_UserIsLoggedIn()
        {
            Given.UserIsNotLoggedIn(_driver);
            When.UserSubmitsCredentials(_driver, _credentials.UserEmailAddress, _credentials.Password);
            Then.LogoutButtonIsDisplayed(_driver);
        }

        [Fact(DisplayName = "Given I am logged in when I click the 'Log out' link then I am logged out of Zupa.")]
        public void LogoutLinkIsClicked_UserIsLoggedOut()
        {
            Given.UserIsLoggedIn(_driver, _credentials.UserEmailAddress, _credentials.Password);
            When.UserClicksLogoutLink(_driver);
            And.UserClicksLogoutButton(_driver);
            Then.LoginLinkIsDisplayed(_driver);
        }

        [Fact(DisplayName = "Given I am not logged in when I click the 'Register' link then the register page is displayed.")]
        public void RegisterLinkIsClicked_RegisterPageIsShown()
        {
            Given.UserIsNotLoggedIn(_driver);
            When.UserClicksRegisterLink(_driver);
            Then.RegisterPageIsOpened(_driver);
        }

        [Theory(DisplayName = "Given I am not logged in when I click the button to login using a 3rd party provider then I'm redirected to the provider's page.")]
        [InlineData("Facebook", "https://www.facebook.com")]
        [InlineData("Google", "https://accounts.google.com")]
        [InlineData("Twitter", "https://api.twitter.com/oauth")]
        public void ExternalProviderLinkIsClicked_ResponseIsSomething(string providerName, string baseUrl)
        {
            Given.UserIsNotLoggedIn(_driver);
            When.UserClicksExternalProviderButton(_driver, providerName);
            Then.ThePageIsRedirectedTo(_driver, baseUrl);
        }

        [Fact(DisplayName = "Given I am logged in when I click the 'Link Account' link then the link account page is displayed.")]
        public void LinkAccountIsClicked_LinkAccountPageIsShown()
        {
            Given.UserIsLoggedIn(_driver, _credentials.UserEmailAddress, _credentials.Password);
            When.UserClicksLinkAccountButton(_driver);
            Then.LinkAccountPageIsOpened(_driver);
        }

        [Theory(DisplayName = "Given I am logged in and on the Link Account page when I click an external provider to add to my login then I'm redirected to the provider's page.")]
        [InlineData("Facebook", "https://www.facebook.com")]
        [InlineData("Google", "https://accounts.google.com")]
        [InlineData("Twitter", "https://api.twitter.com/oauth")]
        public void AddAnotherServiceIsClicked_ResponseIsSomething(string provider, string baseUrl)
        {
            Given.UserIsLoggedIn(_driver, _credentials.UserEmailAddress, _credentials.Password);
            And.UserClicksOnLinkAccountButton(_driver);
            When.UserClicksExternalProviderToAddButton(_driver, provider);
            Then.ThePageIsRedirectedTo(_driver, baseUrl);
        }

        [Fact(DisplayName = "Given I am logged in and have a password on the core account and I am on the Link Account page when I click Change Password then I'm redirected to the Change Password page.")]
        public void ChangePasswordIsClicked_ChangePasswordPageIsDisplayed()
        {
            Given.UserIsLoggedIn(_driver, _credentials.UserEmailAddress, _credentials.Password);
            And.UserClicksOnLinkAccountButton(_driver);
            When.UserClicksChangePasswordButton(_driver);
            Then.ChangePasswordPageIsOpened(_driver);
        }

        [Fact(DisplayName = "Given I am logged in as an Admin and have a password on the core account when I click Admin Management then I'm redirected to the Admin Management page.")]
        public void AdminManagementIsClicked_AdminManagementPageIsDisplayed()
        {
            Given.UserIsLoggedIn(_driver, _credentials.AdminEmailAddress, _credentials.Password);
            When.UserClicksAdminManagementButton(_driver);
            Then.AdminManagementPageIsOpened(_driver);
        }

        [Fact(DisplayName = "Given I am logged in as an Admin and have a password on the core account and I am on the Admin Management page when I click Add Client then I'm redirected to the Add Client page.")]
        public void AddClientIsClicked_AddClientPageIsDisplayed()
        {
            Given.UserIsLoggedIn(_driver, _credentials.AdminEmailAddress, _credentials.Password);
            And.UserClicksOnAddminManagementButton(_driver);
            When.UserClicksAddClientButton(_driver);
            Then.AddClientPageIsOpened(_driver);
        }

        [Fact(DisplayName = "Given I am logged in as an Admin and have a password on the core account and I am on the Admin Management page when I click Add Test Client then I'm redirected to the Add Test Client page.")]
        public void AddTestClientIsClicked_AddTestClientPageIsDisplayed()
        {
            Given.UserIsLoggedIn(_driver, _credentials.AdminEmailAddress, _credentials.Password);
            And.UserClicksOnAddminManagementButton(_driver);
            When.UserClicksAddTestClientButton(_driver);
            Then.AddTestClientPageIsOpened(_driver);
        }

        [Fact(DisplayName = "Given I am logged in as an Admin and have a password on the core account and I am on the Admin Management page " +
                            "when I click Add Api Resource then I'm redirected to the Add Api Resource page.")]
        public void AddApiResourceIsClicked_AddApiResourcePageIsDisplayed()
        {
            Given.UserIsLoggedIn(_driver, _credentials.AdminEmailAddress, _credentials.Password);
            And.UserClicksOnAddminManagementButton(_driver);
            When.UserClicksAddApiResourceButton(_driver);
            Then.AddApiResourcePageIsOpened(_driver);
        }

        private void OpenUrl(string url)
        {
            _driver.Url = url;
            _driver.Navigate();
        }

        public void Dispose()
        {
            _driver?.Quit();
        }
    }
}