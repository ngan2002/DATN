using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using static DATN.Account.AccountPage;
using System.Threading;
using System.Globalization;

namespace DATN.Account
{
    public class PageRepair
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;

        public PageRepair(IWebDriver browser)
        {
            driver = browser ?? throw new ArgumentNullException(nameof(browser));
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // ----------------------------- Sidebar và menu -----------------------------
        private By SidebarMenuLocator => By.Id("sidebarMenu");
        private IWebElement SidebarMenu => driver.FindElement(SidebarMenuLocator);

        private By UserMenuLocator => By.CssSelector("#sidebarMenu .nav-link[href='users.php']");
        private IWebElement BtnUser => driver.FindElement(UserMenuLocator);

        // Modal sửa tài khoản
        private By EditUserModalLocator => By.Id("editUserModal");
        private IWebElement EditUserModal => driver.FindElement(EditUserModalLocator);

        private By EditUsernameLocator => By.Id("edit_username");
        private IWebElement TxtEditUsername => driver.FindElement(EditUsernameLocator);

        private By EditPasswordLocator => By.Id("edit_password");
        private IWebElement TxtEditPassword => driver.FindElement(EditPasswordLocator);

        private By EditMaNVLocator => By.Id("edit_maNV");
        private IWebElement SelectEditMaNV => driver.FindElement(EditMaNVLocator);

        private By EditRoleLocator => By.Id("edit_role");
        private IWebElement SelectEditRole => driver.FindElement(EditRoleLocator);

        private By BtnSubmitEditLocator => By.CssSelector("button[type='submit'][name='update_user']");
        private IWebElement BtnSubmitEdit => driver.FindElement(BtnSubmitEditLocator);

        private By BtnCloseEditModalLocator => By.CssSelector("button.btn.btn-secondary[data-bs-dismiss='modal']");
        private IWebElement BtnCloseEditModal => driver.FindElement(BtnCloseEditModalLocator);

        //Thông tin tài khoản
        public class AccountInfo
        {
            public string Username { get; set; }
            public string MaNV { get; set; }
            public string Role { get; set; }
            public DateTime CreatedDate { get; set; }
            public IWebElement RowElement { get; set; }
        }

        // Lấy chi tiết người dùng 
        public AccountInfo GetUserDetails(string username)
        {
            var row = FindUserByUsername(username);
            if (row == null) return null;

            var cells = row.RowElement.FindElements(By.TagName("td"));
            if (cells.Count < 4) return null;

            DateTime createdDate;
            DateTime.TryParseExact(
                cells[3].Text.Trim(),                      // Cột thứ 4: ngày tạo
                "dd/MM/yyyy HH:mm",                       // Format chuẩn
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out createdDate
            );

            return new AccountInfo
            {
                Username = cells[0].Text.Trim(),          // Cột 1: username
                MaNV = cells[1].Text.Trim(),              // Cột 2: mã nhân viên
                Role = cells[2].Text.Trim(),              // Cột 3: quyền
                CreatedDate = createdDate                 // Cột 4: ngày tạo
            };
        }


        // Mở sidebar nếu cần 
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

        // Mở trang sửa tài khoản 
        public void OpenEditUserPage()
        {
            OpenSidebarIfNeeded();
            BtnUser.Click();
        }

        // Tìm tài khoản theo username 
        public AccountInfo FindUserByUsername(string username)
        {
            OpenSidebarIfNeeded();
            BtnUser.Click();

            wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);

            int maxPages = 20; // Giới hạn số trang để tránh vòng lặp vô hạn

            for (int page = 0; page < maxPages; page++)
            {
                var rows = driver.FindElements(By.CssSelector("table tbody tr"));
                foreach (var row in rows)
                {
                    var cells = row.FindElements(By.TagName("td"));

                    // Kiểm tra đủ cột (Username, Mã NV, Role)
                    if (cells.Count >= 3)
                    {
                        string currentUsername = cells[0].Text.Trim();
                        if (currentUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
                        {
                            return new AccountInfo
                            {
                                Username = currentUsername,
                                MaNV = cells[1].Text.Trim(),
                                Role = cells[2].Text.Trim(),
                                RowElement = row
                            };
                        }
                    }
                }

                // Kiểm tra và chuyển trang nếu có nút "Tiếp"
                var nextButtons = driver.FindElements(By.XPath("//a[contains(@class,'page-link') and normalize-space(text())='Tiếp']"));
                if (nextButtons.Count == 0 || !nextButtons[0].Enabled || nextButtons[0].GetAttribute("class").Contains("disabled"))
                    break;

                // Nhấn nút "Tiếp" sang trang sau
                try
                {
                    new Actions(driver).MoveToElement(nextButtons[0]).Click().Perform();
                    wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi chuyển trang: {ex.Message}");
                    break;
                }
            }

            // Không tìm thấy
            return null;
        }

        // Mở form sửa tài khoản
        public void OpenEditUserForm(string oldUsername)
        {
            var user = FindUserByUsername(oldUsername);
            if (user == null)
                Assert.Fail($"Không tìm thấy tài khoản [{oldUsername}] trong danh sách.");

            var btnEdit = user.RowElement.FindElement(By.CssSelector("button.edit-user"));
            wait.Until(d => btnEdit.Displayed && btnEdit.Enabled);

            try
            {
                btnEdit.Click();
            }
            catch (ElementClickInterceptedException)
            {
                new Actions(driver).MoveToElement(btnEdit).Click().Perform();
            }

            wait.Until(d =>
            {
                var modal = d.FindElement(EditUserModalLocator);
                return modal.Displayed && modal.GetAttribute("class").Contains("show");
            });
        }

        // Cập nhật tài khoản (mặc định) 
        public void UpdateUser(string newUsername, string newPassword, string newMaNV, string newRole)
        {
            TxtEditUsername.Clear();
            TxtEditUsername.SendKeys(newUsername);

            TxtEditPassword.Clear();
            TxtEditPassword.SendKeys(newPassword);

            new SelectElement(SelectEditMaNV).SelectByValue(newMaNV);
            new SelectElement(SelectEditRole).SelectByValue(newRole.ToLower());

            BtnSubmitEdit.Click();
        }

        // Cập nhật tài khoản (chỉ truyền giá trị cần sửa)
        public void UpdateUser1(string newUsername, string newPassword, string newMaNV, string newRole)
        {
            if (!string.IsNullOrEmpty(newUsername))
            {
                TxtEditUsername.Clear();
                TxtEditUsername.SendKeys(newUsername);
            }

            if (!string.IsNullOrEmpty(newPassword))
            {
                TxtEditPassword.Clear();
                TxtEditPassword.SendKeys(newPassword);
            }

            if (!string.IsNullOrEmpty(newMaNV))
            {
                new SelectElement(SelectEditMaNV).SelectByValue(newMaNV);
            }

            if (!string.IsNullOrEmpty(newRole))
            {
                new SelectElement(SelectEditRole).SelectByValue(newRole.ToLower());
            }

            BtnSubmitEdit.Click();
        }

        //  Kiểm tra tồn tại 
        public bool IsUserExists(string username)
        {
            var user = FindUserByUsername(username);
            return user != null;
        }

        public int CountUserByUsername(string username)
        {
            OpenSidebarIfNeeded();
            BtnUser.Click();

            int count = 0, maxPages = 20, currentPage = 0;

            while (currentPage < maxPages)
            {
                var rows = driver.FindElements(By.CssSelector("table tbody tr"));
                foreach (var row in rows)
                {
                    var cells = row.FindElements(By.TagName("td"));
                    if (cells.Count >= 1 && cells[0].Text.Trim().Equals(username, StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                    }
                }

                try
                {
                    var nextButton = driver.FindElement(By.XPath("//a[@class='page-link' and text()='Tiếp']"));
                    if (!nextButton.Enabled || nextButton.GetAttribute("class").Contains("disabled"))
                        break;

                    nextButton.Click();
                    Thread.Sleep(1000);
                    currentPage++;
                }
                catch
                {
                    break;
                }
            }

            return count;
        }

        //Đếm tất cả người dùng 
        public int CountAllUsers()
        {
            OpenSidebarIfNeeded();
            BtnUser.Click();

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
                    wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);
                }
                catch
                {
                    break;
                }
            }

            return totalCount;
        }
    }
}
