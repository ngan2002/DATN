using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DATN.BookingRoom
{
    public class BookingPage
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;

        public BookingPage(IWebDriver browser)
        {
            driver = browser ?? throw new ArgumentNullException(nameof(browser));
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        private By SidebarMenuLocator => By.Id("sidebarMenu");
        private IWebElement SidebarMenu => driver.FindElement(SidebarMenuLocator);

        private By BookingMenuLocator => By.CssSelector("#sidebarMenu .nav-link[href='bookings.php']");
        private IWebElement BtnBooking => driver.FindElement(BookingMenuLocator);

        private By BtnAddBookingLocator => By.CssSelector("button.btn.btn-primary");
        private IWebElement BtnAddBooking => driver.FindElement(BtnAddBookingLocator);

        private By CustomerSelectLocator => By.Id("cccd");
        private IWebElement SelectCustomer => driver.FindElement(CustomerSelectLocator);

        private By RoomCheckboxesLocator => By.CssSelector(".room-checkbox");
        private IReadOnlyCollection<IWebElement> RoomCheckboxes => driver.FindElements(RoomCheckboxesLocator);

        private By CheckInLocator => By.Id("check_in");
        private IWebElement InputCheckIn => driver.FindElement(CheckInLocator);

        private By CheckOutLocator => By.Id("check_out");
        private IWebElement InputCheckOut => driver.FindElement(CheckOutLocator);

        private By DepositLocator => By.Id("deposit");
        private IWebElement InputDeposit => driver.FindElement(DepositLocator);

        private By PaymentMethodLocator => By.Id("method");
        private IWebElement SelectPaymentMethod => driver.FindElement(PaymentMethodLocator);

        private By BtnSubmitAddBookingLocator => By.CssSelector("button[type='submit'][name='add_booking']");
        private IWebElement BtnSubmitAddBooking => driver.FindElement(BtnSubmitAddBookingLocator);

        private By BtnCloseModalLocator => By.CssSelector("button.btn.btn-secondary[data-bs-dismiss='modal']");
        private IWebElement BtnCloseModal => driver.FindElement(BtnCloseModalLocator);

        private void OpenSidebarIfNeeded()
        {
            wait.Until(d => d.FindElement(SidebarMenuLocator));
            var sidebar = SidebarMenu;
            if (!sidebar.Displayed)
            {
                var toggleButton = driver.FindElement(By.CssSelector("button.navbar-toggler"));
                toggleButton.Click();
            }
        }

        public void OpenAddBookingSection()
        {
            OpenSidebarIfNeeded();
            BtnBooking.Click();
            BtnAddBooking.Click();
            wait.Until(d => d.FindElement(By.Id("addBookingModal")).GetAttribute("class").Contains("show"));
        }

        public void AddBooking(string cccd, List<string> roomNumbers, DateTime checkInDate, DateTime checkOutDate, string paymentMethod = "cash")
        {
            OpenAddBookingSection();

            // Chọn khách hàng
            new SelectElement(SelectCustomer).SelectByValue(cccd);

            // Chọn phòng theo số phòng (room_number)
            foreach (string roomNumber in roomNumbers)
            {
                // Giả định: mỗi checkbox phòng có value = room_number
                var checkbox = driver.FindElement(By.CssSelector($"input.room-checkbox[value='{roomNumber}']"));
                if (!checkbox.Selected)
                {
                    checkbox.Click();
                }
            }

            // Nhập ngày nhận và trả
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript($"document.getElementById('check_in').value = '{checkInDate:yyyy-MM-dd}';");
            js.ExecuteScript($"document.getElementById('check_out').value = '{checkOutDate:yyyy-MM-dd}';");

            // Chọn phương thức thanh toán
            new SelectElement(SelectPaymentMethod).SelectByValue(paymentMethod);

            // Gửi form
            BtnSubmitAddBooking.Click();
        }


        public void OpenBookingPage()
        {
            OpenSidebarIfNeeded();
            BtnBooking.Click();
            wait.Until(driver => driver.Url.Contains("bookings.php"));
        }

        public IWebElement FindBookingRow(string roomNumber, string cccd, DateTime checkIn, DateTime checkOut)
        {
            wait.Until(driver => driver.FindElements(By.CssSelector("table tbody tr")).Count > 0);
            int maxPages = 20;

            for (int page = 0; page < maxPages; page++)
            {
                var rows = driver.FindElements(By.CssSelector("table tbody tr"));
                foreach (var row in rows)
                {
                    var cells = row.FindElements(By.TagName("td"));
                    if (cells.Count >= 5)
                    {
                        string customerText = cells[0].Text;
                        string extractedCccd = Regex.Match(customerText, @"\((\d{9,})\)").Groups[1].Value;

                        string roomText = cells[1].Text.Trim();
                        string extractedRoomNumber = Regex.Match(roomText, @"^\S+").Value;

                        string checkInText = cells[2].Text.Trim();
                        string checkOutText = cells[3].Text.Trim();

                        if (extractedRoomNumber == roomNumber &&
                            extractedCccd == cccd &&
                            checkInText == checkIn.ToString("dd/MM/yyyy") &&
                            checkOutText == checkOut.ToString("dd/MM/yyyy"))
                        {
                            return row;
                        }
                    }
                }

                var nextButtons = driver.FindElements(By.XPath("//a[@class='page-link' and normalize-space(text())='Tiếp']"));
                if (nextButtons.Count == 0) break;

                try
                {
                    nextButtons[0].Click();
                    Thread.Sleep(500);
                }
                catch
                {
                    break;
                }
            }

            return null;
        }

        public bool IsBookingExists(string roomNumber, string cccd, DateTime checkIn, DateTime checkOut)
        {
            return FindBookingRow(roomNumber, cccd, checkIn, checkOut) != null;
        }

        public int CountRoomAppearancesWithDetails(string roomNumber, string cccd, string checkInDate, string checkOutDate)
        {
            int count = 0;
            int maxPages = 20;

            for (int page = 0; page < maxPages; page++)
            {
                var rows = driver.FindElements(By.CssSelector("table tbody tr"));
                foreach (var row in rows)
                {
                    var cells = row.FindElements(By.TagName("td"));
                    if (cells.Count >= 5)
                    {
                        string customerInfo = cells[0].Text.Trim();
                        string extractedCCCD = Regex.Match(customerInfo, @"\((\d{9,15})\)").Groups[1].Value;

                        string roomInfo = cells[1].Text.Trim();
                        string extractedRoomNumber = roomInfo.Split('(')[0].Trim();

                        string checkIn = cells[2].Text.Trim();
                        string checkOut = cells[3].Text.Trim();

                        if (extractedRoomNumber.Equals(roomNumber, StringComparison.OrdinalIgnoreCase) &&
                            extractedCCCD.Equals(cccd, StringComparison.OrdinalIgnoreCase) &&
                            checkIn.Equals(checkInDate, StringComparison.OrdinalIgnoreCase) &&
                            checkOut.Equals(checkOutDate, StringComparison.OrdinalIgnoreCase))
                        {
                            count++;
                        }
                    }
                }

                var nextButtons = driver.FindElements(By.XPath("//a[@class='page-link' and normalize-space(text())='Tiếp']"));
                if (nextButtons.Count == 0) break;

                try
                {
                    nextButtons[0].Click();
                    Thread.Sleep(500);
                }
                catch
                {
                    break;
                }
            }

            return count;
        }

        public class BookingModel
        {
            public string CCCD { get; set; }
            public string RoomNumber { get; set; }
            public string RoomType { get; set; }
            public string CheckInDate { get; set; }
            public string CheckOutDate { get; set; }
            public string Status { get; set; }
            public string CreatedBy { get; set; }
        }

        public BookingModel GetBookingDetails(string roomNumber, string cccd, string checkInDate, string checkOutDate)
        {
            if (!DateTime.TryParseExact(checkInDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime checkIn) ||
                !DateTime.TryParseExact(checkOutDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime checkOut))
                return null;

            var row = FindBookingRow(roomNumber, cccd, checkIn, checkOut);
            if (row == null)
                return null;

            var cells = row.FindElements(By.TagName("td"));
            if (cells.Count < 7)
                return null;

            string customerText = cells[0].Text.Trim();
            string extractedCCCD = Regex.Match(customerText, @"\((\d{9,15})\)").Groups[1].Value;

            string roomText = cells[1].Text.Trim();
            string roomNum = roomText.Split('(')[0].Trim();
            string roomType = Regex.Match(roomText, @"\((.*?)\)").Groups[1].Value;

            return new BookingModel
            {
                CCCD = extractedCCCD,
                RoomNumber = roomNum,
                RoomType = roomType,
                CheckInDate = cells[2].Text.Trim(),
                CheckOutDate = cells[3].Text.Trim(),
                Status = cells[4].Text.Trim(),
                CreatedBy = cells[5].Text.Trim()
            };
        }
    }
}
