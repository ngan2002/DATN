using DATN.Rooms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DATN.BookingRoom
{
    [TestClass]
    public class TestAddRoom
    {
        private IWebDriver driver;
        private BookingPage book;
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
            book = new BookingPage(driver);
        }
        [TestMethod]

        public void TestBooking1()
        {
            // Thiết lập dữ liệu đầu vào
            string cccd = "456789012345"; // CCCD khách có thật
            List<string> roomNumbers = new List<string> { "201", "test12" }; // Số phòng

            DateTime checkInDate = DateTime.Today.AddDays(1);
            DateTime checkOutDate = DateTime.Today.AddDays(3);

            // Mở trang đặt phòng và thêm mới
            book.AddBooking(cccd, roomNumbers, checkInDate, checkOutDate, paymentMethod: "cash");

            // Xác minh thông báo thành công
            var successElement = wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(By.CssSelector("div.alert.alert-success"));
                    return element.Displayed ? element : null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
            });

            Assert.IsNotNull(successElement, "Không tìm thấy thông báo thành công.");

            string expectedMessage = $"Đặt thành công {roomNumbers.Count} phòng.";
            Assert.AreEqual(expectedMessage, successElement.Text.Trim(), "Thông báo thành công không đúng.");
            Console.WriteLine("Thông báo thành công: " + successElement.Text.Trim());

            // Kiểm tra từng phòng có xuất hiện đúng trong danh sách
            foreach (var roomNumber in roomNumbers)
            {
                var found = book.IsBookingExists(
                    roomNumber,
                    cccd,
                    checkInDate,
                    checkOutDate
                );

                Assert.IsTrue(found, $"Không tìm thấy phòng [{roomNumber}] trong danh sách đặt phòng.");
                Console.WriteLine($" Đã tìm thấy phòng [{roomNumber}] cho khách [{cccd}] từ {checkInDate:dd/MM/yyyy} đến {checkOutDate:dd/MM/yyyy}");
            }
        }


    }
}
