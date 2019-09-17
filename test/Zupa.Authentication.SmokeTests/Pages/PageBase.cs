using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace Zupa.Authentication.SmokeTests.Pages
{
    public abstract class PageBase
    {
        protected IWebElement GetElement(By by, IWebDriver driver)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                return wait.Until(ExpectedConditions.ElementIsVisible(by));
            }
            catch (WebDriverTimeoutException)
            {
                return null;
            }
        }
    }
}