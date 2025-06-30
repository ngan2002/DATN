using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Diagnostics;
using System.Security.Policy;
using OpenQA.Selenium.Support.UI;
using System.Linq;
using System.Threading;
using OpenQA.Selenium.Interactions;
using System.Net.NetworkInformation;
using static DATN.Rooms.RoomPage;
using SeleniumExtras.WaitHelpers;
using static DATN.Rooms.RoomPageRepair;
namespace DATN.Rooms
{
    [TestClass]
    public class TestAddRoom
    {
        private IWebDriver driver;
        private RoomPage room;
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


            room = new RoomPage(driver);

        }
        [TestMethod]
        public void TestRoom1()
        {
            // Thiết lập dữ liệu đầu vào
            string roomNumber = "21211";          
            string roomType = "VIP";           
            string price = "1500000";           
            string status = "Trống";                  
        
            room.RoomApplication(roomNumber, roomType, price, status);
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

            // Kiểm tra thông báo có nội dung thành công đúng
            Assert.AreEqual("Thêm phòng thành công!", successElement.Text.Trim(), "Nội dung thông báo thành công không đúng.");
            Console.WriteLine($"Thông báo hiển thị thành công là {successElement.Text.Trim()}");
            // Kiểm tra sự tồn tại của phòng vừa thêm
            Assert.IsTrue(room.IsRoomExists(roomNumber), $" Không tìm thấy phòng [{roomNumber}] trong danh sách.");
                Console.WriteLine($" Phòng [{roomNumber}] đã xuất hiện trong danh sách.");

                // Kiểm tra thông tin chi tiết phòng đã thêm
                var details = room.GetRoomDetails(roomNumber);
                Assert.IsNotNull(details, "Không lấy được thông tin chi tiết của phòng.");

                Assert.AreEqual(roomNumber, details.RoomNumber, " Số phòng không khớp.");
                Assert.AreEqual(roomType, details.RoomType, " Loại phòng không khớp.");
                Assert.AreEqual(price, details.Price.Replace(".", ""), " Giá phòng không khớp."); // Nếu giá hiển thị có dấu chấm
                Assert.AreEqual(status, details.Status, " Trạng thái phòng không khớp.");

                // Kiểm tra phòng có xuất hiện trong combobox đặt phòng
                room.OpenBookingPage();
                var select = wait.Until(d => d.FindElement(By.Id("room_id")));
                var options = new SelectElement(select).Options;
                bool existsInDropdown = options.Any(o => o.Text.Trim().Contains(roomNumber));
                Assert.IsTrue(existsInDropdown, $"Phòng [{roomNumber}] không xuất hiện trong combobox đặt phòng.");
                Console.WriteLine("TestRoom1: Passed - Thêm phòng thành công và hiển thị đầy đủ.");
            }
        


        [TestMethod]
        // giá = 0
        public void TestRoom2()
            
        {
            int countBefore = room.CountAllRooms();
            string roomNumber = "11cde";
            room.RoomApplication(roomNumber, "VIP", "0", "Trống");
            try
            {
                var errorElement = wait.Until(drv =>
                {
                    var el = drv.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return el.Displayed ? el : null;
                });

                Assert.IsTrue(errorElement.Text.Contains("Giá trị nhập không hợp lệ"));
                Console.WriteLine($"Thông báo lỗi hiển thị: {errorElement.Text}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi với giá phòng = 0.");
            }
            int countAfter = room.CountAllRooms();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có phòng nào được thêm vào hệ thống -> Pass");

            room.OpenBookingPage();
            // Kiểm tra phòng trong combobox
            var roomSelectBooking = wait.Until(d => d.FindElement(By.Id("room_id")));
            var options = new SelectElement(roomSelectBooking).Options;
       
            bool existsInDropdown = options.Any(o => o.Text.Trim().Contains(roomNumber));
            Assert.IsFalse(existsInDropdown, $"Phòng [{roomNumber}] xuất hiện trong combobox đặt phòng");
            Console.WriteLine($"Phòng [{roomNumber}] không xuất hiện trong list phòng để đặt");

            Console.WriteLine("TestRoom2: Passed - Không thể thêm phòng nếu giá phòng = 0");
          
        }
        [TestMethod]
        public void TestRoom3()
        {
            int countBefore = room.CountAllRooms();
            // Nhập các ô hợp lệ trừ giá phòng âm
            string roomNumber = "11cde";
            room.RoomApplication(roomNumber, "VIP", "-1849324", "Trống");
            var giaPhongInput = driver.FindElement(By.Id("price"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");
            // Thông báo
            Assert.AreEqual("Value must be greater than or equal to 0.", validationMessage);
            Console.WriteLine($"Thông báo lỗi: {validationMessage}");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");


            int countAfter = room.CountAllRooms();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có phòng nào được thêm vào hệ thống -> Pass");
            Console.WriteLine("TestRoom3: Passed - Không thể thêm phòng nếu giá phòng < 0");
        }
        [TestMethod]
        // giá > 100.000.000
        public void TestRoom4()
        {
            int countBefore = room.CountAllRooms();
            // Nhập các ô hợp lệ trừ giá phòng > 10000000
            string roomNumber = "11cde";
            room.RoomApplication(roomNumber, "VIP", "1000000000000000000000000000", "Trống");
            try
            {
                var errorElement = wait.Until(drv =>
                {
                    var el = drv.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return el.Displayed ? el : null;
                });

                Assert.IsTrue(errorElement.Text.Contains("Giá trị nhập không hợp lệ"));
                Console.WriteLine($"Thông báo lỗi hiển thị: {errorElement.Text}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi với giá phòng > 100.000.000.");
            }
            int countAfter = room.CountAllRooms();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có phòng nào được thêm vào hệ thống -> Pass");

            Console.WriteLine("TestRoom4: Passed - Không thể thêm phòng nếu giá phòng > 100.000.000");
        }
        // giá bỏ trống
        [TestMethod]
        public void TestRoom5()
        {
            int countBefore = room.CountAllRooms();
            string roomNumber = "11xyz";
            room.RoomApplication(roomNumber, "VIP", "", "Trống");
            var giaPhongInput = driver.FindElement(By.Id("price"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");

            // Kiểm tra thông báo lỗi có đúng như mong đợi
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Giá phòng hiển thị thông báo lỗi: [{validationMessage}]");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");


            int countAfter = room.CountAllRooms();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có phòng nào được thêm vào hệ thống -> Pass");
            Console.WriteLine("TestRoom5: Passed - Không thể thêm phòng nếu giá phòng bỏ trống.");
        }
        [TestMethod]
        public void TestRoom6()
        {
            int countBefore = room.CountAllRooms();
            string roomNumber = "11cde";
            room.RoomApplication(roomNumber, "VIP", "test", "Trống");
            // Tìm lại input và lấy thông báo lỗi
            var giaPhongInput = driver.FindElement(By.Id("price"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");

            // Kiểm tra thông báo lỗi có đúng như mong đợi
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Giá phòng hiển thị thông báo lỗi: [{validationMessage}]");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");


            int countAfter = room.CountAllRooms();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có phòng nào được thêm vào hệ thống -> Pass");

            Console.WriteLine("TestRoom6: Passed - Không thể thêm phòng nếu giá phòng kí tự không phải số");
        }
        /// <summary>
        /// Loại phòng trống
        /// </summary>
        [TestMethod]
        public void TestRoom7()
        {
            int countBefore = room.CountAllRooms();
            string roomNumber = "hi12";
            room.RoomApplication(roomNumber, "Chọn loại phòng", "12222224", "Trống");

            var LoaiPhongSelect = driver.FindElement(By.Id("type"));
            string validationMessage = LoaiPhongSelect.GetAttribute("validationMessage");
            // Kiểm tra xem có đúng thông báo mong đợi không
            Assert.AreEqual("Please select an item in the list.", validationMessage);
            Console.WriteLine($"Loại phòng hiển thị thông báo lỗi [{validationMessage}] ");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");


            int countAfter = room.CountAllRooms();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có phòng nào được thêm vào hệ thống -> Pass");
            Console.WriteLine("TestRoom7: Passed - Không thể thêm phòng nếu loại phòng trống");
        }


        // Số phòng trống
        [TestMethod]
        public void TestRoom8()
        {
            // Nhập các ô hợp lệ trừ ô Số Phòng (để trống)
            int countBefore = room.CountAllRooms();
            string roomNumber = "";
            room.RoomApplication(roomNumber, "VIP", "1500000", "Trống");
           
            var giaPhongInput = driver.FindElement(By.Id("room_number"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");
            // Kiểm tra xem có đúng thông báo mong đợi không
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Số phòng hiển thị thông báo lỗi [{validationMessage}] ");

            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");


            int countAfter = room.CountAllRooms();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có phòng nào được thêm vào hệ thống -> Pass");

            Console.WriteLine("TestRoom8: Passed - Không thể thêm phòng nếu bỏ trống số phòng.");
        }
        /// <summary>
        /// Phòng trùng + loại phòng kh hợp lệ
        /// </summary>
        [TestMethod]
        public void TestRoom9()
        {
            string roomNumber = "1021"; // Phòng đã tồn tại để test
            string expectedMessage = "Phòng đã tồn tại";
            RoomModel roomBefore = room.GetRoomDetails(roomNumber);
            // 1. Thêm phòng trùng
            room.RoomApplication(roomNumber, "Chọn loại phòng", "khkk", "Trống");

            var loaiPhongInput = driver.FindElement(By.Id("edit_type"));
            string validationMessage = loaiPhongInput.GetAttribute("validationMessage");


            // Kiểm tra xem có đúng thông báo mong đợi không
            Assert.AreEqual("Please select an item in the list.", validationMessage);
            Console.WriteLine($"Loại phòng hiển thị thông báo lỗi [{validationMessage}] ");

            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");
            // Đóng form
            driver.FindElement(By.CssSelector("button.btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form thêm phòng.");

            // 2. Kiểm tra xem có hiển thị lỗi phòng trùng không
            bool ErrorDisplayed = false;
            try
            {
                var errorElement = wait.Until(driver =>
                {
                    var elem = driver.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return (elem.Displayed && !string.IsNullOrWhiteSpace(elem.Text)) ? elem : null;
                });

                if (errorElement.Text.Contains(expectedMessage))
                {
                    ErrorDisplayed = true;
                    Console.WriteLine("Tìm thấy lỗi phòng trùng: " + expectedMessage);
                }
                else
                {
                    Console.WriteLine("Có hiển thị lỗi khác, nhưng không phải lỗi phòng trùng.");
                }
            }
            catch (WebDriverTimeoutException)
            {
                ErrorDisplayed = false;
                Console.WriteLine("Không tìm thấy lỗi phòng trùng.");
            }
            // 4. Đảm bảo phòng gốc vẫn tồn tại (không bị ghi đè, xóa)
            RoomModel roomAfter = room.GetRoomDetails(roomNumber);
            Assert.IsNotNull(roomAfter, $"Không tìm thấy phòng [{roomNumber}] sau khi test.");

            Assert.AreEqual(roomBefore.Price, roomAfter.Price, "Giá phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.Status, roomAfter.Status, "Tình trạng phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.RoomType, roomAfter.RoomType, "Loại phòng bị ghi đè thay đổi!");

            // 5. Đảm bảo không có bản ghi trùng
            int roomCount = room.CountRoom(roomNumber);
            Assert.AreEqual(1, roomCount, $"Phòng [{roomNumber}] xuất hiện {roomCount} lần — hệ thống đã cho thêm trùng phòng!");
            Console.WriteLine("Dữ liệu phòng không bị thay đổi -> Không bị ghi đè -> Đúng.");
            Console.WriteLine("TestRoom9: Passed - Ưu tiên kiểm tra lỗi loại phòng trống, không xét đến lỗi trùng phòng.-> phòng kh được thêm  ");
        }
        /// <summary>
        /// Phòng trùng + giá phòng kh hợp lệ
        /// </summary>
        [TestMethod]
        //-> Giá phòng trống 
        public void TestRoom10()
        {
            string roomNumber = "107";
           
            string expectedMessage = "Phòng đã tồn tại";

            RoomModel roomBefore = room.GetRoomDetails(roomNumber);

            // 1. Thử thêm phòng với giá phòng trống
            room.RoomApplication(roomNumber, "VIP", "", "Trống");

            var giaPhongInput = driver.FindElement(By.Id("price"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");
            // Kiểm tra xem có đúng thông báo mong đợi không
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Giá phòng hiển thị thông báo lỗi [{validationMessage}] ");

            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");
            // Đóng form
            driver.FindElement(By.CssSelector("button.btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form thêm phòng.");

            // 2. Kiểm tra xem có hiển thị lỗi phòng trùng không
            bool ErrorDisplayed = false;
            try
            {
                var errorElement = wait.Until(driver =>
                {
                    var elem = driver.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return (elem.Displayed && !string.IsNullOrWhiteSpace(elem.Text)) ? elem : null;
                });

                if (errorElement.Text.Contains(expectedMessage))
                {
                    ErrorDisplayed = true;
                    Console.WriteLine("Tìm thấy lỗi phòng trùng: " + expectedMessage);
                }
                else
                {
                    Console.WriteLine("Có hiển thị lỗi khác, nhưng không phải lỗi phòng trùng.");
                }
            }
            catch (WebDriverTimeoutException)
            {
                ErrorDisplayed = false;
                Console.WriteLine("Không tìm thấy lỗi phòng trùng.");
            }
            // 4. Đảm bảo phòng gốc vẫn tồn tại (không bị ghi đè, xóa)
            RoomModel roomAfter = room.GetRoomDetails(roomNumber);
            Assert.IsNotNull(roomAfter, $"Không tìm thấy phòng [{roomNumber}] sau khi test.");

            Assert.AreEqual(roomBefore.Price, roomAfter.Price, "Giá phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.Status, roomAfter.Status, "Tình trạng phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.RoomType, roomAfter.RoomType, "Loại phòng bị ghi đè thay đổi!");

            // 5. Đảm bảo không có bản ghi trùng
            int roomCount = room.CountRoom(roomNumber);
            Assert.AreEqual(1, roomCount, $"Phòng [{roomNumber}] xuất hiện {roomCount} lần — hệ thống đã cho thêm trùng phòng!");
            Console.WriteLine("Dữ liệu phòng không bị thay đổi -> Không bị ghi đè -> Đúng.");
            Console.WriteLine("TestRoom10: Passed - Ưu tiên kiểm tra lỗi giá phòng để trống, không xét đến lỗi trùng phòng.-> phòng kh được thêm  ");
        }

        [TestMethod]
        //-> Giá phòng nhập chữ
        public void TestRoom11()
        {
            string roomNumber = "107";
            string expectedMessage = "Phòng đã tồn tại";

            RoomModel roomBefore = room.GetRoomDetails(roomNumber);
           
            // 1. Thêm phòng trùng
            room.RoomApplication(roomNumber, "VIP", "test", "Trống");

            var giaPhongInput = driver.FindElement(By.Id("price"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");
            // Kiểm tra xem có đúng thông báo mong đợi không
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Giá phòng hiển thị thông báo lỗi [{validationMessage}] ");

            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");
            // Đóng form
            driver.FindElement(By.CssSelector("button.btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form thêm phòng.");

            // 2. Kiểm tra xem có hiển thị lỗi phòng trùng không
            bool ErrorDisplayed = false;
            try
            {
                var errorElement = wait.Until(driver =>
                {
                    var elem = driver.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return (elem.Displayed && !string.IsNullOrWhiteSpace(elem.Text)) ? elem : null;
                });

                if (errorElement.Text.Contains(expectedMessage))
                {
                    ErrorDisplayed = true;
                    Console.WriteLine("Tìm thấy lỗi phòng trùng: " + expectedMessage);
                }
                else
                {
                    Console.WriteLine("Có hiển thị lỗi khác, nhưng không phải lỗi phòng trùng.");
                }
            }
            catch (WebDriverTimeoutException)
            {
                ErrorDisplayed = false;
                Console.WriteLine("Không tìm thấy lỗi phòng trùng.");
            }
            // 4. Đảm bảo phòng gốc vẫn tồn tại (không bị ghi đè, xóa)
            RoomModel roomAfter = room.GetRoomDetails(roomNumber);
            Assert.IsNotNull(roomAfter, $"Không tìm thấy phòng [{roomNumber}] sau khi test.");

            Assert.AreEqual(roomBefore.Price, roomAfter.Price, "Giá phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.Status, roomAfter.Status, "Tình trạng phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.RoomType, roomAfter.RoomType, "Loại phòng bị ghi đè thay đổi!");

            // 5. Đảm bảo không có bản ghi trùng
            int roomCount = room.CountRoom(roomNumber);
            Assert.AreEqual(1, roomCount, $"Phòng [{roomNumber}] xuất hiện {roomCount} lần — hệ thống đã cho thêm trùng phòng!");
            Console.WriteLine("Dữ liệu phòng không bị thay đổi -> Không bị ghi đè -> Đúng.");
            Console.WriteLine("TestRoom11: Passed - Ưu tiên kiểm tra lỗi giá phòng nhập chữ, không xét đến lỗi trùng phòng.-> phòng kh được thêm  ");
        }
        [TestMethod]
        //-> Giá phòng < 0
        public void TestRoom12()
        {
            string roomNumber = "107";
            string expectedMessage = "Phòng đã tồn tại";

            RoomModel roomBefore = room.GetRoomDetails(roomNumber);

            // 1. Thêm phòng trùng
            room.RoomApplication(roomNumber, "VIP", "-2000", "Trống");

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Tìm lại input và lấy thông báo lỗi
            var giaPhongInput = driver.FindElement(By.Id("price"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");

            // Kiểm tra xem có đúng thông báo mong đợi không
            Assert.AreEqual("Value must be greater than or equal to 0.", validationMessage);
            Console.WriteLine($"Loại phòng hiển thị thông báo lỗi [{validationMessage}] ");

            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");
            // Đóng form
            driver.FindElement(By.CssSelector("button.btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form thêm phòng.");

            // 2. Kiểm tra xem có hiển thị lỗi phòng trùng không
            bool ErrorDisplayed = false;
            try
            {
                var errorElement = wait.Until(driver =>
                {
                    var elem = driver.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return (elem.Displayed && !string.IsNullOrWhiteSpace(elem.Text)) ? elem : null;
                });

                if (errorElement.Text.Contains(expectedMessage))
                {
                    ErrorDisplayed = true;
                    Console.WriteLine("Tìm thấy lỗi phòng trùng: " + expectedMessage);
                }
                else
                {
                    Console.WriteLine("Có hiển thị lỗi khác, nhưng không phải lỗi phòng trùng.");
                }
            }
            catch (WebDriverTimeoutException)
            {
                ErrorDisplayed = false;
                Console.WriteLine("Không tìm thấy lỗi phòng trùng.");
            }
            // 4. Đảm bảo phòng gốc vẫn tồn tại (không bị ghi đè, xóa)
            RoomModel roomAfter = room.GetRoomDetails(roomNumber);
            Assert.IsNotNull(roomAfter, $"Không tìm thấy phòng [{roomNumber}] sau khi test.");

            Assert.AreEqual(roomBefore.Price, roomAfter.Price, "Giá phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.Status, roomAfter.Status, "Tình trạng phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.RoomType, roomAfter.RoomType, "Loại phòng bị ghi đè thay đổi!");

            // 5. Đảm bảo không có bản ghi trùng
            int roomCount = room.CountRoom(roomNumber);
            Assert.AreEqual(1, roomCount, $"Phòng [{roomNumber}] xuất hiện {roomCount} lần — hệ thống đã cho thêm trùng phòng!");
            Console.WriteLine("Dữ liệu phòng không bị thay đổi -> Không bị ghi đè -> Đúng.");
            Console.WriteLine("TestRoom12: Passed - Ưu tiên kiểm tra lỗi giá phòng < 0, không xét đến lỗi trùng phòng.-> phòng kh được thêm  ");
        }
        [TestMethod]
        //-> Giá phòng = 0
        public void TestRoom13()
        {
            string roomNumber = "1021"; // Phòng đã tồn tại
            string expectedErrorPrice = "Giá trị nhập không hợp lệ";

            // 1. Lưu thông tin phòng ban đầu
            RoomModel roomBefore = room.GetRoomDetails(roomNumber);
            Assert.IsNotNull(roomBefore, $"Không tìm thấy phòng [{roomNumber}] để kiểm thử.");

            room.RoomApplication(roomNumber, "VIP", "0", "Trống");

            // 3. Kiểm tra thông báo lỗi giá = 0
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            //Check số  lỗi giá hiển thị
            wait.Until(d => d.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Any(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)));

            var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

            // Kiểm tra có ít nhất 1 lỗi
            if (alerts.Count == 0)
            {
                Assert.Fail("Không có thông báo lỗi nào được hiển thị.");
            }

            // Kiểm tra chỉ có 1 lỗi
            Assert.AreEqual(1, alerts.Count, "Có nhiều hơn 1 thông báo lỗi hiển thị.");

            // Kiểm tra nội dung lỗi đúng
            string actualError = alerts[0].Text.Trim();
            Assert.AreEqual(expectedErrorPrice, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedErrorPrice}\" - Thực tế: \"{actualError}\"");

            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            // 4. Đảm bảo phòng gốc vẫn tồn tại (không bị ghi đè, xóa)
            RoomModel roomAfter = room.GetRoomDetails(roomNumber);
            Assert.IsNotNull(roomAfter, $"Không tìm thấy phòng [{roomNumber}] sau khi test.");

            Assert.AreEqual(roomBefore.Price, roomAfter.Price, "Giá phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.Status, roomAfter.Status, "Tình trạng phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.RoomType, roomAfter.RoomType, "Loại phòng bị ghi đè thay đổi!");

            // 5. Đảm bảo không có bản ghi trùng
            int roomCount = room.CountRoom(roomNumber);
            Assert.AreEqual(1, roomCount, $"Phòng [{roomNumber}] xuất hiện {roomCount} lần — hệ thống đã cho thêm trùng phòng!");

            Console.WriteLine("Phòng không được thêm mới. Dữ liệu gốc không bị thay đổi -> Pass.");
            Console.WriteLine("TestRoom13: Passed - Hệ thống chặn giá phòng = 0, không ghi đè phòng cũ.");
        }

        [TestMethod]
        //-> Giá phòng > 100.000.0000
        public void TestRoom14()
        {
            string roomNumber = "1021";
            string expectedMessage = "Phòng đã tồn tại";

            RoomModel roomBefore = room.GetRoomDetails(roomNumber);
            Assert.IsNotNull(roomBefore, $"Không tìm thấy phòng [{roomNumber}] để kiểm thử.");

            // 1. Thêm phòng trùng
            room.RoomApplication(roomNumber, "VIP", "1000000000000000000000000000", "Trống");

            //Check số  lỗi giá hiển thị
            wait.Until(d => d.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Any(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)));

            var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

            // Kiểm tra có ít nhất 1 lỗi
            if (alerts.Count == 0)
            {
                Assert.Fail("Không có thông báo lỗi nào được hiển thị.");
            }

            // Kiểm tra chỉ có 1 lỗi
            Assert.AreEqual(1, alerts.Count, "Có nhiều hơn 1 thông báo lỗi hiển thị.");

            // Kiểm tra nội dung lỗi đúng
            string actualError = alerts[0].Text.Trim();
            Assert.AreEqual(expectedMessage, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedMessage}\" - Thực tế: \"{actualError}\"");

            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            RoomModel roomAfter = room.GetRoomDetails(roomNumber);
            Assert.IsNotNull(roomAfter, $"Không tìm thấy phòng [{roomNumber}] sau khi test.");

            Assert.AreEqual(roomBefore.Price, roomAfter.Price, "Giá phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.Status, roomAfter.Status, "Tình trạng phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.RoomType, roomAfter.RoomType, "Loại phòng bị ghi đè thay đổi!");

            // 5. Đảm bảo không có bản ghi trùng
            int roomCount = room.CountRoom(roomNumber);
            Assert.AreEqual(1, roomCount, $"Phòng [{roomNumber}] xuất hiện {roomCount} lần — hệ thống đã cho thêm trùng phòng!");

            Console.WriteLine("Phòng không được thêm mới. Dữ liệu gốc không bị thay đổi -> Pass.");
            Console.WriteLine("TestRoom14: Passed - Ưu tiên kiểm tra lỗi giá phòng > 100000000, không xét đến lỗi trùng phòng -> phòng không được thêm  ");
        }
        [TestMethod]
        public void TestRoom15()
        {
            string roomNumber = "1021"; // Phòng đã tồn tại để test
            string expectedMessage = "Phòng đã tồn tại";

            RoomModel roomBefore = room.GetRoomDetails(roomNumber);

            // 1. Thêm phòng trùng
            room.RoomApplication(roomNumber, "VIP", "1500000", "Trống");

            // 3. Kiểm tra thông báo lỗi hiển thị
            bool errorDisplayed = false;

            try
            {
                var errorElement = wait.Until(driver =>
                {
                    var elem = driver.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return (elem.Displayed && !string.IsNullOrWhiteSpace(elem.Text)) ? elem : null;
                });

                string actualMessage = errorElement.Text.Trim();

                if (actualMessage == expectedMessage)
                {
                    errorDisplayed = true;
                    Console.WriteLine("Lỗi đúng mong đợi: " + actualMessage);
                }
                else
                {
                    Console.WriteLine($"Lỗi sai. Mong đợi: \"{expectedMessage}\", Thực tế: \"{actualMessage}\"");
                }
            }
            catch
            {
                Console.WriteLine("Không hiển thị lỗi.");
            }

            Assert.IsTrue(errorDisplayed, "Không hiển thị đúng lỗi mong đợi.");

            RoomModel roomAfter = room.GetRoomDetails(roomNumber);
            Assert.IsNotNull(roomAfter, $"Không tìm thấy phòng [{roomNumber}] sau khi test.");

            Assert.AreEqual(roomBefore.Price, roomAfter.Price, "Giá phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.Status, roomAfter.Status, "Tình trạng phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.RoomType, roomAfter.RoomType, "Loại phòng bị ghi đè thay đổi!");

            // 5. Đảm bảo không có bản ghi trùng
            int roomCount = room.CountRoom(roomNumber);
            Assert.AreEqual(1, roomCount, $"Phòng [{roomNumber}] xuất hiện {roomCount} lần — hệ thống đã cho thêm trùng phòng!");

            Console.WriteLine("Phòng không được thêm mới. Dữ liệu gốc không bị thay đổi -> Pass.");

            Console.WriteLine("TestRoom15: Passed - Không thể thêm phòng nếu số phòng trùng");
        }
        /// Thêm phòng Bảo trì
        [TestMethod]
        public void TestRoom16()
        {
            string roomNumber = "1002";       // Số phòng cần thêm và kiểm tra
            string roomType = "VIP";       // Loại phòng
            string price = "1500000";      // Giá phòng
            string status = "Bảo trì";       // Trạng thái phòng

            room.RoomApplication(roomNumber, roomType, price, status);
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

            // Kiểm tra thông báo có nội dung thành công đúng
            Assert.IsTrue(successElement.Text.Contains("Thêm phòng thành công!"), "Không hiển thị thông báo thành công.");


            // Kiểm tra sự tồn tại của phòng vừa thêm
            Assert.IsTrue(room.IsRoomExists(roomNumber), $" Không tìm thấy phòng [{roomNumber}] trong danh sách.");
            Console.WriteLine($" Phòng [{roomNumber}] đã xuất hiện trong danh sách.");

            // Kiểm tra thông tin chi tiết phòng đã thêm
            var details = room.GetRoomDetails(roomNumber);
            Assert.IsNotNull(details, "Không lấy được thông tin chi tiết của phòng.");

            Assert.AreEqual(roomNumber, details.RoomNumber, " Số phòng không khớp.");
            Assert.AreEqual(roomType, details.RoomType, " Loại phòng không khớp.");
            Assert.AreEqual(price, details.Price.Replace(".", ""), " Giá phòng không khớp."); // Nếu giá hiển thị có dấu chấm
            Assert.AreEqual(status, details.Status, " Trạng thái phòng không khớp.");

            // Kiểm tra phòng có xuất hiện trong combobox đặt phòng
            room.OpenBookingPage();
            var select = wait.Until(d => d.FindElement(By.Id("room_id")));
            var options = new SelectElement(select).Options;
            bool existsInDropdown = options.Any(o => o.Text.Trim().Contains(roomNumber));
            Assert.IsFalse(existsInDropdown, $"Phòng [{roomNumber}] xuất hiện trong combobox đặt phòng.");
   
            Console.WriteLine("TestRoom16: Passed - Phòng Bảo trì không hiển thị trong combobox đặt phòng");

        }
       
        /// Đóng Form 
        ///--> Khi chưa nhập text
        [TestMethod]
        public void TestRoom17()
        {
            room.CloseForm1();
            bool isModalClosed = wait.Until(driver =>
            {
                try
                {
                    return !driver.FindElement(By.CssSelector("div.modal-content")).Displayed;
                   
                }
                catch (NoSuchElementException)
                {
                    // Nếu phần tử không tồn tại nghĩa là modal đã đóng
                    return true;
                }
            });

         
            if (isModalClosed)
            {
                Console.WriteLine(" Form sửa phòng đã được đóng thành công.");
            }
            else
            {
                Console.WriteLine("Form sửa phòng vẫn đang hiển thị.");
            }

            Assert.IsTrue(isModalClosed, "Form sửa phòng chưa được đóng sau khi nhấn nút Đóng.");
            Console.WriteLine("TestRoom17: Passed - Form sửa phòng chưa được đóng sau khi nhấn nút Đóng.");
        }
        ///--> Khi nhập text nhưng chưa lưu
        [TestMethod]
        public void TestRoom18()
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Nhập dữ liệu
            string roomNumber = "";
            string roomType = "VIP";
            string price = "1500000";
            string status = "Trống";

            // Gọi hàm đóng form sau khi điền
            room.CloseForm2(roomNumber, roomType, price, status);

            // Kiểm tra form đã đóng chưa (giả định modal không còn hiển thị)
            bool isModalClosed = wait.Until(driver =>
            {
                try
                {
                    return !driver.FindElement(By.CssSelector("div.modal-content")).Displayed;
                }
                catch (NoSuchElementException)
                {
                    // Nếu phần tử không tồn tại nghĩa là modal đã đóng
                    return true;
                }
            });


            if (isModalClosed)
            {
                Console.WriteLine(" Form sửa phòng đã được đóng thành công.");
            }
            else
            {
                Console.WriteLine("Form sửa phòng vẫn đang hiển thị.");
            }

            Assert.IsTrue(isModalClosed, "Form sửa phòng chưa được đóng sau khi nhấn nút Đóng.");
            Console.WriteLine("TestRoom18: Passed - Form sửa phòng chưa được đóng sau khi nhấn nút Đóng.");
        }
        //-> Giá phòng = -0,01
        [TestMethod]
        public void TestRoom19()
        {
            int countBefore = room.CountAllRooms();
            // Nhập các ô hợp lệ trừ giá phòng âm
            string roomNumber = "11cde";
            room.RoomApplication(roomNumber, "VIP", "-0,01", "Trống");
            var giaPhongInput = driver.FindElement(By.Id("price"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");
            // Thông báo
            Assert.AreEqual("Value must be greater than or equal to 0.", validationMessage);
            Console.WriteLine($"Thông báo lỗi: {validationMessage}");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");


            int countAfter = room.CountAllRooms();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có phòng nào được thêm vào hệ thống -> Pass");
            Console.WriteLine("TestRoom19: Passed - Không thể thêm phòng nếu giá phòng cận biên < 0 = - 0,01");
        }
        [TestMethod]
        public void TestRoom20()
        {
            int countBefore = room.CountAllRooms();
            // Nhập các ô hợp lệ trừ giá phòng = 100000000
            string roomNumber = "11cde";
            room.RoomApplication(roomNumber, "VIP", "100000000", "Trống");
            try
            {
                var errorElement = wait.Until(drv =>
                {
                    var el = drv.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return el.Displayed ? el : null;
                });

                Assert.IsTrue(errorElement.Text.Contains("Giá trị nhập không hợp lệ"));
                Console.WriteLine($"Thông báo lỗi hiển thị: {errorElement.Text}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi với giá phòng = 100.000.000.");
            }
            int countAfter = room.CountAllRooms();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có phòng nào được thêm vào hệ thống -> Pass");

            Console.WriteLine("TestRoom20: Passed - Không thể thêm phòng nếu giá phòng = 100.000.000");
        }
        [TestMethod]
        public void TestRoom21()
        {
            int countBefore = room.CountAllRooms();
            // Nhập các ô hợp lệ trừ giá phòng = 100000001
            string roomNumber = "11cde";
            room.RoomApplication(roomNumber, "VIP", "100000001", "Trống");
            try
            {
                var errorElement = wait.Until(drv =>
                {
                    var el = drv.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return el.Displayed ? el : null;
                });

                Assert.IsTrue(errorElement.Text.Contains("Giá trị nhập không hợp lệ"));
                Console.WriteLine($"Thông báo lỗi hiển thị: {errorElement.Text}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi với giá phòng = 100.000.000.");
            }
            int countAfter = room.CountAllRooms();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có phòng nào được thêm vào hệ thống -> Pass");

            Console.WriteLine("TestRoom21: Passed - Không thể thêm phòng nếu giá phòng = 100.000.001");
        }
        [TestMethod]
        //-> Giá phòng = -0,01 và phòng trùng
        public void TestRoom22()
        {
            string roomNumber = "107";
            string expectedMessage = "Phòng đã tồn tại";

            RoomModel roomBefore = room.GetRoomDetails(roomNumber);

            // 1. Thêm phòng trùng
            room.RoomApplication(roomNumber, "VIP", "-0,01", "Trống");

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Tìm lại input và lấy thông báo lỗi
            var giaPhongInput = driver.FindElement(By.Id("price"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");

            // Kiểm tra xem có đúng thông báo mong đợi không
            Assert.AreEqual("Value must be greater than or equal to 0.", validationMessage);
            Console.WriteLine($"Loại phòng hiển thị thông báo lỗi [{validationMessage}] ");

            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");
            // Đóng form
            driver.FindElement(By.CssSelector("button.btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form thêm phòng.");

            // 2. Kiểm tra xem có hiển thị lỗi phòng trùng không
            bool ErrorDisplayed = false;
            try
            {
                var errorElement = wait.Until(driver =>
                {
                    var elem = driver.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return (elem.Displayed && !string.IsNullOrWhiteSpace(elem.Text)) ? elem : null;
                });

                if (errorElement.Text.Contains(expectedMessage))
                {
                    ErrorDisplayed = true;
                    Console.WriteLine("Tìm thấy lỗi phòng trùng: " + expectedMessage);
                }
                else
                {
                    Console.WriteLine("Có hiển thị lỗi khác, nhưng không phải lỗi phòng trùng.");
                }
            }
            catch (WebDriverTimeoutException)
            {
                ErrorDisplayed = false;
                Console.WriteLine("Không tìm thấy lỗi phòng trùng.");
            }
            // 4. Đảm bảo phòng gốc vẫn tồn tại (không bị ghi đè, xóa)
            RoomModel roomAfter = room.GetRoomDetails(roomNumber);
            Assert.IsNotNull(roomAfter, $"Không tìm thấy phòng [{roomNumber}] sau khi test.");

            Assert.AreEqual(roomBefore.Price, roomAfter.Price, "Giá phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.Status, roomAfter.Status, "Tình trạng phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.RoomType, roomAfter.RoomType, "Loại phòng bị ghi đè thay đổi!");

            // 5. Đảm bảo không có bản ghi trùng
            int roomCount = room.CountRoom(roomNumber);
            Assert.AreEqual(1, roomCount, $"Phòng [{roomNumber}] xuất hiện {roomCount} lần — hệ thống đã cho thêm trùng phòng!");
            Console.WriteLine("Dữ liệu phòng không bị thay đổi -> Không bị ghi đè -> Đúng.");
            Console.WriteLine("TestRoom22: Passed - Ưu tiên kiểm tra lỗi giá phòng = -0,01, không xét đến lỗi trùng phòng.-> phòng kh được thêm  ");
        }
        [TestMethod]
        //-> Giá phòng = 100.000.0000
        public void TestRoom23()
        {
            string roomNumber = "1021";
            string expectedMessage = "Phòng đã tồn tại";

            RoomModel roomBefore = room.GetRoomDetails(roomNumber);
            Assert.IsNotNull(roomBefore, $"Không tìm thấy phòng [{roomNumber}] để kiểm thử.");

            // 1. Thêm phòng trùng
            room.RoomApplication(roomNumber, "VIP", "100000000", "Trống");

            //Check số  lỗi giá hiển thị
            // Chờ nếu cần
            wait.Until(d => d.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Any(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)));

            var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

            // Kiểm tra có ít nhất 1 lỗi
            if (alerts.Count == 0)
            {
                Assert.Fail("Không có thông báo lỗi nào được hiển thị.");
            }

            // Kiểm tra chỉ có 1 lỗi
            Assert.AreEqual(1, alerts.Count, "Có nhiều hơn 1 thông báo lỗi hiển thị.");

            // Kiểm tra nội dung lỗi đúng
            string actualError = alerts[0].Text.Trim();
            Assert.AreEqual(expectedMessage, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedMessage}\" - Thực tế: \"{actualError}\"");

            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            RoomModel roomAfter = room.GetRoomDetails(roomNumber);
            Assert.IsNotNull(roomAfter, $"Không tìm thấy phòng [{roomNumber}] sau khi test.");

            Assert.AreEqual(roomBefore.Price, roomAfter.Price, "Giá phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.Status, roomAfter.Status, "Tình trạng phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.RoomType, roomAfter.RoomType, "Loại phòng bị ghi đè thay đổi!");

            // 5. Đảm bảo không có bản ghi trùng
            int roomCount = room.CountRoom(roomNumber);
            Assert.AreEqual(1, roomCount, $"Phòng [{roomNumber}] xuất hiện {roomCount} lần — hệ thống đã cho thêm trùng phòng!");

            Console.WriteLine("Phòng không được thêm mới. Dữ liệu gốc không bị thay đổi -> Pass.");
            Console.WriteLine("TestRoom23: Passed - Ưu tiên kiểm tra lỗi giá phòng = 100.000.000, không xét đến lỗi trùng phòng -> phòng không được thêm  ");
        }
        [TestMethod]
        //-> Giá phòng = 100.000.001
        public void TestRoom24()
        {
            string roomNumber = "1021";
            string expectedMessage = "Phòng đã tồn tại";

            RoomModel roomBefore = room.GetRoomDetails(roomNumber);
            Assert.IsNotNull(roomBefore, $"Không tìm thấy phòng [{roomNumber}] để kiểm thử.");

            // 1. Thêm phòng trùng
            room.RoomApplication(roomNumber, "VIP", "100000001", "Trống");

            //Check số  lỗi giá hiển thị
            //Check số  lỗi giá hiển thị
            wait.Until(d => d.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Any(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)));

            var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

            // Kiểm tra có ít nhất 1 lỗi
            if (alerts.Count == 0)
            {
                Assert.Fail("Không có thông báo lỗi nào được hiển thị.");
            }

            // Kiểm tra chỉ có 1 lỗi
            Assert.AreEqual(1, alerts.Count, "Có nhiều hơn 1 thông báo lỗi hiển thị.");

            // Kiểm tra nội dung lỗi đúng
            string actualError = alerts[0].Text.Trim();
            Assert.AreEqual(expectedMessage, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedMessage}\" - Thực tế: \"{actualError}\"");

            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            RoomModel roomAfter = room.GetRoomDetails(roomNumber);
            Assert.IsNotNull(roomAfter, $"Không tìm thấy phòng [{roomNumber}] sau khi test.");

            Assert.AreEqual(roomBefore.Price, roomAfter.Price, "Giá phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.Status, roomAfter.Status, "Tình trạng phòng bị ghi đè thay đổi!");
            Assert.AreEqual(roomBefore.RoomType, roomAfter.RoomType, "Loại phòng bị ghi đè thay đổi!");

            // 5. Đảm bảo không có bản ghi trùng
            int roomCount = room.CountRoom(roomNumber);
            Assert.AreEqual(1, roomCount, $"Phòng [{roomNumber}] xuất hiện {roomCount} lần — hệ thống đã cho thêm trùng phòng!");

            Console.WriteLine("Phòng không được thêm mới. Dữ liệu gốc không bị thay đổi -> Pass.");
            Console.WriteLine("TestRoom24: Passed - Ưu tiên kiểm tra lỗi giá phòng = 100.000.001, không xét đến lỗi trùng phòng -> phòng không được thêm  ");
        }
        [TestMethod]
        //-> Tổng số phòng vượt quá max giưới hạn
        public void TestRoom25()
        {
            room.OpenSidebarIfNeeded();
            var  BtnRoom = driver.FindElement(By.CssSelector("#sidebarMenu .nav-link[href='rooms.php']"));
            BtnRoom.Click();

            // Chờ nút hiển thị
            wait.Until(d => d.FindElement(By.CssSelector("button.btn.btn-primary")));
            var addButton = driver.FindElement(By.CssSelector("button.btn.btn-primary"));

            // Kiểm tra nút bị vô hiệu hóa
            bool isDisabled = addButton.GetAttribute("disabled") != null;
            Assert.IsTrue(isDisabled, "Nút Thêm Phòng đáng lẽ phải bị disable khi vượt quá giới hạn.");

            // Di chuột để kích hoạt tooltip
            new Actions(driver).MoveToElement(addButton).Perform();

            // Chờ tooltip hiển thị (Bootstrap cần chút thời gian)
            Thread.Sleep(1000);

            // Kiểm tra tooltip hiển thị
            var tooltipElement = driver.FindElement(By.CssSelector(".tooltip.show .tooltip-inner"));
            string tooltipText = tooltipElement.Text;
            Console.WriteLine("Tooltip: " + tooltipText);

            Assert.IsTrue(tooltipText.Contains("Đã đạt giới hạn"), "Tooltip không đúng hoặc không xuất hiện.");
        }
        //[TestCleanup]
        //public void TearDown()
        //{
        //    driver.Quit(); // Luôn chạy sau mỗi test
        //}
    }
}
