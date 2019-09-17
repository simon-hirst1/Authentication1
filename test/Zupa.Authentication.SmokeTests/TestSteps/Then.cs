using FluentAssertions;
using OpenQA.Selenium;
using Zupa.Authentication.SmokeTests.Pages;

namespace Zupa.Authentication.SmokeTests.TestSteps
{
    public static class Then
    {
        public static void LogoutButtonIsDisplayed(IWebDriver driver)
        {
            var page = new Home(driver);
            page.LogoutButton.Displayed.Should().BeTrue();
        }

        public static void LoginLinkIsDisplayed(IWebDriver driver)
        {
            var page = new Home(driver);
            page.LoginLink.Displayed.Should().BeTrue();
        }

        public static void RegisterPageIsOpened(IWebDriver driver)
        {
            driver.Url.Should().EndWithEquivalent("Register");
        }

        public static void ThePageIsRedirectedTo(IWebDriver driver, string baseUrl)
        {
            driver.Url.Should().StartWithEquivalent(baseUrl);
        }

        public static void LinkAccountPageIsOpened(IWebDriver driver)
        {
            driver.Url.Should().EndWithEquivalent("LinkAccount");
        }

        public static void ChangePasswordPageIsOpened(IWebDriver driver)
        {
            driver.Url.Should().EndWithEquivalent("ChangePassword");
        }

        public static void AdminManagementPageIsOpened(IWebDriver driver)
        {
            driver.Url.Should().EndWithEquivalent("Admin/Index");
        }

        public static void AddClientPageIsOpened(IWebDriver driver)
        {
            driver.Url.Should().EndWithEquivalent("AddClient");
        }

        public static void AddTestClientPageIsOpened(IWebDriver driver)
        {
            driver.Url.Should().EndWithEquivalent("AddTestClient");
        }

        public static void AddApiResourcePageIsOpened(IWebDriver driver)
        {
            driver.Url.Should().EndWithEquivalent("AddApiResource");
        }
    }
}
