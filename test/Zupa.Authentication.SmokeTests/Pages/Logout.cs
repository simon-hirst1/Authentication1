using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace Zupa.Authentication.SmokeTests.Pages
{
    public class Logout : PageBase
    {
        private readonly IWebDriver _driver;

        public Logout(IWebDriver driver)
        {
            _driver = driver;
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".navbar-nav")));
        }

        public void ClickLogoutButton()
        {
            GetElement(By.Id("logout-btn"), _driver).Click();
        }
    }
}