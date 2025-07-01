using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DATN.CheckIn_CheckOut
{
    [TestClass]
    public class TestCheckOut_2
    {
        private IWebDriver driver;
        private CheckOut_2Page check;
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
            check = new CheckOut_2Page(driver);
        }
        [TestMethod]
        // Phòng không hợp lệ để nhận (nhận trong khoảng thời gian phòng đang dược sử dụng)
        public void TestCheckOut1()
        {
            string roomNumber = "107";
            string customerName = "Phạm Minh C";
            DateTime checkIn = new DateTime(2025, 5, 17);
            DateTime checkOut = new DateTime(2025, 5, 9);
            string surchargeAmount = "50000";
            string method = "cash";

            check.OpenBookingPage();
            var bookingRow = check.FindBookingRow(roomNumber, customerName, checkOut, "checked_in");
            Assert.IsNotNull(bookingRow, "Không tìm thấy dòng đặt phòng trong danh sách.");

            string roomCostText = bookingRow.FindElements(By.TagName("td"))[4].Text;
            string roomCostClean = Regex.Replace(roomCostText, @"[^\d]", "");
            int roomPrice = int.Parse(roomCostClean);
            int expectedDeposit = (int)(roomPrice * 0.4);
            int expectedFinalAmount = roomPrice - expectedDeposit;

            check.OpenCheckOutPage();
            var row = check.FindCheckOutRowByInfo(roomNumber, customerName, checkOut);
            Assert.IsNotNull(row, "Không tìm thấy dòng đặt phòng cần trả.");

            var btn = row.FindElement(By.CssSelector(".checkout-payment-btn"));
            btn.Click();

            wait.Until(d => d.FindElement(By.Id("checkoutPaymentModal")).Displayed);

            string modalText = driver.FindElement(By.Id("checkout_payment_booking_info")).Text;
            Assert.IsTrue(modalText.Contains(customerName), "Tên khách không khớp.");
            Assert.IsTrue(modalText.Contains(roomNumber), "Số phòng không khớp.");
            Assert.IsTrue(modalText.Contains(checkIn.ToString("dd/MM/yyyy")), "Ngày nhận không khớp.");
            Assert.IsTrue(modalText.Contains(checkOut.ToString("dd/MM/yyyy")), "Ngày trả không khớp.");

            string CleanCurrency(string raw) => Regex.Replace(raw, "[^\\d]", "");

            string modalRoomCost = CleanCurrency(driver.FindElement(By.Id("checkout_room_cost")).Text);
            string modalTotalCost = CleanCurrency(driver.FindElement(By.Id("checkout_total_cost")).Text);
            string modalDeposit = CleanCurrency(driver.FindElement(By.Id("checkout_deposit_amount")).Text);
            string modalFinalAmount = CleanCurrency(driver.FindElement(By.Id("checkout_final_amount")).Text);
            string modalAmountInput = driver.FindElement(By.Id("checkout_amount")).GetAttribute("value");

            Assert.AreEqual(roomPrice.ToString(), modalRoomCost, "Sai giá phòng.");
            Assert.AreEqual(roomPrice.ToString(), modalTotalCost, "Sai tổng tiền.");
            Assert.AreEqual(expectedDeposit.ToString(), modalDeposit, "Sai giá trị cọc.");
            Assert.AreEqual(expectedFinalAmount.ToString(), modalFinalAmount, "Sai tổng thanh toán.");
            Assert.AreEqual(expectedFinalAmount.ToString(), modalAmountInput, "Sai input amount.");

            Console.WriteLine("\u2705 Tất cả thông tin thanh toán hiển thị đúng.");

            var surchargeInput = driver.FindElement(By.Id("checkout_surcharge"));
            surchargeInput.Clear();
            surchargeInput.SendKeys(surchargeAmount);

            var methodSelect = new SelectElement(driver.FindElement(By.Id("checkout_method")));
            methodSelect.SelectByValue(method);

            var noteInput = driver.FindElement(By.Id("checkout_surcharge_note"));
            noteInput.Clear();
            noteInput.SendKeys("Test trả phòng có dịch vụ");

            driver.FindElement(By.CssSelector("button[name='checkout_payment']")).Click();

            wait.Until(d => d.PageSource.Contains("Trả phòng thành công") ||
                            d.PageSource.Contains("Thanh toán và trả phòng thành công"));

            Console.WriteLine("\u2705 Đã xác minh thông tin và trả phòng thành công.");
        }

    }
}