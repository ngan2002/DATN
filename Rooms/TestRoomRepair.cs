using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static DATN.Rooms.RoomPageRepair;

namespace DATN.Rooms
{
    [TestClass]
    public class TestRoomRepair
    {
        private IWebDriver driver;
        private RoomPageRepair room;
        private WebDriverWait wait;

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
            room = new RoomPageRepair(driver);
           
        }
        [TestMethod]

        /// Sửa phòng với thông tin hợn lệ cho Phòng chưa có dữ liệu xuất hiên combobox
        public void TestRepairRoom1()
        {
            // Thông tin ban đầu
            string oldRoomNumber = "21210";
            string newRoomNumber = "12150";
            string newRoomType = "VIP";
            string newPrice = "20000";
            string newStatus = "Trống";

            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($" Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }

            // Bước 3: Cập nhật thông tin phòng
            room.UpdateRoom(newRoomNumber, newRoomType, newPrice, newStatus);

            // Bước 4: Kiểm tra thông báo thành công
            var alert = wait.Until(d => d.FindElement(By.CssSelector("div.alert.alert-success")));
            Assert.IsTrue(alert.Text.Contains("Cập nhật phòng thành công"), "Không thấy thông báo cập nhật thành công.");

            // Bước 5: Kiểm tra phòng mới có trong danh sách phòng
            bool isExistAfter = room.IsRoomExists(newRoomNumber);
            Assert.IsTrue(isExistAfter, $" Không tìm thấy phòng mới [{newRoomNumber}] sau khi sửa.");
            Console.WriteLine($"Phòng [{newRoomNumber}] đã xuất hiện trong danh sách phòng sau khi sửa.");

            // Bước 6: Kiểm tra lại toàn bộ thông tin phòng đã cập nhật
            var updatedRoom = room.FindRoomByNumber(newRoomNumber);
            Assert.AreEqual(newRoomNumber, updatedRoom.RoomNumber, "Số phòng chưa đúng.");
            Assert.AreEqual(newRoomType, updatedRoom.RoomType, "Loại phòng chưa đúng.");
            Assert.AreEqual(newPrice, updatedRoom.Price.Replace(".", ""), "Giá phòng chưa đúng.");
            //Assert.AreEqual(newStatus, updatedRoom.Status, "Tình trạng phòng chưa đúng.");
            // Bước 7: Kiểm tra thông tin phòng mới trong combobox đặt phòng
            string priceFormatted = Convert.ToDecimal(newPrice).ToString("#,##0").Replace(",", ".");
            room.VerifyRoomInBookingDropdown(
                roomNumberExpected: newRoomNumber,
                roomTypeExpected: newRoomType,
                priceFormatted: priceFormatted,
                roomNumberOld: oldRoomNumber
            );

            Console.WriteLine("TestRoomRepair1: Passed - Các thông tin được cập nhật thành công");
        }


        

        [TestMethod]
        public void TestRepairRoom2()
        {
            string oldRoomNumber = "3";
            string invalidRoomNumber = "111";
            string invalidRoomType = "VIP";
            string invalidPrice = "0";
            string invalidStatus = "Trống";

            // Lưu lại thông tin gốc trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin");

            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }

            // Gửi dữ liệu không hợp lệ
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);

            // Kiểm tra hiển thị lỗi
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

            // Kiểm tra phòng không bị sửa đổi
            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng cần kiểm tra sau khi sửa");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng bị thay đổi.");

            Console.WriteLine("TestRepairRoom2: Phòng giữ nguyên sau khi nhập giá trị không hợp lệ => TEST PASSED");
        }

        [TestMethod]
        public void TestRepairRoom3()
        {
            string oldRoomNumber = "3";
            string invalidRoomNumber = "test";
            string invalidRoomType = "VIP";
            string invalidPrice = "-231";
            string invalidStatus = "Trống";

            // Lưu lại thông tin phòng gốc
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin");

            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }

            // Gửi dữ liệu không hợp lệ
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);

            // Kiểm tra thông báo lỗi từ trình duyệt (HTML5 validation message)
            var giaPhongInput = driver.FindElement(By.Id("edit_price"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");
            Assert.AreEqual("Value must be greater than or equal to 0.", validationMessage);
            Console.WriteLine($"Thông báo lỗi: {validationMessage}");

            // Đảm bảo form chưa bị đóng
            bool isModalStillOpen = driver.FindElement(By.Id("editRoomModal"))
                                           .GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            // Đóng form thủ công
            driver.FindElement(By.CssSelector("div#editRoomModal button.btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form sửa phòng");

            // Kiểm tra phòng gốc vẫn còn
            bool stillExists = room.IsRoomExists(oldRoomNumber);
            Assert.IsTrue(stillExists, "Phòng gốc đã bị xóa hoặc thay đổi sau khi nhập dữ liệu không hợp lệ!");
            Console.WriteLine("Phòng gốc vẫn giữ nguyên sau khi nhập dữ liệu không hợp lệ.");

            // Kiểm tra lại toàn bộ thông tin phòng không bị thay đổi
            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng sau khi kiểm tra");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng bị thay đổi.");

            Console.WriteLine($"TestRepairRoom3: Thông tin phòng không bị cập nhật => TEST PASSED");
        }

        [TestMethod]
        public void TestRepairRoom4()
        {
            // giá > 10000000
            string oldRoomNumber = "3";
            string invalidRoomNumber = "1023t";
            string invalidRoomType = "VIP";
            string invalidPrice = "10000000000000000000000000";
            string invalidStatus = "Trống";
            // Lưu lại thông tin gốc trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin");

            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }

            // Gửi dữ liệu không hợp lệ
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);

            // Kiểm tra hiển thị lỗi
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
                Assert.Fail("Không tìm thấy thông báo lỗi với giá phòng > 100000000.");
            }

            // Kiểm tra phòng không bị sửa đổi
            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng cần kiểm tra sau khi sửa");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng bị thay đổi.");
            Console.WriteLine($"TestRoomRepair4: Phòng không bị cập nhật => TEST PASSED");


        }

        // Giá phòng trống
        [TestMethod]
        public void TestRepairRoom6()
        {
        
            string oldRoomNumber = "3";
            string invalidRoomNumber = "1023";
            string invalidRoomType = "VIP";
            string invalidPrice = "";
            string invalidStatus = "Trống";

            // Lưu lại thông tin gốc trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin");
            // Bước 1: Mở form sửa phòng
            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($" Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);
            // Tạo biến đợi chờ
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Tìm lại input và lấy thông báo lỗi
            var giaPhongInput = driver.FindElement(By.Id("edit_price"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");

            // Kiểm tra thông báo lỗi có đúng như mong đợi
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Giá phòng hiển thị thông báo lỗi: [{validationMessage}]");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("editRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");
            // Đóng form
            driver.FindElement(By.CssSelector("div#editRoomModal button.btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form sửa phòng");
            // Kiểm tra phòng gốc vẫn tồn tại
            bool stillExists = room.IsRoomExists(oldRoomNumber);
            Assert.IsTrue(stillExists, "Phòng gốc đã bị xóa hoặc đổi số sau khi nhập dữ liệu không hợp lệ!");
            Console.WriteLine("Phòng gốc vẫn giữ nguyên sau khi nhập dữ liệu không hợp lệ.");

            //Kiểm tra giá không bị cập nhật thành giá không
            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng cần kiểm tra sau khi sửa");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng bị thay đổi.");
            Console.WriteLine($"TestRoomRepair6: Giá phòng hiện tại: {roomInfo.Price} => KHÔNG bị cập nhật => TEST PASSED");
        }
        //giá phòng nhập chữ
        [TestMethod]
        public void TestRepairRoom5()
        {
            // giá trống
            string oldRoomNumber = "2";
            string newRoomNumber = "1023";
            string newRoomType = "VIP";
            string newPrice = "test";
            string newStatus = "Trống";
            // Bước 1: Mở form sửa phòng
            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($" Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }
            room.UpdateRoom(newRoomNumber, newRoomType, newPrice, newStatus);
            // Tạo biến đợi chờ
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Tìm lại input và lấy thông báo lỗi
            var giaPhongInput = driver.FindElement(By.Id("edit_price"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");
            // Kiểm tra xem có đúng thông báo mong đợi không
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Giá phòng hiển thị thông báo lỗi [{validationMessage}] -> không nhập được chữ ");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("editRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");
            // Đóng form
            driver.FindElement(By.CssSelector("div#editRoomModal button.btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form sửa phòng");
            // Kiểm tra phòng gốc vẫn tồn tại
            bool stillExists = room.IsRoomExists(oldRoomNumber);
            Assert.IsTrue(stillExists, "Phòng gốc đã bị xóa hoặc đổi số sau khi nhập dữ liệu không hợp lệ!");
            Console.WriteLine("Phòng gốc vẫn giữ nguyên sau khi nhập dữ liệu không hợp lệ.");

            //Kiểm tra giá không bị cập nhật thành giá không
            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng cần kiểm tra sau khi sửa");
            Assert.AreNotEqual(newRoomNumber, roomInfo.RoomNumber, "Số phòng bị cập nhật sai.");
            Assert.AreNotEqual(newRoomType, roomInfo.RoomType, "Loại phòng bị cập nhật sai.");
            Assert.AreNotEqual(newStatus, roomInfo.Status, "Tình trạng phòng bị cập nhật sai");
            Assert.AreNotEqual(newPrice, roomInfo.Price, "Giá phòng bị cập nhật sai (nhập chữ) dù không hợp lệ!");
            Console.WriteLine($"TestRoomRepair5: Giá phòng hiện tại: {roomInfo.Price} => KHÔNG bị cập nhật => TEST PASSED");
        }
        // Loại phòng để trống
        [TestMethod]
        public void TestRepairRoom7()
        {
            string oldRoomNumber = "3";
            string invalidRoomNumber = "103";
            string invalidRoomType = "Chọn loại phòng";
            string invalidPrice = "57899000";
            string invalidStatus = "Trống";

            // Lưu lại thông tin gốc trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin");
            // Bước 1: Mở form sửa phòng
            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($" Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);
            // Tạo biến đợi chờ
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Tìm lại input và lấy thông báo lỗi
            var LoaiPhongSelect = driver.FindElement(By.Id("edit_type"));
            string validationMessage = LoaiPhongSelect.GetAttribute("validationMessage");
            // Kiểm tra xem có đúng thông báo mong đợi không
            Assert.AreEqual("Please select an item in the list.", validationMessage);
            Console.WriteLine($"Loại phòng hiển thị thông báo lỗi [{validationMessage}] ");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("editRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");
            // Đóng form
            driver.FindElement(By.CssSelector("div#editRoomModal button.btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form sửa phòng");
            // Kiểm tra phòng gốc vẫn tồn tại
            bool stillExists = room.IsRoomExists(oldRoomNumber);
            Assert.IsTrue(stillExists, "Phòng gốc đã bị xóa hoặc đổi số sau khi nhập dữ liệu không hợp lệ!");
            Console.WriteLine("Phòng gốc vẫn giữ nguyên sau khi nhập dữ liệu không hợp lệ.");

            //Kiểm tra loại phòng có cập nhật không
            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng cần kiểm tra sau khi sửa");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng bị thay đổi.");
            Console.WriteLine($"TestRoomRepair7: Loại phòng hiện tại: {roomInfo.RoomType} => KHÔNG bị cập nhật => TEST PASSED");
        }
        //Số phòng trống
        [TestMethod]
        public void TestRepairRoom8()
        {
           
            string oldRoomNumber = "3";
            string invalidRoomNumber = "";
            string invalidRoomType = "VIP";
            string invalidPrice = "57899000";
            string invalidStatus = "Trống";

            // Lưu lại thông tin gốc trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin");
            // Bước 1: Mở form sửa phòng
            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($" Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);
            // Tạo biến đợi chờ
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Tìm lại input và lấy thông báo lỗi
            var giaPhongInput = driver.FindElement(By.Id("edit_room_number"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");
            // Kiểm tra xem có đúng thông báo mong đợi không
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Số phòng hiển thị thông báo lỗi [{validationMessage}] ");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("editRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");
            // Đóng form
            driver.FindElement(By.CssSelector("div#editRoomModal button.btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form sửa phòng");
            // Kiểm tra phòng gốc vẫn tồn tại
            bool stillExists = room.IsRoomExists(oldRoomNumber);
            Assert.IsTrue(stillExists, "Phòng gốc đã bị xóa hoặc đổi số sau khi nhập dữ liệu không hợp lệ!");
            Console.WriteLine("Phòng gốc vẫn giữ nguyên sau khi nhập dữ liệu không hợp lệ.");

            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng cần kiểm tra sau khi sửa");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng bị thay đổi.");
            Console.WriteLine($"TestRoomRepair8: Số phòng hiện tại: {oldRoomNumber} => KHÔNG bị cập nhật => TEST PASSED");
        }
        /// Phòng trùng + loại phòng kh hợp lệ
        [TestMethod]
        public void TestRepairRoom9()
        {
            
            string oldRoomNumber = "2";
            string invalidRoomNumber = "121";
            string invalidRoomType = "Chọn loại phòng";
            string invalidPrice = "57899000";
            string invalidStatus = "Trống";

            string expectedMessage = "Phòng đã tồn tại";

            // Lưu lại thông tin gốc trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin");

            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($" Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            // Tìm lại input và lấy thông báo lỗi
            var loaiPhongInput = driver.FindElement(By.Id("edit_type"));
            string validationMessage = loaiPhongInput.GetAttribute("validationMessage");
            
          
            // Kiểm tra xem có đúng thông báo mong đợi không
            Assert.AreEqual("Please select an item in the list.", validationMessage);
            Console.WriteLine($"Loại phòng hiển thị thông báo lỗi [{validationMessage}] ");

            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("editRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            // Đóng form
            driver.FindElement(By.CssSelector("#editRoomModal .btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form thêm phòng.");

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
            ;
          
            // Kiểm tra phòng gốc vẫn tồn tại
            bool stillExists = room.IsRoomExists(oldRoomNumber);
            Assert.IsTrue(stillExists, "Phòng gốc đã bị xóa hoặc đổi số sau khi nhập dữ liệu không hợp lệ!");
            Console.WriteLine("Phòng gốc vẫn giữ nguyên sau khi nhập dữ liệu không hợp lệ.");

            int roomCount = room.CountRoom(invalidRoomNumber);
            Assert.AreEqual(1, roomCount, $"Phòng [{invalidRoomNumber}] xuất hiện {roomCount} lần — hệ thống đã cho cập nhật trùng phòng");
            Console.WriteLine($"Phòng [{invalidRoomNumber}] chỉ xuất hiện {roomCount} lần trong danh sách phòng -> Pass");


            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng cần kiểm tra sau khi sửa");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng bị thay đổi.");
            Console.WriteLine("TestRoom9: Passed - Ưu tiên kiểm tra lỗi loại phòng trống, không xét đến lỗi trùng phòng.-> phòng kh được cập nhật");
        }
        /// <summary>
        /// Phòng trùng + giá phòng trống
        /// </summary>
        [TestMethod]
        public void TestRoomRepair10()
        {
            string oldRoomNumber = "107";
            string invalidRoomNumber = "121";
            string invalidRoomType = "Chọn loại phòng";
            string invalidPrice = "57899000";
            string invalidStatus = "Trống";

            string expectedMessage = "Phòng đã tồn tại";

            // Lưu lại thông tin gốc trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin");

            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($" Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Tìm lại input và lấy thông báo lỗi
            var giaPhongInput = driver.FindElement(By.Id("edit_price"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");
            // Kiểm tra xem có đúng thông báo mong đợi không
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Giá phòng hiển thị thông báo lỗi [{validationMessage}] ");

            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("editRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            // Đóng form
            driver.FindElement(By.CssSelector("#editRoomModal .btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form thêm phòng.");

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

            // Kiểm tra phòng gốc vẫn tồn tại
            bool stillExists = room.IsRoomExists(oldRoomNumber);
            Assert.IsTrue(stillExists, "Phòng gốc đã bị xóa hoặc đổi số sau khi nhập dữ liệu không hợp lệ!");
            Console.WriteLine("Phòng gốc vẫn giữ nguyên sau khi nhập dữ liệu không hợp lệ.");

            int roomCount = room.CountRoom(invalidRoomNumber);
            Assert.AreEqual(1, roomCount, $"Phòng [{invalidRoomNumber}] xuất hiện {roomCount} lần — hệ thống đã cho cập nhật trùng phòng");
            Console.WriteLine($"Phòng [{invalidRoomNumber}] chỉ xuất hiện {roomCount} lần trong danh sách phòng -> Pass");

            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng cần kiểm tra sau khi sửa");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng bị thay đổi.");

            Console.WriteLine("TestRepair10: Passed - Ưu tiên kiểm tra lỗi giá phòng trống, không xét đến lỗi trùng phòng.-> phòng không được cập nhật ");
        }
        /// <summary>
        /// Phòng trùng + giá phòng nhập chữ
        /// </summary>
        [TestMethod]
        public void TestRoomRepair11()
        {
            string oldRoomNumber = "107";
            string invalidRoomNumber = "121";
            string invalidRoomType = "Chọn loại phòng";
            string invalidPrice = "test";
            string invalidStatus = "Trống";

            string expectedMessage = "Phòng đã tồn tại";

            // Lưu lại thông tin gốc trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin");

            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($" Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Tìm lại input và lấy thông báo lỗi
            var giaPhongInput = driver.FindElement(By.Id("edit_price"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");
            // Kiểm tra xem có đúng thông báo mong đợi không
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Giá phòng hiển thị thông báo lỗi [{validationMessage}] ");

            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("editRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            // Đóng form
            driver.FindElement(By.CssSelector("#editRoomModal .btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form thêm phòng.");

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

            // Kiểm tra phòng gốc vẫn tồn tại
            bool stillExists = room.IsRoomExists(oldRoomNumber);
            Assert.IsTrue(stillExists, "Phòng gốc đã bị xóa hoặc đổi số sau khi nhập dữ liệu không hợp lệ!");
            Console.WriteLine("Phòng gốc vẫn giữ nguyên sau khi nhập dữ liệu không hợp lệ.");


            int roomCount = room.CountRoom(invalidRoomNumber);
            Assert.AreEqual(1, roomCount, $"Phòng [{invalidRoomNumber}] xuất hiện {roomCount} lần — hệ thống đã cho cập nhật trùng phòng");
            Console.WriteLine($"Phòng [{invalidRoomNumber}] chỉ xuất hiện {roomCount} lần trong danh sách phòng -> Pass");

            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng cần kiểm tra sau khi sửa");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng bị thay đổi.");

            Console.WriteLine("TestRepair11:Passed - Ưu tiên kiểm tra lỗi giá phòng nhập chữ, không xét đến lỗi trùng phòng.-> phòng kh được cập nhật");
        }
        /// <summary>
        /// Phòng trùng + giá phòng < 0
        /// </summary>
        [TestMethod]
        public void TestRoomRepair12()
        {
            string oldRoomNumber = "3";
            string invalidRoomNumber = "121";
            string invalidRoomType = "VIP";
            string invalidPrice = "-2000";
            string invalidStatus = "Trống";

            string expectedMessage = "Phòng đã tồn tại";

            // Lưu lại thông tin gốc trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin");
            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($" Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Tìm lại input và lấy thông báo lỗi
            var giaPhongInput = driver.FindElement(By.Id("edit_price"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");
           
            // Kiểm tra xem có đúng thông báo mong đợi không
            Assert.AreEqual("Value must be greater than or equal to 0.", validationMessage);
            Console.WriteLine($"Loại phòng hiển thị thông báo lỗi [{validationMessage}] ");

            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("editRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            // Đóng form
            driver.FindElement(By.CssSelector("#editRoomModal .btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form thêm phòng.");

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

          
            // Kiểm tra phòng gốc vẫn tồn tại
            bool stillExists = room.IsRoomExists(oldRoomNumber);
            Assert.IsTrue(stillExists, "Phòng gốc đã bị xóa hoặc đổi số sau khi nhập dữ liệu không hợp lệ!");
            Console.WriteLine("Phòng gốc vẫn giữ nguyên sau khi nhập dữ liệu không hợp lệ.");

            int roomCount = room.CountRoom(invalidRoomNumber);
            Assert.AreEqual(1, roomCount, $"Phòng [{invalidRoomNumber}] xuất hiện {roomCount} lần — hệ thống đã cho cập nhật trùng phòng");
            Console.WriteLine($"Phòng [{invalidRoomNumber}] chỉ xuất hiện {roomCount} lần trong danh sách phòng -> Pass");

            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng cần kiểm tra sau khi sửa");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng bị thay đổi.");

            Console.WriteLine("TestRoomRepair12:Passed - Ưu tiên kiểm tra lỗi giá phòng âm, không xét đến lỗi trùng phòng.-> phòng kh được cập nhật  ");
        }
        /// <summary>
        /// Phòng trùng + giá phòng > 10000000
        /// </summary>
        [TestMethod]
        public void TestRoomRepair13()
        {
            string oldRoomNumber = "3";
            string invalidRoomNumber = "121"; // Trùng cũng không xét, ưu tiên lỗi giá
            string invalidRoomType = "VIP";
            string invalidPrice = "1000000000000000"; // Giá quá lớn, không hợp lệ
            string invalidStatus = "Trống";

            string expectedError = "Giá trị nhập không hợp lệ";

            // Bước 1: Lưu lại thông tin gốc của phòng trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin.");
            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($" Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }
            // Bước 2: Mở form sửa và nhập dữ liệu không hợp lệ
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);

            // Bước 3: Kiểm tra thông báo lỗi
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
            Assert.AreEqual(expectedError, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedError}\" - Thực tế: \"{actualError}\"");

            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            // Bước 4: Kiểm tra phòng gốc vẫn còn
            bool stillExists = room.IsRoomExists(oldRoomNumber);
            Assert.IsTrue(stillExists, "Phòng gốc đã bị xóa hoặc đổi số sau khi nhập dữ liệu không hợp lệ.");
            Console.WriteLine("Phòng gốc vẫn giữ nguyên sau khi nhập dữ liệu không hợp lệ.");

            // Bước 5: Kiểm tra phòng trùng không bị thêm/sửa (nếu có)
            int roomCount = room.CountRoom(invalidRoomNumber);
            Assert.AreEqual(1, roomCount,
                $"Phòng [{invalidRoomNumber}] xuất hiện {roomCount} lần — hệ thống đã cập nhật/sửa sai.");
            Console.WriteLine($"Phòng [{invalidRoomNumber}] chỉ xuất hiện {roomCount} lần trong danh sách phòng => PASSED.");

            // Bước 6: Kiểm tra dữ liệu phòng không thay đổi
            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng sau khi kiểm tra.");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng đã bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng đã bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng đã bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng đã bị thay đổi.");

            Console.WriteLine("TestRoomRepair13: PASSED — Ưu tiên kiểm tra lỗi giá phòng lớn bất thường => Phòng KHÔNG bị cập nhật.");
        }


        /// <summary>
        /// Phòng trùng + giá phòng = 0
        /// </summary>
        [TestMethod]
        public void TestRoomRepair14()
        {
            string oldRoomNumber = "3";
            string invalidRoomNumber = "121"; // Trùng cũng không xét, ưu tiên lỗi giá
            string invalidRoomType = "VIP";
            string invalidPrice = "0"; // Giá quá lớn, không hợp lệ
            string invalidStatus = "Trống";

            string expectedError = "Giá trị nhập không hợp lệ";

            // Bước 1: Lưu lại thông tin gốc của phòng trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin.");
            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($" Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }
            // Bước 2: Mở form sửa và nhập dữ liệu không hợp lệ
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);

            // Bước 3: Kiểm tra thông báo lỗi
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
            Assert.AreEqual(expectedError, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedError}\" - Thực tế: \"{actualError}\"");

            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            // Bước 4: Kiểm tra phòng gốc vẫn còn
            bool stillExists = room.IsRoomExists(oldRoomNumber);
            Assert.IsTrue(stillExists, "Phòng gốc đã bị xóa hoặc đổi số sau khi nhập dữ liệu không hợp lệ.");
            Console.WriteLine("Phòng gốc vẫn giữ nguyên sau khi nhập dữ liệu không hợp lệ.");

            // Bước 5: Kiểm tra phòng trùng không bị thêm/sửa (nếu có)
            int roomCount = room.CountRoom(invalidRoomNumber);
            Assert.AreEqual(1, roomCount,
                $"Phòng [{invalidRoomNumber}] xuất hiện {roomCount} lần — hệ thống đã cập nhật/sửa sai.");
            Console.WriteLine($"Phòng [{invalidRoomNumber}] chỉ xuất hiện {roomCount} lần trong danh sách phòng => PASSED.");

            // Bước 6: Kiểm tra dữ liệu phòng không thay đổi
            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng sau khi kiểm tra.");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng đã bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng đã bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng đã bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng đã bị thay đổi.");
            Console.WriteLine("TestRoomRepair14:Passed - Ưu tiên kiểm tra lỗi giá phòng  = 0, không xét đến lỗi trùng phòng.-> phòng kh được cập nhật  ");
        }
        /// <summary>
        /// Phòng trùng 
        /// </summary>
        [TestMethod]
        public void TestRoomRepair15()
        {
            string oldRoomNumber = "3";
            string invalidRoomNumber = "121"; // Trùng cũng không xét, ưu tiên lỗi giá
            string invalidRoomType = "VIP";
            string invalidPrice = "20000000"; 
            string invalidStatus = "Trống";

            string expectedError = "Phòng đã tồn tại";

            // Bước 1: Lưu lại thông tin gốc của phòng trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin.");
            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($" Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }
            // Bước 2: Mở form sửa và nhập dữ liệu không hợp lệ
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);

            // Bước 3: Kiểm tra thông báo lỗi
            bool errorDisplayed = false;

            try
            {
                var errorElement = wait.Until(driver =>
                {
                    var elem = driver.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return (elem.Displayed && !string.IsNullOrWhiteSpace(elem.Text)) ? elem : null;
                });

                string actualMessage = errorElement.Text.Trim();

                if (actualMessage == expectedError)
                {
                    errorDisplayed = true;
                    Console.WriteLine("Lỗi đúng mong đợi: " + actualMessage);
                }
                else
                {
                    Console.WriteLine($"Lỗi sai. Mong đợi: \"{expectedError}\", Thực tế: \"{actualMessage}\"");
                }
            }
            catch
            {
                Console.WriteLine("Không hiển thị lỗi.");
            }

            Assert.IsTrue(errorDisplayed, "Không hiển thị đúng lỗi mong đợi.");


            // Bước 4: Kiểm tra phòng gốc vẫn còn
            bool stillExists = room.IsRoomExists(oldRoomNumber);
            Assert.IsTrue(stillExists, "Phòng gốc đã bị xóa hoặc đổi số sau khi nhập dữ liệu không hợp lệ.");
            Console.WriteLine("Phòng gốc vẫn giữ nguyên sau khi nhập dữ liệu không hợp lệ.");

            // Bước 5: Kiểm tra phòng trùng không bị thêm/sửa (nếu có)
            int roomCount = room.CountRoom(invalidRoomNumber);
            Assert.AreEqual(1, roomCount,
                $"Phòng [{invalidRoomNumber}] xuất hiện {roomCount} lần — hệ thống đã cập nhật/sửa sai.");
            Console.WriteLine($"Phòng [{invalidRoomNumber}] chỉ xuất hiện {roomCount} lần trong danh sách phòng => PASSED.");

            // Bước 6: Kiểm tra dữ liệu phòng không thay đổi
            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng sau khi kiểm tra.");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng đã bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng đã bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng đã bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng đã bị thay đổi.");
            Console.WriteLine("TestRoom15:Passed - Ưu tiên kiểm tra lỗi trùng phòng.-> phòng không được cập nhật  ");
        }
        [TestMethod]
        /// Sửa phòng với thông tin hợn lệ cho Phòng bảo trì
        public void TestRepairRoom16()
        {
            // Thông tin ban đầu
            String oldRoomNumber = "3";
            string newRoomNumber = "32";
            string newRoomType = "VIP";
            string newPrice = "20000";
            string newStatus = "Trống";

            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($" Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }

            // Bước 3: Cập nhật thông tin phòng
            room.UpdateRoom(newRoomNumber, newRoomType, newPrice, newStatus);

            // Bước 4: Kiểm tra thông báo thành công
            var alert = wait.Until(d => d.FindElement(By.CssSelector("div.alert.alert-success")));
            Assert.IsTrue(alert.Text.Contains("Cập nhật phòng thành công"), "Không thấy thông báo cập nhật thành công.");

            // Bước 5: Kiểm tra phòng mới có trong danh sách phòng
            bool isExistAfter = room.IsRoomExists(newRoomNumber);
            Assert.IsTrue(isExistAfter, $" Không tìm thấy phòng mới [{newRoomNumber}] sau khi sửa.");
            Console.WriteLine($"Phòng [{newRoomNumber}] đã xuất hiện trong danh sách phòng sau khi sửa.");

            // Bước 6: Kiểm tra lại toàn bộ thông tin phòng đã cập nhật
            var updatedRoom = room.FindRoomByNumber(newRoomNumber);
            Assert.AreEqual(newRoomNumber, updatedRoom.RoomNumber, "Số phòng chưa đúng.");
            Assert.AreEqual(newRoomType, updatedRoom.RoomType, "Loại phòng chưa đúng.");
            Assert.AreEqual(newPrice, updatedRoom.Price.Replace(".", ""), "Giá phòng chưa đúng.");
            //Assert.AreEqual(newStatus, updatedRoom.Status, "Tình trạng phòng chưa đúng.");
            // Bước 7: Kiểm tra thông tin phòng mới trong combobox đặt phòng
            string priceFormatted = Convert.ToDecimal(newPrice).ToString("#,##0").Replace(",", ".");
            room.VerifyRoomInBookingDropdown(
                roomNumberExpected: newRoomNumber,
                roomTypeExpected: newRoomType,
                priceFormatted: priceFormatted,
                roomNumberOld: oldRoomNumber
            );
            Console.WriteLine("TestRoomRepair16: Passed - Các thông tin phong bảo trì được cập nhật thành công");
        }
        [TestMethod]

        public void TestRepairRoom17()
        {

            string oldRoomNumber = "1021";

            room.CloseForm1(oldRoomNumber); // đã xử lý đầy đủ trong hàm
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
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
                Console.WriteLine("Pass: Form sửa phòng đã được đóng thành công dù chưa nhập thông tin.");
            }
            else
            {
                Console.WriteLine("Fail: Form sửa phòng vẫn đang hiển thị.");
            }

            Assert.IsTrue(isModalClosed, "Form sửa phòng chưa được đóng sau khi nhấn nút Đóng.");

        }
        /// Đóng Form 
        ///--> Khi chưa nhập text
        //        ///--> Khi nhập text nhưng chưa lưu
        [TestMethod]
        public void TestRepairRoom18()
        {
            string oldRoomNumber = "1021";
            string newRoomNumber = "-5000";
            string newRoomType = "Deluxe";
            string newPrice = "2000000";
            string newStatus = "Trống";
            // Gọi hàm đóng form sau khi điền dữ liệu
            room.CloseForm2(newRoomNumber, oldRoomNumber, newRoomType, newPrice, newStatus);

            // Kiểm tra class của modal không còn chứa "show"
            var modal = driver.FindElement(By.Id("editRoomModal"));
            bool isModalClosed = !modal.GetAttribute("class").Contains("show");

            if (isModalClosed)
            {
                Console.WriteLine("Pass: Form sửa phòng đã được đóng thành công dù đã nhập hết thông tin nhưng chưa lưu.");
            }
            else
            {
                Console.WriteLine("Fail: Form sửa phòng vẫn đang hiển thị.");
            }

            Assert.IsTrue(isModalClosed, "Form sửa phòng chưa được đóng sau khi nhấn nút Đóng.");
        }
        //-> Giá phòng = -0,01
        [TestMethod]
        public void TestRepairRoom19()
        {
            string oldRoomNumber = "3";
            string invalidRoomNumber = "test";
            string invalidRoomType = "VIP";
            string invalidPrice = "-0,01";
            string invalidStatus = "Trống";

            // Lưu lại thông tin phòng gốc
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin");

            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }

            // Gửi dữ liệu không hợp lệ
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);

            // Kiểm tra thông báo lỗi từ trình duyệt (HTML5 validation message)
            var giaPhongInput = driver.FindElement(By.Id("edit_price"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");
            Assert.AreEqual("Value must be greater than or equal to 0.", validationMessage);
            Console.WriteLine($"Thông báo lỗi: {validationMessage}");

            // Đảm bảo form chưa bị đóng
            bool isModalStillOpen = driver.FindElement(By.Id("editRoomModal"))
                                           .GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            // Đóng form thủ công
            driver.FindElement(By.CssSelector("div#editRoomModal button.btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form sửa phòng");

            // Kiểm tra phòng gốc vẫn còn
            bool stillExists = room.IsRoomExists(oldRoomNumber);
            Assert.IsTrue(stillExists, "Phòng gốc đã bị xóa hoặc thay đổi sau khi nhập dữ liệu không hợp lệ!");
            Console.WriteLine("Phòng gốc vẫn giữ nguyên sau khi nhập dữ liệu không hợp lệ.");

            // Kiểm tra lại toàn bộ thông tin phòng không bị thay đổi
            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng sau khi kiểm tra");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng bị thay đổi.");
            Console.WriteLine("TestRepairRoom19: Passed - Không thể sửa phòng nếu giá phòng cận biên < 0 = - 0,01");
        }
        [TestMethod]
        public void TestRoom20()
        {
            string oldRoomNumber = "101test";
            string invalidRoomNumber = "1023te";
            string invalidRoomType = "VIP";
            string invalidPrice = "100000000";
            string invalidStatus = "Trống";
            // Lưu lại thông tin gốc trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin");

            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }

            // Gửi dữ liệu không hợp lệ
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);

            // Kiểm tra hiển thị lỗi
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
                Assert.Fail("Không tìm thấy thông báo lỗi với giá phòng = 100000000.");
            }

            // Kiểm tra phòng không bị sửa đổi
            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng cần kiểm tra sau khi sửa");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng bị thay đổi.");

            Console.WriteLine("TestRoom20: Passed - Không thể sửa phòng nếu giá phòng = 100000000");
        }
        [TestMethod]
        public void TestRoom21()
        {
            string oldRoomNumber = "101test";
            string invalidRoomNumber = "1023te";
            string invalidRoomType = "VIP";
            string invalidPrice = "100000001";
            string invalidStatus = "Trống";
            // Lưu lại thông tin gốc trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin");

            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }

            // Gửi dữ liệu không hợp lệ
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);

            // Kiểm tra hiển thị lỗi
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
                Assert.Fail("Không tìm thấy thông báo lỗi với giá phòng = 100000001.");
            }

            // Kiểm tra phòng không bị sửa đổi
            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng cần kiểm tra sau khi sửa");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng bị thay đổi.");

            Console.WriteLine("TestRoom20: Passed - Không thể sửa phòng nếu giá phòng = 100000001");
        }
        [TestMethod]
        //-> Giá phòng = -0,01 và phòng trùng
        public void TestRoom22()
        {
            string oldRoomNumber = "3";
            string invalidRoomNumber = "121";
            string invalidRoomType = "VIP";
            string invalidPrice = "-0,01";
            string invalidStatus = "Trống";

            string expectedMessage = "Phòng đã tồn tại";

            // Lưu lại thông tin gốc trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin");
            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($" Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Tìm lại input và lấy thông báo lỗi
            var giaPhongInput = driver.FindElement(By.Id("edit_price"));
            string validationMessage = giaPhongInput.GetAttribute("validationMessage");

            // Kiểm tra xem có đúng thông báo mong đợi không
            Assert.AreEqual("Value must be greater than or equal to 0.", validationMessage);
            Console.WriteLine($"Loại phòng hiển thị thông báo lỗi [{validationMessage}] ");

            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("editRoomModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            // Đóng form
            driver.FindElement(By.CssSelector("#editRoomModal .btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form thêm phòng.");

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


            // Kiểm tra phòng gốc vẫn tồn tại
            bool stillExists = room.IsRoomExists(oldRoomNumber);
            Assert.IsTrue(stillExists, "Phòng gốc đã bị xóa hoặc đổi số sau khi nhập dữ liệu không hợp lệ!");
            Console.WriteLine("Phòng gốc vẫn giữ nguyên sau khi nhập dữ liệu không hợp lệ.");

            int roomCount = room.CountRoom(invalidRoomNumber);
            Assert.AreEqual(1, roomCount, $"Phòng [{invalidRoomNumber}] xuất hiện {roomCount} lần — hệ thống đã cho cập nhật trùng phòng");
            Console.WriteLine($"Phòng [{invalidRoomNumber}] chỉ xuất hiện {roomCount} lần trong danh sách phòng -> Pass");

            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng cần kiểm tra sau khi sửa");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng bị thay đổi.");
            Console.WriteLine("TestRoom22: Passed - Ưu tiên kiểm tra lỗi giá phòng = -0,01, không xét đến lỗi trùng phòng.-> phòng kh được sửa  ");
        }
        [TestMethod]
        //-> Giá phòng = 100.000.0000
        public void TestRoom23()
        {
            string oldRoomNumber = "3";
            string invalidRoomNumber = "121"; // Trùng cũng không xét, ưu tiên lỗi giá
            string invalidRoomType = "VIP";
            string invalidPrice = "100000000"; // Giá quá lớn, không hợp lệ
            string invalidStatus = "Trống";

            string expectedError = "Giá trị nhập không hợp lệ";

            // Bước 1: Lưu lại thông tin gốc của phòng trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin.");
            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($" Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }
            // Bước 2: Mở form sửa và nhập dữ liệu không hợp lệ
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);

            // Bước 3: Kiểm tra thông báo lỗi
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
            Assert.AreEqual(expectedError, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedError}\" - Thực tế: \"{actualError}\"");

            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            // Bước 4: Kiểm tra phòng gốc vẫn còn
            bool stillExists = room.IsRoomExists(oldRoomNumber);
            Assert.IsTrue(stillExists, "Phòng gốc đã bị xóa hoặc đổi số sau khi nhập dữ liệu không hợp lệ.");
            Console.WriteLine("Phòng gốc vẫn giữ nguyên sau khi nhập dữ liệu không hợp lệ.");

            // Bước 5: Kiểm tra phòng trùng không bị thêm/sửa (nếu có)
            int roomCount = room.CountRoom(invalidRoomNumber);
            Assert.AreEqual(1, roomCount,
                $"Phòng [{invalidRoomNumber}] xuất hiện {roomCount} lần — hệ thống đã cập nhật/sửa sai.");
            Console.WriteLine($"Phòng [{invalidRoomNumber}] chỉ xuất hiện {roomCount} lần trong danh sách phòng => PASSED.");

            // Bước 6: Kiểm tra dữ liệu phòng không thay đổi
            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng sau khi kiểm tra.");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng đã bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng đã bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng đã bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng đã bị thay đổi.");
            Console.WriteLine("TestRoom23: Passed - Ưu tiên kiểm tra lỗi giá phòng = 100000000, không xét đến lỗi trùng phòng -> phòng không được sửa  ");
        }
        [TestMethod]
        //-> Giá phòng = 100.000.001
        public void TestRoom24()
        {
            string oldRoomNumber = "3";
            string invalidRoomNumber = "121"; // Trùng cũng không xét, ưu tiên lỗi giá
            string invalidRoomType = "VIP";
            string invalidPrice = "100000001"; // Giá quá lớn, không hợp lệ
            string invalidStatus = "Trống";

            string expectedError = "Giá trị nhập không hợp lệ";

            // Bước 1: Lưu lại thông tin gốc của phòng trước khi sửa
            var originalRoomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(originalRoomInfo, "Không tìm thấy phòng gốc để lưu thông tin.");
            try
            {
                room.OpenEditRoomForm(oldRoomNumber);
            }
            catch (Exception ex)
            {
                Assert.Fail($" Không tìm thấy phòng [{oldRoomNumber}] để sửa: {ex.Message}");
            }
            // Bước 2: Mở form sửa và nhập dữ liệu không hợp lệ
            room.UpdateRoom(invalidRoomNumber, invalidRoomType, invalidPrice, invalidStatus);

            // Bước 3: Kiểm tra thông báo lỗi
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
            Assert.AreEqual(expectedError, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedError}\" - Thực tế: \"{actualError}\"");

            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            // Bước 4: Kiểm tra phòng gốc vẫn còn
            bool stillExists = room.IsRoomExists(oldRoomNumber);
            Assert.IsTrue(stillExists, "Phòng gốc đã bị xóa hoặc đổi số sau khi nhập dữ liệu không hợp lệ.");
            Console.WriteLine("Phòng gốc vẫn giữ nguyên sau khi nhập dữ liệu không hợp lệ.");

            // Bước 5: Kiểm tra phòng trùng không bị thêm/sửa (nếu có)
            int roomCount = room.CountRoom(invalidRoomNumber);
            Assert.AreEqual(1, roomCount,
                $"Phòng [{invalidRoomNumber}] xuất hiện {roomCount} lần — hệ thống đã cập nhật/sửa sai.");
            Console.WriteLine($"Phòng [{invalidRoomNumber}] chỉ xuất hiện {roomCount} lần trong danh sách phòng => PASSED.");

            // Bước 6: Kiểm tra dữ liệu phòng không thay đổi
            var roomInfo = room.FindRoomByNumber(oldRoomNumber);
            Assert.IsNotNull(roomInfo, "Không tìm thấy phòng sau khi kiểm tra.");

            Assert.AreEqual(originalRoomInfo.RoomNumber, roomInfo.RoomNumber, "Số phòng đã bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.RoomType, roomInfo.RoomType, "Loại phòng đã bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Price, roomInfo.Price, "Giá phòng đã bị thay đổi.");
            Assert.AreEqual(originalRoomInfo.Status, roomInfo.Status, "Tình trạng phòng đã bị thay đổi.");
            Console.WriteLine("TestRoom24: Passed - Ưu tiên kiểm tra lỗi giá phòng = 100000001, không xét đến lỗi trùng phòng -> phòng không được sửa ");
        }
        [TestMethod]
        //-> Nút Sửa khi phòng có dữ liệu
        public void TestRoom25()
        {
            room.OpenSidebarIfNeeded();
            var BtnRoom = driver.FindElement(By.CssSelector("#sidebarMenu .nav-link[href='rooms.php']"));
            BtnRoom.Click();
            // Tìm phòng cần kiểm tra (giả sử là phòng số 101)
            string roomNumber = "107";

            // Tìm dòng chứa số phòng đó
            var row = driver.FindElements(By.CssSelector("table tbody tr"))
                            .FirstOrDefault(r => r.Text.Contains(roomNumber));

            Assert.IsNotNull(row, $"Không tìm thấy phòng có số [{roomNumber}] trong danh sách.");

            // Tìm nút sửa trong dòng đó
            var editButton = row.FindElement(By.CssSelector("button.btn-info"));

            // Kiểm tra nút có bị vô hiệu hóa (disabled) không
            bool isDisabled = editButton.GetAttribute("disabled") == "true"
                           || !editButton.Enabled;

            Assert.IsTrue(isDisabled, $"Nút sửa phòng [{roomNumber}] không bị vô hiệu hóa dù phòng đang được đặt.");

            Console.WriteLine($"Nút sửa phòng [{roomNumber}] đã bị vô hiệu hóa đúng như mong đợi.");

        }
        //[TestCleanup]
        //public void TearDown()
        //{
        //    driver.Quit(); // Luôn chạy sau mỗi test
        //}
    }
}




