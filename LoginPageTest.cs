using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;



namespace DATN
{
    public class LoginPageTest
    {
        private IWebDriver driver;

        public LoginPageTest(IWebDriver browser)
        {
            driver = browser;
        }
        // Text username
        private By username = By.Id("username");
        private IWebElement Username => driver.FindElement(username);
        //Text password

        private By password = By.Id("password");
        private IWebElement Password => driver.FindElement(password);
        // button Login

        private By loginClick = By.CssSelector("button.w-100.btn.btn-lg.btn-primary");
        private IWebElement BtnLoginClick => driver.FindElement(loginClick);


        public void LoginApplication(string username, string password)
        {
            Username.Clear(); // Clear any pre-existing value
            Password.Clear(); // Clear any pre-existing value
            Username.SendKeys(username);
            Password.SendKeys(password);
            BtnLoginClick.Click();
        }
    }
}

