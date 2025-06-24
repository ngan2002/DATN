using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;
using static DATN.Rooms.RoomPage;

namespace DATN.Rooms
{
    public class RoomPageRepair
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public RoomPageRepair(IWebDriver browser)
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

        // Buttons đặt phòng trong màn booking

        private By BtnBookRoomLocator => By.CssSelector("button.btn.btn-primary");
        private IWebElement BtnBookRoom => driver.FindElement(BtnBookRoomLocator);

        // Form Edit room
        private By EditRoomNumberInputLocator => By.Id("edit_room_number");
        private IWebElement TxtEditRoomNumber => driver.FindElement(EditRoomNumberInputLocator);

        private By EditRoomTypeSelectLocator => By.Id("edit_type");
        private IWebElement SelectEditRoomType => driver.FindElement(EditRoomTypeSelectLocator);

        private By EditPriceInputLocator => By.Id("edit_price");
        private IWebElement TxtEditPrice => driver.FindElement(EditPriceInputLocator);

        private By EditStatusSelectLocator => By.Id("edit_status");
        private IWebElement SelectEditStatus => driver.FindElement(EditStatusSelectLocator);

        private By BtnSaveEditLocator => By.Name("update_room");
        private IWebElement BtnSaveEdit => driver.FindElement(BtnSaveEditLocator);

        public IWebElement BtnCloseEditModal => driver.FindElement(By.CssSelector("#editRoomModal .btn.btn-secondary[data-bs-dismiss='modal']"));


        private By BtnRemoveLocator => By.CssSelector("button.btn.btn-sm.btn-danger");
        private IWebElement BtnRemove => driver.FindElement(BtnRemoveLocator);


        // Pagination next button xpath
        private By NextPageButtonXPath => By.XPath("//a[@class='page-link' and text()='Tiếp']");

        // Mở sidebar nếu ẩn
        public void OpenSidebarIfNeeded()
        {
            wait.Until(d => d.FindElement(SidebarMenuLocator));
            var sidebar = driver.FindElement(SidebarMenuLocator);
            if (!sidebar.Displayed)
            {
                var toggleButton = driver.FindElement(By.CssSelector("button.navbar-toggler"));
                toggleButton.Click();
                wait.Until(d => sidebar.Displayed);
            }
        }
        /// Mở Form đặt phòng
        public void OpenBookingPage()
        {
            OpenSidebarIfNeeded();
            var BookingMenu = driver.FindElement(By.CssSelector("#sidebarMenu .nav-link[href='bookings.php']"));
            BookingMenu.Click();
            var BtnAddBooking = driver.FindElement(By.CssSelector("button.btn.btn-primary"));
            BtnAddBooking.Click();

        }
        public void VerifyRoomInBookingDropdown(string roomNumberExpected, string roomTypeExpected, string priceFormatted, string roomNumberOld = null)
        {
            // Mở trang đặt phòng và lấy combobox phòng
            OpenBookingPage();
            var roomDropdown = wait.Until(d => d.FindElement(By.Id("room_id")));
            var dropdownOptions = new SelectElement(roomDropdown).Options;
            var optionTexts = dropdownOptions.Select(o => o.Text.Trim()).ToList();

            // 1. Kiểm tra phòng mới xuất hiện đúng 1 lần (so sánh chính xác)
            int newRoomCount = optionTexts.Count(text => text.StartsWith(roomNumberExpected + " "));
            Assert.AreEqual(1, newRoomCount, $"Phòng [{roomNumberExpected}] không xuất hiện đúng 1 lần trong combobox.");

            // 2. Kiểm tra thông tin kèm theo (Loại phòng và Giá)
            bool hasCorrectInfo = optionTexts.Any(text =>
                text.StartsWith(roomNumberExpected + " ") &&
                text.Contains("(" + roomTypeExpected + ")") &&
                text.Contains(priceFormatted)
            );
            Assert.IsTrue(hasCorrectInfo,
                $"Phòng [{roomNumberExpected}] không có đầy đủ thông tin đúng: loại [{roomTypeExpected}], giá [{priceFormatted}].");

            // 3. Nếu có phòng cũ được chỉ định, kiểm tra phòng đó không còn xuất hiện (so sánh chính xác)
            if (roomNumberOld != null)
            {
                bool oldRoomStillExists = optionTexts.Any(text =>
                    text.StartsWith(roomNumberOld + " ") || text.Equals(roomNumberOld)
                );
                Assert.IsFalse(oldRoomStillExists,
                    $"Phòng cũ [{roomNumberOld}] vẫn còn xuất hiện trong combobox.");
            }

            Console.WriteLine($"Phòng [{roomNumberExpected}] đã được cập nhật chính xác trong combobox đặt phòng.");
        }



        // Mở trang Room
        public void OpenRoomPage()
        {
            OpenSidebarIfNeeded();
            var btnRoom = driver.FindElement(RoomMenuLocator);
            btnRoom.Click();
            wait.Until(d => d.Url.Contains("rooms.php"));
            wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);
        }
        public class BookInfo
        {
            public string RoomNumber { get; set; }
            public string RoomType { get; set; }
            public string Status { get; set; }
            public string Price { get; set; }
            public IWebElement RowElement { get; set; }
        }

        public BookInfo FindRoomByNumber(string roomNumber)
        {
            OpenSidebarIfNeeded();
            BtnRoom.Click();

            // Đợi bảng có dữ liệu
            wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);

            int maxPages = 20;

            for (int page = 0; page < maxPages; page++)
            {
                // Lấy tất cả các dòng trong bảng
                var rows = driver.FindElements(By.CssSelector("table tbody tr"));

                foreach (var row in rows)
                {
                    var cells = row.FindElements(By.TagName("td"));

                    if (cells.Count >= 5)
                    {
                        string currentRoomNumber = cells[0].Text.Trim();

                        if (currentRoomNumber.Equals(roomNumber, StringComparison.OrdinalIgnoreCase))
                        {
                            // Tìm thấy phòng, trả về luôn
                            return new BookInfo
                            {
                                RoomNumber = currentRoomNumber,
                                RoomType = cells[1].Text.Trim(),
                                Price = cells[2].Text.Trim(),
                                Status = cells[3].Text.Trim(),
                                RowElement = row
                            };
                        }
                    }
                }

                // Chưa tìm thấy, kiểm tra nút "Tiếp" để chuyển trang
                var nextButtons = driver.FindElements(By.XPath("//a[contains(@class,'page-link') and normalize-space(text())='Tiếp']"));

                if (nextButtons.Count == 0)
                {
                    // Không có nút tiếp => hết trang
                    break;
                }

                var nextButton = nextButtons[0];

                // Nếu nút tiếp bị vô hiệu hoặc không nhấn được thì dừng
                if (!nextButton.Enabled || nextButton.GetAttribute("class").Contains("disabled"))
                {
                    break;
                }

                // Click nút "Tiếp" để sang trang kế tiếp
                new Actions(driver).MoveToElement(nextButton).Click().Perform();

                // Đợi bảng dữ liệu load xong ở trang mới
                wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);

                // Tiếp tục vòng lặp để duyệt trang mới
            }

            // Không tìm thấy phòng trong tất cả các trang đã duyệt
            return null;
        }




        // Kiểm tra sự tồn tại của phòng
        public bool IsRoomExists(string roomNumber)
        {
            var room = FindRoomByNumber(roomNumber);
            return room != null;
        }

        public void OpenEditRoomForm(string oldRoomNumber)
        {
            // Tìm phòng theo số phòng
            var room = FindRoomByNumber(oldRoomNumber);

            // Kiểm tra phòng có tồn tại không
            if (room == null)
            {
                Assert.Fail($"Không tìm thấy phòng [{oldRoomNumber}] trong danh sách.");
            }

            // Tìm nút sửa trong dòng phòng đó
            var btnEdit = room.RowElement.FindElement(By.CssSelector("button.edit-room"));

            // Tạo WebDriverWait để đợi nút sửa hiển thị và có thể click được
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => btnEdit.Displayed && btnEdit.Enabled);

            try
            {
                // Cố gắng click nút sửa
                btnEdit.Click();
            }
            catch (ElementClickInterceptedException)
            {
                // Nếu bị lỗi click intercepted, dùng Actions di chuyển chuột rồi click lại
                var actions = new Actions(driver);
                actions.MoveToElement(btnEdit).Click().Perform();
            }

            // Đợi modal sửa phòng hiện lên
            wait.Until(d =>
            {
                var modal = d.FindElement(By.Id("editRoomModal"));
                return modal.Displayed;
            });
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


        public void UpdateRoom(string newRoomNumber, string newRoomType, string newPrice, string newStatus)
        {
            TxtEditRoomNumber.Clear();
            TxtEditRoomNumber.SendKeys(newRoomNumber);
            new SelectElement(SelectEditRoomType).SelectByText(newRoomType);
            TxtEditPrice.Clear();
            TxtEditPrice.SendKeys(newPrice);
            new SelectElement(SelectEditStatus).SelectByText(newStatus);
            BtnSaveEdit.Click();

        }
        //public string GetGiaValidationMessage() => TxtPrice.GetAttribute("validationMessage");
        /// <summary>
        /// Đóng modal form hiện tại
        /// </summary>
        public void CloseForm1(string oldRoomNumber)
        {

            OpenEditRoomForm(oldRoomNumber);

            BtnCloseEditModal.Click();
        }
        public void CloseForm2(string newRoomNumber, string oldRoomNumber, string newRoomType, string newPrice, string newStatus)
        {
            OpenEditRoomForm(oldRoomNumber);

            TxtEditRoomNumber.Clear();
            TxtEditRoomNumber.SendKeys(newRoomNumber);

            new SelectElement(SelectEditRoomType).SelectByText(newRoomType);

            TxtEditPrice.Clear();
            TxtEditPrice.SendKeys(newPrice);

            new SelectElement(SelectEditStatus).SelectByText(newStatus);

            // Click nút "Đóng"
            BtnCloseEditModal.Click();
        }


    }
}



