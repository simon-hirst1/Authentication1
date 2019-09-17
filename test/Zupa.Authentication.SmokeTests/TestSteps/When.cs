using OpenQA.Selenium;
using Zupa.Authentication.SmokeTests.Pages;

namespace Zupa.Authentication.SmokeTests.TestSteps
{
    public static class When
    {
        public static void UserSubmitsCredentials(IWebDriver driver, string email, string password)
        {
            var page = new Home(driver);
            page.EnterEmail(email);
            page.EnterPassword(password);
            page.ClickSubmitButton();
        }

        public static void UserClicksLogin(IWebDriver driver)
        {
            var page = new Home(driver);
            page.ClickLoginLink();
        }

        public static void UserClicksLogoutLink(IWebDriver driver)
        {
            var page = new Home(driver);
            page.ClickLogoutButton();
        }

        public static void UserClicksRegisterLink(IWebDriver driver)
        {
            var page = new Home(driver);
            page.ClickRegisterLink();
        }

        public static void UserClicksExternalProviderButton(IWebDriver driver, string providerName)
        {
            var page = new Home(driver);
            page.ClickProviderButton(providerName);
        }

        public static void UserClicksLinkAccountButton(IWebDriver driver)
        {
            var page = new Home(driver);
            page.ClickLinkAccountButton();
        }

        public static void UserClicksExternalProviderToAddButton(IWebDriver driver, string providerName)
        {
            var page = new LinkAccount(driver);
            page.ClickProviderButton(providerName);
        }

        public static void UserClicksChangePasswordButton(IWebDriver driver)
        {
            var page = new LinkAccount(driver);
            page.ClickCreatePasswordButton();
        }

        public static void UserClicksAdminManagementButton(IWebDriver driver)
        {
            var page = new Home(driver);
            page.ClickAdminManagementButton();
        }

        public static void UserClicksAddClientButton(IWebDriver driver)
        {
            var page = new AdminManagement(driver);
            page.ClickAddClientButton();
        }

        public static void UserClicksAddTestClientButton(IWebDriver driver)
        {
            var page = new AdminManagement(driver);
            page.ClickAddTestClientButton();
        }

        public static void UserClicksAddApiResourceButton(IWebDriver driver)
        {
            var page = new AdminManagement(driver);
            page.ClickAddApiResourceButton();
        }
    }
}
