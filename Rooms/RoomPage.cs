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
using static DATN.Rooms.RoomPageRepair;

namespace DATN.Rooms
{
    public class RoomPage
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;

        public RoomPage(IWebDriver browser)
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

        // Buttons
        private By BtnAddRoomLocator => By.CssSelector("button.btn.btn-primary");
        private IWebElement BtnAddRoom => driver.FindElement(BtnAddRoomLocator);


        // Form Add room
        private By RoomNumberInputLocator => By.Id("room_number");
        private IWebElement TxtRoomNumber => driver.FindElement(RoomNumberInputLocator);

        private By RoomTypeSelectLocator => By.Id("type");
        private IWebElement SelectRoomType => driver.FindElement(RoomTypeSelectLocator);

        private By PriceInputLocator => By.Id("price");
        private IWebElement TxtPrice => driver.FindElement(PriceInputLocator);

        private By StatusSelectLocator => By.Id("status");
        private IWebElement SelectStatus => driver.FindElement(StatusSelectLocator);

        private By BtnSubmitAddRoomLocator => By.CssSelector("button[type='submit'][name='add_room']");
        private IWebElement BtnSubmitAddRoom => driver.FindElement(BtnSubmitAddRoomLocator);

        private By BtnCloseModalLocator => By.CssSelector("button.btn.btn-secondary[data-bs-dismiss='modal']");
        private IWebElement BtnCloseModal => driver.FindElement(BtnCloseModalLocator);

        /// Mở Form đặt phòng
        public void OpenBookingPage()
        {
            OpenSidebarIfNeeded();
            var BookingMenu = driver.FindElement(By.CssSelector("#sidebarMenu .nav-link[href='bookings.php']"));
            BookingMenu.Click();
            var BtnAddBooking = driver.FindElement(By.CssSelector("button.btn.btn-primary"));
            BtnAddBooking.Click();
           
        }

        /// <summary>
        /// Mở sidebar menu nếu đang ẩn
        /// </summary>
        public void OpenSidebarIfNeeded()
        {
            wait.Until(d => d.FindElement(SidebarMenuLocator));
            var sidebar = SidebarMenu;
            if (!sidebar.Displayed)
            {
                var toggleButton = driver.FindElement(By.CssSelector("button.navbar-toggler"));
                toggleButton.Click();
            }
        }
        /// <summary>
        /// Mở form thêm phòng
        /// </summary>
        public void OpenAddRoomSection()
        {
            OpenSidebarIfNeeded();
            BtnRoom.Click();
            BtnAddRoom.Click();
            wait.Until(d => d.FindElement(By.Id("addRoomModal")).GetAttribute("class").Contains("show"));
        }

        /// <summary>
        /// Thêm phòng mới
        /// </summary>
        public void RoomApplication(string roomNumber, string roomType, string price, string status)
        {
            OpenAddRoomSection();

            TxtRoomNumber.Clear();
            TxtRoomNumber.SendKeys(roomNumber);

            new SelectElement(SelectRoomType).SelectByText(roomType);

            TxtPrice.Clear();
            TxtPrice.SendKeys(price);

            new SelectElement(SelectStatus).SelectByText(status);

            BtnSubmitAddRoom.Click();
        }
//        public int GetTotalRoomCount()
//{
//    var rows = driver.FindElements(By.CssSelector("table#roomTable tbody tr"));
//    return rows.Count;
//}

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
                        string currentRoomNumber = cells[0].Text.Trim();
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
        ///  Số lần xuất hiện trong DS phòng
        /// </summary>
        public int CountRoom(string roomNumber)
        {
            // Mở sidebar nếu chưa mở
            OpenSidebarIfNeeded();

            // Nhấn vào nút phòng
            BtnRoom.Click();

            int count = 0;
            int maxPages = 20; // số trang tối đa để duyệt
            int currentPage = 0;

            while (currentPage < maxPages)
            {
                // Lấy danh sách các dòng trong bảng phòng hiện tại
                var rows = driver.FindElements(By.CssSelector("table tbody tr"));

                // Duyệt từng dòng để kiểm tra số phòng
                foreach (var row in rows)
                {
                    var cells = row.FindElements(By.TagName("td"));

                    if (cells.Count >= 2)
                    {
                        string roomNumText = cells[0].Text.Trim();

                        // Nếu số phòng trùng với roomNumber truyền vào, tăng biến đếm
                        if (roomNumText.Equals(roomNumber, StringComparison.OrdinalIgnoreCase))
                        {
                            count++;
                        }
                    }
                }

                try
                {
                    // Tìm nút "Tiếp" để chuyển trang
                    var nextButton = driver.FindElement(By.XPath("//a[@class='page-link' and text()='Tiếp']"));

                    // Nếu nút "Tiếp" bị vô hiệu hóa thì dừng duyệt trang
                    if (!nextButton.Enabled || nextButton.GetAttribute("class").Contains("disabled"))
                    {
                        break;
                    }

                    // Click nút "Tiếp" để sang trang kế tiếp
                    nextButton.Click();

                    // Đợi một chút cho trang mới tải lại, đơn giản dùng Thread.Sleep thay cho wait phức tạp
                    System.Threading.Thread.Sleep(1000);

                    currentPage++;
                }
                catch
                {
                    break;
                }
            }

            return count;
        }
        public class RoomModel
        {
            public string RoomNumber { get; set; }
            public string RoomType { get; set; }
            public string Price { get; set; }
            public string Status { get; set; }
        }
        public RoomModel GetRoomDetails(string roomNumber)
        {
            var row = FindRoomRowByNumber(roomNumber);
            if (row == null)
                return null;

            var cells = row.FindElements(By.TagName("td"));
            if (cells.Count < 4) // Kiểm tra đủ cột để lấy dữ liệu
                return null;

            return new RoomModel
            {
                RoomNumber = cells[0].Text.Trim(),
                RoomType = cells[1].Text.Trim(),
                Price = cells[2].Text.Trim(),
                Status = cells.Count > 3 ? cells[3].Text.Trim() : ""
            };
        }
        public int CountAllRooms()
        {
            // Đóng modal nếu đang hiển thị (ví dụ modal thêm phòng)
            try
            {
                var modal = driver.FindElement(By.Id("addRoomModal"));
                if (modal.Displayed && modal.GetAttribute("class").Contains("show"))
                {
                    var closeBtn = modal.FindElement(By.CssSelector(".btn-close"));
                    closeBtn.Click();

                    // Đợi modal biến mất
                    wait.Until(d =>
                    {
                        try
                        {
                            return !d.FindElement(By.Id("addRoomModal")).Displayed;
                        }
                        catch (NoSuchElementException)
                        {
                            return true;
                        }
                    });
                }
            }
            catch (NoSuchElementException)
            {
                // Modal không tồn tại => không cần đóng
            }

            OpenSidebarIfNeeded();
            BtnRoom.Click();

            int totalCount = 0;  // Tổng số phòng đếm được
            int maxPages = 50;   // Giới hạn số trang duyệt

            for (int page = 1; page <= maxPages; page++)
            {
                wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);
                var rows = driver.FindElements(By.CssSelector("table tbody tr"));
                totalCount += rows.Count;

                var nextButtons = driver.FindElements(By.XPath("//a[contains(@class,'page-link') and normalize-space(text())='Tiếp']"));

                if (nextButtons.Count == 0)
                    break;

                var nextButton = nextButtons[0];
                if (!nextButton.Enabled || nextButton.GetAttribute("class").Contains("disabled"))
                    break;

                try
                {
                    new Actions(driver).MoveToElement(nextButton).Click().Perform();
                    wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);
                }
                catch (StaleElementReferenceException ex)
                {
                    Console.WriteLine($"Lỗi stale element tại trang {page}: {ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi chuyển trang tại trang {page}: {ex.Message}");
                    break;
                }
            }

            return totalCount;
        }







        //public string GetGiaValidationMessage() => TxtPrice.GetAttribute("validationMessage");
        /// <summary>
        /// Đóng modal form hiện tại
        /// </summary>
        public void CloseForm1()
        {

            OpenAddRoomSection();

            BtnCloseModal.Click();
        }
        public void CloseForm2(string roomNumber, string roomType, string price, string status)
        {
            OpenAddRoomSection();

            TxtRoomNumber.Clear();
            TxtRoomNumber.SendKeys(roomNumber);

            new SelectElement(SelectRoomType).SelectByText(roomType);

            TxtPrice.Clear();
            TxtPrice.SendKeys(price);

            new SelectElement(SelectStatus).SelectByText(status);

            BtnCloseModal.Click();
        }
        //public string GetGiaValidationMessage() => TxtPrice.GetAttribute("validationMessage");
       
    }
}
