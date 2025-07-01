using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DATN.CheckIn_CheckOut
{
    internal class CheckOut_2Page
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;

        public CheckOut_2Page(IWebDriver browser)
        {
            driver = browser ?? throw new ArgumentNullException(nameof(browser));
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        private By SidebarMenuLocator => By.Id("sidebarMenu");
        private IWebElement SidebarMenu => driver.FindElement(SidebarMenuLocator);

        private By CheckIn_OutMenuLocator => By.CssSelector("#sidebarMenu .nav-link[href='checkin_checkout.php']");
        private IWebElement BtnCheckIn_out => driver.FindElement(CheckIn_OutMenuLocator);


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
        public void OpenCheckOutPage()
        {
            OpenSidebarIfNeeded();
            BtnCheckIn_out.Click();
            wait.Until(driver => driver.Url.Contains("checkin_checkout.php"));
        }
        public IWebElement FindCheckOutRowByInfo(string roomNumber, string customerName, DateTime checkIn)
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
                        string customerText = cells[1].Text.Trim();
                        string roomText = cells[2].Text.Trim();
                        string checkOutText = cells[3].Text.Trim();

                        string extractedRoomNumber = Regex.Match(roomText, @"^\S+").Value;

                        if (customerText.Equals(customerName, StringComparison.OrdinalIgnoreCase) &&
                            extractedRoomNumber == roomNumber &&
                            checkOutText == checkIn.ToString("dd/MM/yyyy"))
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
                    Thread.Sleep(500); // đợi trang chuyển
                }
                catch
                {
                    break;
                }
            }

            return null;
        }
        public bool IsBookingPresent(string roomNumber, string customerName, DateTime checkIn)
        {
            return FindCheckOutRowByInfo(roomNumber, customerName, checkIn) != null;
        }
        ////------------Đặt Phòng
        private By BookingMenuLocator => By.CssSelector("#sidebarMenu .nav-link[href='bookings.php']");
        private IWebElement BtnBooking => driver.FindElement(BookingMenuLocator);

        private By BtnAddBookingLocator => By.CssSelector("button.btn.btn-primary");
        private IWebElement BtnAddBooking => driver.FindElement(BtnAddBookingLocator);
        public void OpenBookingPage()
        {
            OpenSidebarIfNeeded();
            BtnBooking.Click();
            wait.Until(driver => driver.Url.Contains("bookings.php"));
        }

        public IWebElement FindBookingRow(string roomNumber, string customerName, DateTime checkOutDate, string expectedStatus = null)
        {
            wait.Until(driver => driver.FindElements(By.CssSelector("table tbody tr")).Count > 0);

            for (int page = 0; page < 20; page++)
            {
                var rows = driver.FindElements(By.CssSelector("table tbody tr"));
                foreach (var row in rows)
                {
                    var cells = row.FindElements(By.TagName("td"));
                    if (cells.Count >= 6)
                    {
                        string customerCellText = cells[0].Text.Trim();
                        string extractedName = customerCellText.Split('(')[0].Trim();
                        string roomCellText = cells[1].Text.Trim();
                        string extractedRoom = roomCellText.Split(' ')[0].Trim();
                        string checkOutText = cells[3].Text.Trim();
                        string status = cells[4].Text.Trim().ToLower();



                        bool match =
                            extractedName.Equals(customerName, StringComparison.OrdinalIgnoreCase) &&
                            extractedRoom == roomNumber &&
                            checkOutText == checkOutDate.ToString("dd/MM/yyyy");

                        if (expectedStatus != null)
                            match = match && status.Contains(expectedStatus.ToLower());

                        if (match)
                        {
                            Console.WriteLine($"Tìm thấy dòng khớp: {extractedName} - {extractedRoom} - {checkOutText} - {status}");
                            return row;
                        }
                    }
                }

                var nextButtons = driver.FindElements(By.XPath("//a[@class='page-link' and normalize-space(text())='Tiếp']"));
                if (nextButtons.Count == 0)
                {
                    Console.WriteLine("Không còn nút 'Tiếp' -> kết thúc tìm kiếm.");
                    break;
                }

                try
                {

                    Actions actions = new Actions(driver);
                    actions.MoveToElement(nextButtons[0]).Click().Perform();

                    Thread.Sleep(500);
                    wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi click nút 'Tiếp': {ex.Message} -> thoát vòng lặp.");
                    break;
                }
            }

            Console.WriteLine("Không tìm thấy dòng đặt phòng khớp sau khi duyệt toàn bộ trang.");
            return null;
        }

    }
}
