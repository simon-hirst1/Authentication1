using OpenQA.Selenium;
using Zupa.Authentication.SmokeTests.Pages;

namespace Zupa.Authentication.SmokeTests.TestSteps
{
    public static class Given
    {
        public static void UserIsNotLoggedIn(IWebDriver driver)
        {
            var page = new Home(driver);
            page.Logout();
        }

        public static void UserIsLoggedIn(IWebDriver driver, string email, string password)
        {
            var page = new Home(driver);
            if (page.LogoutButton == null)
                page.Login(email, password);
        }
    }
}
