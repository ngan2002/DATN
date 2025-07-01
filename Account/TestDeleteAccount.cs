using DATN.Rooms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;

namespace DATN.Account
{
    [TestClass]
    public class TestDeleteAccount
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private AccountDelete account;

        [TestInitialize]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost/qlks-ngan_hn/source/login.php");

            // Đăng nhập
            driver.FindElement(By.Id("username")).SendKeys("admin");
            driver.FindElement(By.Id("password")).SendKeys("admin123");
            driver.FindElement(By.CssSelector("button.w-100.btn.btn-lg.btn-primary")).Click();

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            account = new AccountDelete(driver);
        }
        [TestMethod]
        public void DeleteAccount1()
        {
            string username = "NV12";
            //var detailsBefore = account.GetUserDetails(username);
            //Assert.IsNotNull(detailsBefore, "Không tìm thấy tài khoản trước khi sửa");
            //// Đảm bảo tài khoản tồn tại trước khi xóa
            //bool isExistBefore = account.IsUserExists(username);
            //Assert.IsTrue(isExistBefore, $"Tài khoản [{username}] không tồn tại trước khi xóa.");

            //// Thực hiện xóa
            //account.DeleteUser(username);

            //// Kiểm tra lại sau khi xóa
            //bool isExistAfter = account.IsUserExists(username);
            //Assert.IsFalse(isExistAfter, $"Tài khoản [{username}] vẫn tồn tại sau khi xóa.");

            //Console.WriteLine($"Đã xóa thành công tài khoản [{username}].");
        }

    }

}
