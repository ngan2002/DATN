using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Principal;
using System.Globalization;
using OpenQA.Selenium.Interactions;

namespace DATN.Account
{
    public class AccountPage
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;

        public AccountPage(IWebDriver browser)
        {
            driver = browser ?? throw new ArgumentNullException(nameof(browser));
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // Sidebar menu
        private By SidebarMenuLocator => By.Id("sidebarMenu");
        private IWebElement SidebarMenu => driver.FindElement(SidebarMenuLocator);

        // Navigation menu
        private By UserMenuLocator => By.CssSelector("#sidebarMenu .nav-link[href='users.php']");
        private IWebElement BtnUser => driver.FindElement(UserMenuLocator);

        // Buttons
        private By BtnAddUserLocator => By.CssSelector("button.btn.btn-primary");
        private IWebElement BtnAddUser => driver.FindElement(BtnAddUserLocator);

        // Form Add user
        private By UserInputLocator => By.Id("username");
        private IWebElement TxtUser => driver.FindElement(UserInputLocator);

        private By PassInputLocator => By.Id("password");
        private IWebElement TxtPass => driver.FindElement(PassInputLocator);

        private By ConfirmPassInputLocator => By.Id("confirm_password");
        private IWebElement TxtConfirmPass => driver.FindElement(ConfirmPassInputLocator);

        private By NVSelectLocator => By.Id("maNV");
        private IWebElement SelectNV => driver.FindElement(NVSelectLocator);

        private By RoleSelectLocator => By.Id("role");
        private IWebElement SelectRole => driver.FindElement(RoleSelectLocator);

        private By BtnSubmitAddUserLocator => By.CssSelector("button[type='submit'][name='add_user']");
        private IWebElement BtnSubmitAddUser => driver.FindElement(BtnSubmitAddUserLocator);

        private By BtnCloseModalLocator => By.CssSelector("button.btn.btn-secondary[data-bs-dismiss='modal']");
        private IWebElement BtnCloseModal => driver.FindElement(BtnCloseModalLocator);

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
        /// Mở trang tài khoản
        /// </summary>
        public void OpenUserPage()
        {
            OpenSidebarIfNeeded();
            BtnUser.Click();
        }
        /// <summary>
        /// Mở form thêm tài khoản
        /// </summary>
        public void OpenAddUserSection()
        {
            OpenSidebarIfNeeded();
            BtnUser.Click();
            BtnAddUser.Click();
            wait.Until(d => d.FindElement(By.Id("addUserModal")).GetAttribute("class").Contains("show"));
        }
        /// <summary>
        /// Thêm Tài khoản mới
        /// </summary>

        public void AddUserAccount(string username, string password, string confirmPassword, string maNV, string role)
        {
            OpenAddUserSection();

            TxtUser.Clear();
            TxtUser.SendKeys(username);

            TxtPass.Clear();
            TxtPass.SendKeys(password);

            TxtConfirmPass.Clear();
            TxtConfirmPass.SendKeys(confirmPassword);

            // Chọn nhân viên
            new SelectElement(SelectNV).SelectByValue(maNV); // "" nếu để trống

            // Chọn quyền: theo value hoặc text tùy thuộc biến truyền vào
            new SelectElement(SelectRole).SelectByValue(role.ToLower()); // dùng "staff", "admin"
                                                                        

            BtnSubmitAddUser.Click();
        }

        /// <summary>
        /// Tìm tài khoản theo username
        /// </summary>
        public IWebElement FindUserRowByUsername(string username)
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
                        string currentUsername = cells[0].Text.Trim(); // Username nằm ở cột đầu tiên
                        if (currentUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
                        {
                            return row;
                        }
                    }
                }

                // Tìm nút "Tiếp" trong phân trang
                var nextButtons = driver.FindElements(By.XPath("//a[@class='page-link' and normalize-space(text())='Tiếp']"));
                if (nextButtons.Count == 0)
                    break;

                var nextButton = nextButtons[0];

                // Cuộn xuống để tránh bị che
                var body = driver.FindElement(By.TagName("body"));
                body.SendKeys(Keys.PageDown);
                Thread.Sleep(300); // Đợi cuộn

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

                // Chờ trang tải lại danh sách
                wait.Until(driver => driver.FindElements(By.CssSelector("table tbody tr")).Count > 0);
            }

            Console.WriteLine($"Không tìm thấy tài khoản: {username}");
            return null;
        }

        //kiểm tra sự tồn tại phòng mới
        public bool IsUserExists(string username)
        {
            var row = FindUserRowByUsername(username);
            return row != null;
        }
        /// <summary>
        /// Đếm số lần xuất hiện của username trong danh sách tài khoản người dùng.
        /// </summary>
        /// 
        public int CountUsername(string username)
        {
            int count = 0;
            int maxPages = 20; // Giới hạn duyệt tối đa 20 trang
            int currentPage = 0;

            // Đợi bảng tải xong trước khi bắt đầu
            wait.Until(driver => driver.FindElements(By.CssSelector("table tbody tr")).Count > 0);

            while (currentPage < maxPages)
            {
                // Lấy các dòng trong bảng
                var rows = driver.FindElements(By.CssSelector("table tbody tr"));

                foreach (var row in rows)
                {
                    var cells = row.FindElements(By.TagName("td"));

                    if (cells.Count >= 2)
                    {
                        string usernameText = cells[0].Text.Trim();

                        if (usernameText.Equals(username, StringComparison.OrdinalIgnoreCase))
                        {
                            count++;
                        }
                    }
                }

                try
                {
                    // Tìm nút "Tiếp"
                    var nextButton = driver.FindElement(By.XPath("//a[@class='page-link' and normalize-space(text())='Tiếp']"));

                    if (!nextButton.Enabled || nextButton.GetAttribute("class").Contains("disabled"))
                    {
                        break;
                    }

                    nextButton.Click();

                    // Chờ trang tải lại
                    Thread.Sleep(1000); // Có thể thay bằng WebDriverWait nếu cần chính xác hơn

                    currentPage++;
                }
                catch (NoSuchElementException)
                {
                    break; // Không còn nút "Tiếp", dừng lại
                }
            }

            return count;
        }

        public class UserModel
        {
            public string Username { get; set; }
            public string FullName { get; set; }
            public string Role { get; set; }
            //public string CreatedDate { get; set; }
        }
        public UserModel GetUserDetails(string username)
        {
            var row = FindUserRowByUsername(username);
            if (row == null)
                return null;

            var cells = row.FindElements(By.TagName("td"));
            if (cells.Count < 4)
                return null;

            return new UserModel
            {
                Username = cells[0].Text.Trim(),
                FullName = cells[1].Text.Trim(),
                Role = cells[2].Text.Trim(),
                
                // Nếu cần xử lý thêm cột thao tác (vd: nút Xóa), có thể thêm ở đây
            };
        }

        public int CountAllUsers()
        {
            // Đóng modal "Thêm người dùng" nếu đang mở (nếu có)
            try
            {
                var modal = driver.FindElement(By.Id("addUserModal"));
                if (modal.Displayed && modal.GetAttribute("class").Contains("show"))
                {
                    BtnCloseModal.Click();

                    // Chờ modal đóng hoàn toàn
                    wait.Until(d =>
                    {
                        try
                        {
                            return !d.FindElement(By.Id("addUserModal")).Displayed;
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
                // Không có modal mở
            }

            OpenSidebarIfNeeded();
            BtnUser.Click(); // Mở trang quản lý người dùng

            int totalCount = 0;
            int maxPages = 50;

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

                    // Đợi dữ liệu trang mới được load
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

        // Modal sửa tài khoản
        private By EditUserModalLocator => By.Id("editUserModal");
        private IWebElement EditUserModal => driver.FindElement(EditUserModalLocator);

        // Input: Tên đăng nhập
        private By EditUsernameLocator => By.Id("edit_username");
        private IWebElement TxtEditUsername => driver.FindElement(EditUsernameLocator);

        // Input: Mật khẩu (trống nếu không đổi)
        private By EditPasswordLocator => By.Id("edit_password");
        private IWebElement TxtEditPassword => driver.FindElement(EditPasswordLocator);

        // Select: Nhân viên
        private By EditMaNVLocator => By.Id("edit_maNV");
        private IWebElement SelectEditMaNV => driver.FindElement(EditMaNVLocator);

        // Select: Vai trò
        private By EditRoleLocator => By.Id("edit_role");
        private IWebElement SelectEditRole => driver.FindElement(EditRoleLocator);

        // Button: Lưu thay đổi
        private By BtnSubmitEditLocator => By.CssSelector("button[type='submit'][name='update_user']");
        private IWebElement BtnSubmitEdit => driver.FindElement(BtnSubmitEditLocator);

        // Button: Đóng modal
        private By BtnCloseEditModalLocator => By.CssSelector("button.btn.btn-secondary[data-bs-dismiss='modal']");
        private IWebElement BtnCloseEditModal => driver.FindElement(BtnCloseEditModalLocator);

    }
}

          

