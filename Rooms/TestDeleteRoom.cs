using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Linq;

namespace DATN.Rooms
{
    [TestClass]
    public class TestDeleteRoom
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private RoomPageDelete room;

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
            room = new RoomPageDelete(driver);
        }
        //        /// <summary>
        //        /// Xóa phòng không thành công với 2TH:
        //        ///không có dữ liệu đặt phòng ⇒ có thể xóa
        //       
        //        /// </summary>
        [TestMethod]
        public void TestDeleteRoom1()
        {
            string roomNumberDelete = "2";

            // Xóa phòng
            room.DeleteRoom(roomNumberDelete);

            // Tạo WebDriverWait một lần, sử dụng cho cả alert và thông báo
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

            // Bắt và xử lý Alert xác nhận
            try
            {
                IAlert alert = wait.Until(ExpectedConditions.AlertIsPresent());

                string alertText = alert.Text;
                Console.WriteLine("Alert hiển thị với nội dung: " + alertText);

                if (alertText.Contains("Bạn có chắc chắn muốn xóa phòng này?"))
                {
                    Console.WriteLine("Nội dung xác nhận đúng.");
                    alert.Accept();
                }
                else
                {
                    Console.WriteLine("Nội dung xác nhận KHÔNG đúng.");
                    alert.Dismiss();
                }
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("Không có alert hiển thị.");
                Assert.Fail("Không có alert xác nhận khi xóa.");
            }

            try
            {
                var alertSuccess = wait.Until(d =>
                {
                    var el = d.FindElement(By.CssSelector("div.alert.alert-success"));
                    return el.Displayed ? el : null;
                });

                Console.WriteLine("Thông báo hiển thị: " + alertSuccess.Text);
                Assert.IsTrue(alertSuccess.Text.Contains("Xoá phòng thành công!"), "Không thấy thông báo xóa thành công.");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không thấy thông báo xóa thành công sau khi xác nhận.");
            }

            bool isExistAfter = room.IsRoomExists(roomNumberDelete);
            Assert.IsFalse(isExistAfter, $"Phòng [{roomNumberDelete}] vẫn còn trong danh sách sau khi xóa.");
            Console.WriteLine($"Phòng [{roomNumberDelete}] không còn xuất hiện trong danh sách phòng sau khi xóa. => Đúng mong đợi => Pass");

        }
        [TestMethod]
        /// có dữ liệu đặt phòng ⇒ không được xóa

        public void TestDeleteRoom2()
        {
            string roomNumberDelete = "111c";

            var roomRow = room.FindRoomRowByNumber(roomNumberDelete);
            var deleteButton = roomRow.FindElements(By.CssSelector("a.btn-danger, button.btn-danger")).FirstOrDefault();

            Assert.IsNotNull(deleteButton, $"Không tìm thấy nút xóa cho phòng [{roomNumberDelete}].");

            bool isDisabled = deleteButton.GetAttribute("disabled") != null || !deleteButton.Enabled;
            bool isHidden = !deleteButton.Displayed;

            Assert.IsTrue(isDisabled || isHidden, $"Nút xóa vẫn khả dụng cho phòng [{roomNumberDelete}].");

            Console.WriteLine($"Nút xóa của phòng [{roomNumberDelete}] đang bị vô hiệu hóa hoặc ẩn.=> Đúng mong đợi => Pass");
        }
        /// có dữ liệu đặt phòng ⇒ không được xóa
        
    }


    }


//      