using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace Zupa.Authentication.SmokeTests.Pages
{
    public class AdminManagement : PageBase
    {
        private readonly IWebDriver _driver;

        public AdminManagement(IWebDriver driver)
        {
            _driver = driver;
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".navbar-nav")));
        }
        
        public void ClickAddClientButton()
        {
            GetElement(By.Id("add_client_button"), _driver).Click();
        }

        public void ClickAddTestClientButton()
        {
            GetElement(By.Id("add_test_client_button"), _driver).Click();
        }

        public void ClickAddApiResourceButton()
        {
            GetElement(By.Id("add_api_resource_button"), _driver).Click();
        }
    }
}
