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
           
            string cccd = "111111111117";        // CCCD khách hàng đã có
            string roomId = "245";               // ID phòng chọn
            string roomNumber = "12";            // Số phòng để kiểm tra lại
            DateTime checkIn = new DateTime(2025, 7, 18);
            DateTime checkOut = new DateTime(2025, 7, 19);
            int countBefore = book.CountAllBookings();
            book.OpenAddBookingSection();
            book.AddBooking(cccd, roomId, checkIn, checkOut, "cash");

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

            string successText = successElement.Text.Trim();
            Assert.IsTrue(successText.Contains("Đặt phòng thành công!"), $"Nội dung thông báo không đúng: {successText}");
            Console.WriteLine($"Thông báo: {successText}");

            bool exists = book.IsBookingExists(roomNumber, cccd, checkIn, checkOut);
            Assert.IsTrue(exists, $"Không tìm thấy đơn đặt phòng [{roomNumber}] cho khách [{cccd}] từ {checkIn:dd/MM/yyyy} đến {checkOut:dd/MM/yyyy}.");
            Console.WriteLine("Đặt phòng đã được tạo và hiển thị trong danh sách.");


            var details = book.GetBookingDetails(roomNumber, cccd, checkIn.ToString("dd/MM/yyyy"), checkOut.ToString("dd/MM/yyyy"));
            Assert.IsNotNull(details, "Không lấy được thông tin chi tiết đặt phòng.");

            Assert.AreEqual(cccd, details.CCCD, "CCCD không đúng.");
            Assert.AreEqual(roomNumber, details.RoomNumber, "Số phòng không đúng.");
            Assert.AreEqual("Standard", details.RoomType, "Loại phòng không đúng.");  // sửa nếu bạn test loại khác
            Assert.AreEqual("15/07/2025", details.CheckInDate, "Ngày nhận không đúng.");
            Assert.AreEqual("17/07/2025", details.CheckOutDate, "Ngày trả không đúng.");
            Assert.AreEqual("Đã đặt", details.Status, "Trạng thái không đúng."); // tuỳ hệ thống
        }
        [TestMethod]
        public void TestBooking2()
        {
            string cccd = "";        // CCCD khách hàng đã có
            string roomId = "245";               // ID phòng chọn
            string roomNumber = "12";
            int countBefore = book.CountAllBookings();// Số phòng để kiểm tra lại
            DateTime checkIn = new DateTime(2025, 7, 15);
            DateTime checkOut = new DateTime(2025, 7, 17);

            book.OpenAddBookingSection();
            book.AddBooking(cccd, roomId, checkIn, checkOut, "cash", expectError: true);



            var message = driver.FindElement(By.Id("cccd")).GetAttribute("validationMessage");
            Assert.IsTrue(message.Contains("Please select an item in the list."), "Không báo lỗi required ở trường khách hàng.");
            // Kiểm tra form vẫn mở (modal chưa đóng lại)
            bool isModalStillOpen = driver.FindElement(By.Id("addBookingModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            // Kiểm tra số lượng khách hàng không thay đổi
            int countAfter = book.CountAllBookings();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có khách hàng nào được thêm vào hệ thống -> Pass");
        }
        [TestMethod]
        public void TestBooking3()
        {
            string cccd = "111111111117";      // CCCD hợp lệ
            string roomId = "245";             // ID phòng hợp lệ
            string roomNumber = "12";
            int countBefore = book.CountAllBookings();

            DateTime? checkIn = null; // Bỏ trống ngày nhận
            DateTime? checkOut = new DateTime(2025, 7, 17);

            book.OpenAddBookingSection();
            book.AddBooking(cccd, roomId, checkIn, checkOut, "cash", expectError: true);

            // Kiểm tra hiển thị lỗi required
            var message = driver.FindElement(By.Id("check_in")).GetAttribute("validationMessage");
            Assert.IsTrue(message.Contains("Please fill out this field"), "Không báo lỗi required ở trường ngày nhận.");

            // Kiểm tra form vẫn mở
            bool isModalStillOpen = driver.FindElement(By.Id("addBookingModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");

            // Kiểm tra không có dữ liệu mới được thêm
            int countAfter = book.CountAllBookings();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm dữ liệu dù thiếu ngày nhận");

            Console.WriteLine("Trường hợp để trống ngày nhận -> Pass");
        }

        [TestMethod]
        public void TestBooking4()
        {
            string cccd = "111111111117";      // CCCD hợp lệ
            string roomId = "245";             // ID phòng hợp lệ
            string roomNumber = "12";
            int countBefore = book.CountAllBookings();

            DateTime? checkIn = new DateTime(2025, 7, 17);  // Bỏ trống ngày nhận
            DateTime? checkOut = null;

            book.OpenAddBookingSection();
            book.AddBooking(cccd, roomId, checkIn, checkOut, "cash", expectError: true);

            // Kiểm tra hiển thị lỗi required
            var message = driver.FindElement(By.Id("check_out")).GetAttribute("validationMessage");
            Assert.IsTrue(message.Contains("Please fill out this field"), "Không báo lỗi required ở trường ngày nhận.");

            // Kiểm tra form vẫn mở
            bool isModalStillOpen = driver.FindElement(By.Id("addBookingModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");

            // Kiểm tra không có dữ liệu mới được thêm
            int countAfter = book.CountAllBookings();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm dữ liệu dù thiếu ngày trả");

            Console.WriteLine("Trường hợp để trống ngày trả -> Pass");
        }
        [TestMethod]
        public void TestBooking5()
        {
            string cccd = "111111111117";        // CCCD khách hàng đã có
            string roomId = "";               // ID phòng chọn
            string roomNumber = "";
            int countBefore = book.CountAllBookings();// Số phòng để kiểm tra lại
            DateTime checkIn = new DateTime(2025, 3, 15);
            DateTime checkOut = new DateTime(2025, 4, 17);

            book.OpenAddBookingSection();
            book.AddBooking(cccd, roomId, checkIn, checkOut, "cash", expectError: true);



            var message = driver.FindElement(By.Id("room_id")).GetAttribute("validationMessage");
            Assert.IsTrue(message.Contains("Please select an item in the list"), "Không báo lỗi required ở trường phòng.");
            // Kiểm tra form vẫn mở (modal chưa đóng lại)
            bool isModalStillOpen = driver.FindElement(By.Id("addBookingModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            // Kiểm tra số lượng khách hàng không thay đổi
            int countAfter = book.CountAllBookings();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có khách hàng nào được thêm vào hệ thống -> Pass");
        }
        [TestMethod]
        public void TestBooking6()
        {
            string cccd = "111111111117";        // CCCD khách hàng đã có
            string roomId = "12";               // ID phòng chọn
            string roomNumber = "1021";
            int countBefore = book.CountAllBookings();// Số phòng để kiểm tra lại
            DateTime checkIn = new DateTime(2025, 7, 16);
            DateTime checkOut = new DateTime(2025, 7, 17);

            book.OpenAddBookingSection();
            book.AddBooking(cccd, roomId, checkIn, checkOut, "cash", expectError: true);
            var successElement = wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return element.Displayed ? element : null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
            });

            string successText = successElement.Text.Trim();
            Assert.IsTrue(successText.Contains("Vui lòng nhập giá trị hợp lệ."), $"Nội dung thông báo không đúng: {successText}");
            Console.WriteLine($"Thông báo: {successText}");

        }
        [TestMethod]
        public void TestBooking7()
        {
            string cccd = "111111111117";        // CCCD khách hàng đã có
            string roomId = "245";               // ID phòng chọn
            string roomNumber = "12";
            int countBefore = book.CountAllBookings();// Số phòng để kiểm tra lại
            DateTime checkIn = new DateTime(2025, 7, 25);
            DateTime checkOut = new DateTime(2025, 7, 27);

            book.OpenAddBookingSection();
            book.AddBooking(cccd, roomId, checkIn, checkOut, "cash", expectError: true);
            var successElement = wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return element.Displayed ? element : null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
            });

            string successText = successElement.Text.Trim();
            Assert.IsTrue(successText.Contains("Phòng đã được đặt trong khoảng thời gian này"), $"Nội dung thông báo không đúng: {successText}");
            Console.WriteLine($"Thông báo: {successText}");

           ;

        }



    }
}
