using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Customer
{
    [TestClass]
    public class TestCustomerRepair
    {
        private IWebDriver driver;
        private CustomerPageRepair customer;
        private WebDriverWait wait;

        [TestInitialize] // chạy trước mỗi test
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost/qlks-ngan_hn/source/login.php");
            driver.FindElement(By.Id("username")).SendKeys("admin");
            driver.FindElement(By.Id("password")).SendKeys("admin123");
            driver.FindElement(By.CssSelector("button.w-100.btn.btn-lg.btn-primary")).Click();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));


            customer = new CustomerPageRepair(driver);

        }
        [TestMethod]
        public void TestEditCustomer1()
        {
            string cccd = "2021050751"; // khách hàng đã tồn tại
            string newFullName = "Nguyễn Văn B";
            string newPhone = "0987654321";
            string newEmail = "vanb@gmail.com";
            string newAddress = "456 Lê Lợi, TP.HCM";

            // Mở form sửa khách hàng
            customer.OpenEditCustomerModal(cccd);

            // Cập nhật khách hàng
            customer.EditCustomer(cccd, newFullName, newPhone, newEmail, newAddress);

            var successElement = wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(By.CssSelector("div.alert.alert-success"));
                    return element.Displayed ? element : null;
                }
                catch (NoSuchElementException) { return null; }
            });

            Assert.IsNotNull(successElement);
            Assert.AreEqual("Cập nhật thông tin khách hàng thành công!", successElement.Text.Trim());

            // Kiểm tra thông tin đã cập nhật
            var details = customer.GetCustomerDetails(cccd);
            Assert.AreEqual(newFullName, details.FullName);
            Assert.AreEqual(newPhone, details.Phone);
            Assert.AreEqual(newEmail, details.Email);
            Assert.AreEqual(newAddress, details.Address);
        }

        // CCCD trùng, hợp lệ
        [TestMethod]
    
        public void TestEditCustomer2_TrungCCCD()
        {
            string oldCCCD = "2021050751";
            string newCCCD = "202105075111"; // đã tồn tại

            // Mở form sửa khách hàng
            customer.OpenEditCustomerModal(oldCCCD);

            // Thử đổi CCCD sang đã có
            customer.EditCustomer(newCCCD, "Nguyễn Văn A", "0912345678", "test@gmail.com", "ABC");

            var errorElement = wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return element.Displayed ? element : null;
                }
                catch (NoSuchElementException) { return null; }
            });

            Assert.AreEqual("CCCD đã tồn tại trong hệ thống.", errorElement.Text.Trim());
        }

      
        //bỏ trống CCCD
        [TestMethod]
        public void TestEditCustomer3_BoTrongHoTen()
        {
            string cccd = "2021050751";
            string emptyName = "";
            string phone = "0912345678";
            string email = "abc@gmail.com";
            string address = "XYZ";

            // Mở form sửa
            customer.OpenEditCustomerModal(cccd);

            // Nhập họ tên rỗng
            customer.EditCustomer(cccd, emptyName, phone, email, address);

            var errorElement = wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return element.Displayed ? element : null;
                }
                catch (NoSuchElementException) { return null; }
            });

            Assert.AreEqual("Vui lòng nhập họ tên.", errorElement.Text.Trim());
        }

        [TestMethod]
        //CCCD =< 10 
  
        public void TestEditCustomer4_CCCDKhongHopLe()
        {
            string cccd = "2021"; // Không hợp lệ
            string fullName = "Nguyễn Văn C";
            string phone = "0912345678";
            string email = "abc@gmail.com";
            string address = "XYZ";

            // Mở form sửa
            customer.OpenEditCustomerModal(cccd);

            customer.EditCustomer(cccd, fullName, phone, email, address);

            var errorElement = wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return element.Displayed ? element : null;
                }
                catch (NoSuchElementException) { return null; }
            });

            Assert.AreEqual("CCCD không hợp lệ.", errorElement.Text.Trim());
        }

    }
}
