using OpenQA.Selenium;
using Zupa.Authentication.SmokeTests.Pages;

namespace Zupa.Authentication.SmokeTests.TestSteps
{
    public class And
    {
        public static void UserClicksLogoutButton(IWebDriver driver)
        {
            var page = new Logout(driver);
            page.ClickLogoutButton();
        }

        public static void UserClicksOnLinkAccountButton(IWebDriver driver)
        {
            var page = new Home(driver);
            page.ClickLinkAccountButton();
        }

        public static void UserClicksOnAddminManagementButton(IWebDriver driver)
        {
            var page = new Home(driver);
            page.ClickAdminManagementButton();
        }
    }
}
