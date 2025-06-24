using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Diagnostics;
using System.Security.Policy;
using OpenQA.Selenium.Support.UI;

namespace DATN
{
    [TestClass]
    public class LoginTest
    {
        
        private IWebDriver driver;
        private LoginPageTest login;

        [TestInitialize] // chạy trước mỗi test
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost/qlks-ngan_hn/source/login.php");
            login = new LoginPageTest(driver);

        }
        [TestMethod]
        public void TestLogin1()
        {
            login.LoginApplication("admin", "admin123");
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => driver.Url.Contains("index.php"));

            // Kiểm tra URL chuyển hướng đến trang chính sau khi đăng nhập thành công
            Assert.IsTrue(
                driver.Url.Contains("http://localhost/qlks-ngan_hn/source/index.php"),
                "Đăng nhập thất bại: Không chuyển hướng đến trang chính"
            );

            Console.WriteLine("Đăng nhập thành công như mong đợi");
        }

        [TestMethod]
        public void TestLogin2()
        {
            login.LoginApplication("admin", "ad");
            // Tìm phần tử hiển thị lỗi (từ selector)
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var errorElement = driver.FindElement(By.CssSelector("div.alert.alert-danger"));

            // In ra kết quả kiểm tra
            if (errorElement.Text.Contains("Mật khẩu không đúng"))
            {
                Console.WriteLine("Đăng nhập thất bại đúng như mong đợi (sai mật khẩu)");
            }
            else
            {
                Console.WriteLine("Đăng nhập sai nhưng không hiển thị lỗi đúng");
            }

            // Kiểm tra bắt buộc: lỗi phải xuất hiện
            Assert.IsTrue(errorElement.Text.Contains("Mật khẩu không đúng"), "Không thấy thông báo lỗi khi nhập sai mật khẩu");
        }
        [TestMethod]
        public void TestLogin3()
        {
            login.LoginApplication("admin", "");

            var PassInput = driver.FindElement(By.Id("password"));
            string validationMessage = PassInput.GetAttribute("validationMessage");
            // Thông báo
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Thông báo lỗi: {validationMessage}");
            if (driver.Url.Contains("http://localhost/qlks-ngan_hn/source/login.php"))
            {
                // Nếu vẫn ở URL cũ, kiểm tra cho trường hợp thành công
                Assert.IsTrue(true, "Đăng nhập thất bại (trường pass trống), vẫn ở trang cũ => case thành công");
            }
            else
            {
                // Nếu URL thay đổi, có nghĩa là đăng nhập thành công, và báo lỗi
                Assert.Fail("Đăng nhập thành công => case thất bại");
            }

            Console.WriteLine("Đăng nhập thất bại đúng như mong đợi (password để trống)");


        }
        [TestMethod]
        public void TestLogin4()
        {
            login.LoginApplication("ad", "admin123");
            var errorElement = driver.FindElement(By.CssSelector("div.alert.alert-danger"));

            if (errorElement.Text.Contains("Tên đăng nhập không tồn tại"))
            {
                Console.WriteLine("Đăng nhập thất bại đúng như mong đợi (sai username)");
            }
            else
            {
                Console.WriteLine("Đăng nhập sai nhưng không hiển thị lỗi đúng");
            }

            // Sửa lại phần kiểm tra này
            Assert.IsTrue(errorElement.Text.Contains("Tên đăng nhập không tồn tại"), "Không thấy thông báo lỗi khi nhập sai username");
        }
        [TestMethod]
        public void TestLogin5()
        {
            login.LoginApplication("", "hiiii");


            var PassInput = driver.FindElement(By.Id("username"));
            string validationMessage = PassInput.GetAttribute("validationMessage");
            // Thông báo
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Thông báo lỗi: {validationMessage}");
            if (driver.Url.Contains("http://localhost/qlks-ngan_hn/source/login.php"))
            {
                // Nếu vẫn ở URL cũ, kiểm tra cho trường hợp thành công
                Assert.IsTrue(true, "Đăng nhập thất bại (trường username trống), vẫn ở trang cũ => case thành công");
            }
            else
            {
                // Nếu URL thay đổi, có nghĩa là đăng nhập thành công, và báo lỗi
                Assert.Fail("Đăng nhập thành công => case thất bại");
            }

            Console.WriteLine("Đăng nhập thất bại đúng như mong đợi (username để trống)");

        }

        //[TestCleanup]
        //public void Cleanup()
        //{
        //    driver.Quit();
        //}
    }
}

