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

namespace DATN.Rooms
{
    public class RoomPageDelete
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public RoomPageDelete(IWebDriver browser)
        {
            driver = browser ?? throw new ArgumentNullException(nameof(browser));
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // Sidebar menu
        private By SidebarMenuLocator => By.Id("sidebarMenu");
        private IWebElement SidebarMenu => driver.FindElement(SidebarMenuLocator);

        // Navigation menu
        private By RoomMenuLocator => By.CssSelector("#sidebarMenu .nav-link[href='rooms.php']");
        private IWebElement BtnRoom => driver.FindElement(RoomMenuLocator);


        /// <summary>
        /// Mở sidebar menu nếu đang ẩn
        /// </summary>
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
        // <summary>
        /// Tìm dòng chứa phòng theo số phòng (có xử lý phân trang)
        /// </summary>
        public IWebElement FindRoomRowByNumber(string roomNumber)
        {
            OpenSidebarIfNeeded();
            BtnRoom.Click();

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
                        string currentRoomNumber = cells[1].Text.Trim();
                        if (currentRoomNumber.Equals(roomNumber, StringComparison.OrdinalIgnoreCase))
                        {

                            return row;
                        }
                    }
                }

                // Tìm nút “Tiếp”
                var nextButtons = driver.FindElements(By.XPath("//a[@class='page-link' and normalize-space(text())='Tiếp']"));
                if (nextButtons.Count == 0)
                {

                    break;
                }

                var nextButton = nextButtons[0];

                // Giải pháp cuộn xuống bằng cách gửi phím
                var body = driver.FindElement(By.TagName("body"));
                body.SendKeys(Keys.PageDown);
                Thread.Sleep(300); // Cho scroll xong

                // Đảm bảo nút sẵn sàng để click
                wait.Until(driver => nextButton.Displayed && nextButton.Enabled);

                try
                {
                    nextButton.Click();
                }
                catch (ElementClickInterceptedException ex)
                {
                    Console.WriteLine("Lỗi click nút Tiếp: bị che hoặc chưa vào view.");
                    return null;
                }

                // Chờ bảng load lại
                wait.Until(driver => driver.FindElements(By.CssSelector("table tbody tr")).Count > 0);
            }

            Console.WriteLine($"Không tìm thấy phòng: {roomNumber}");
            return null;
        }



        //kiểm tra sự tồn tại phòng mới
        public bool IsRoomExists(string roomNumber)
        {
            var row = FindRoomRowByNumber(roomNumber);
            return row != null;
        }
        /// <summary>
        /// Xóa phòng - chưa cài đặt, bạn có thể bổ sung logic sau
        /// </summary>
        public void DeleteRoom(string roomNumber)
        {
            var row = FindRoomRowByNumber(roomNumber);
            Assert.IsNotNull(row, $"Không tìm thấy phòng [{roomNumber}] trong danh sách phòng");

            var deleteButton = row.FindElement(By.CssSelector("a.btn.btn-sm.btn-danger"));

            deleteButton.Click();

           

        }

    }
}
