using DATN.BookingRoom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Interactions;

namespace DATN.CheckIn_CheckOut
{
    [TestClass]
    public class TestCheckIn
    {
        private IWebDriver driver;
        private CheckInPage check;
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
            check = new CheckInPage(driver);
        }
        [TestMethod]
        // Phòng không hợp lệ để nhận (nhận trong khoảng thời gian phòng đang dược sử dụng)
        public void TestCheckIn1()
        {
            string roomNumber = "107";
            string customerName = "Ngô Thị H";
            DateTime checkInDate = new DateTime(2025, 5, 8);

            check.OpenCheckInPage();
            var row = check.FindCheckInRowByInfo(roomNumber, customerName, checkInDate);
            Assert.IsNotNull(row, "Không tìm thấy dòng đặt phòng phù hợp.");

            var checkinButton = row.FindElement(By.CssSelector("button[name='checkin']"));
            checkinButton.Click();

            // Xử lý xác nhận popup
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.AlertIsPresent());
            IAlert confirmAlert = driver.SwitchTo().Alert();
            string confirmText = confirmAlert.Text;
            Console.WriteLine("Alert hiển thị với nội dung: " + confirmText);
            Assert.IsTrue(confirmText.Contains("Xác nhận nhận phòng"), "Nội dung xác nhận KHÔNG đúng.");
            confirmAlert.Accept();

            // Đợi thông báo 
            var alertLocator = By.CssSelector(".alert-success, .toast-success, .alert");
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(alertLocator));
            var alertElement = driver.FindElement(alertLocator);
            string alertText = alertElement.Text;

            Assert.IsTrue(alertText.Contains("Không thể nhận phòng. Phòng đang được sử dụng hoặc đang bảo trì."),
                $"Thông báo không đúng hoặc không hiển thị: {alertText}");
        }
        [TestMethod]
        public void TestCheckIn2()
        {
            string roomNumber = "302";
            string customerName = "Châu Văn P";
            DateTime checkInDate = new DateTime(2025, 6, 29);

            check.OpenCheckInPage();
            var row = check.FindCheckInRowByInfo(roomNumber, customerName, checkInDate);
            Assert.IsNotNull(row, "Không tìm thấy dòng đặt phòng phù hợp.");

            var checkinButton = row.FindElement(By.CssSelector("button[name='checkin']"));

            // Di chuyển chuột đến button trước khi click
            Actions actions = new Actions(driver);
            actions.MoveToElement(checkinButton).Click().Perform();

            // Xử lý xác nhận popup
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.AlertIsPresent());
            IAlert confirmAlert = driver.SwitchTo().Alert();
            string confirmText = confirmAlert.Text;
            Console.WriteLine("Alert hiển thị với nội dung: " + confirmText);
            Assert.IsTrue(confirmText.Contains("Xác nhận nhận phòng"), "Nội dung xác nhận KHÔNG đúng.");
            confirmAlert.Accept();

            // Đợi thông báo 
            var alertLocator = By.CssSelector(".alert-success, .toast-success, .alert");
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(alertLocator));
            var alertElement = driver.FindElement(alertLocator);
            string alertText = alertElement.Text;

            Assert.IsTrue(alertText.Contains("Nhận phòng thành công!"),
                $"Thông báo không đúng hoặc không hiển thị: {alertText}");

            // Mở trang danh sách đặt phòng và kiểm tra trạng thái
            check.OpenBookingPage(); // Mở trang danh sách đặt phòng       
            var row1 = check.FindBookingRow(roomNumber, customerName, checkInDate, "đã nhận phòng");
            Assert.IsNotNull(row1, "Không tìm thấy dòng đặt phòng sau khi nhận phòng.");

            var cells = row1.FindElements(By.TagName("td"));
            Assert.IsTrue(cells.Count >= 5, "Không đủ cột trong bảng đặt phòng.");
            string status = cells[4].Text.Trim().ToLower();

            Assert.IsTrue(status.Contains("đã nhận phòng") || status.Contains("checked in"),
                $"Trạng thái đặt phòng không đúng sau khi nhận: {status}");

            Console.WriteLine($" Đặt phòng của [{customerName}] phòng [{roomNumber}] đã được nhận đúng => Pass.");
        }
    }
    }


        

 
    
