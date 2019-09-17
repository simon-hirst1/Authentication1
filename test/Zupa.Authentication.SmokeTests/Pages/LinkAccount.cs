using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace Zupa.Authentication.SmokeTests.Pages
{
    public class LinkAccount : PageBase
    {
        private readonly IWebDriver _driver;

        public LinkAccount(IWebDriver driver)
        {
            _driver = driver;
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".navbar-nav")));
        }

        public void ClickProviderButton(string providerName)
        {
            GetElement(By.Id(providerName), _driver).Click();
        }

        public void ClickCreatePasswordButton()
        {
            GetElement(By.Id("change_password_button"), _driver).Click();
        }
    }
}