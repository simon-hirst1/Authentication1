using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace Zupa.Authentication.SmokeTests.Pages
{
    public class Home : PageBase
    {
        private readonly IWebDriver _driver;

        public Home(IWebDriver driver)
        {
            _driver = driver;
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".navbar-nav")));
        }

        public IWebElement LoginLink => GetElement(By.Id("login"), _driver);

        public IWebElement LogoutButton => GetElement(By.Id("logout"), _driver);

        public void ClickLoginLink()
        {
            if (!LoginLink.Displayed)
                Logout();

            LoginLink.Click();
        }

        public void Login(string email, string password)
        {
            LoginLink.Click();
            EnterEmail(email);
            EnterPassword(password);
            ClickSubmitButton();
        }

        public void Logout()
        {
            if (LogoutButton?.Displayed == true)
                LogoutButton.Click();
        }

        public void EnterEmail(string emailAddress)
        {
            GetElement(By.Id("Email"), _driver).SendKeys(emailAddress);
        }

        public void EnterPassword(string password)
        {
            GetElement(By.Id("Password"), _driver).SendKeys(password);
        }

        public void ClickSubmitButton()
        {
            GetElement(By.Id("submit"), _driver).Click();
        }

        public void ClickLogoutButton()
        {
            LogoutButton.Click();
        }

        public void ClickRegisterLink()
        {
            GetElement(By.Id("register"), _driver).Click();
        }

        public void ClickProviderButton(string providerName)
        {
            GetElement(By.Id(providerName), _driver).Click();
        }

        public void ClickLinkAccountButton()
        {
            GetElement(By.Id("link"), _driver).Click();
        }

        public void ClickAdminManagementButton()
        {
            GetElement(By.Id("admin"), _driver).Click();
        }
    }
}
