using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Interactions;
using static DATN.Account.PageRepair;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumExtras.WaitHelpers;

namespace DATN.Account
{
    public class AccountDelete
    {


        private IWebDriver driver;
        private WebDriverWait wait;

        public AccountDelete(IWebDriver browser)
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
        public AccountInfo FindUserByUsername(string username)
        {
            OpenSidebarIfNeeded();
            BtnUser.Click();

            wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);
            int maxPages = 20;

            for (int page = 0; page < maxPages; page++)
            {
                var rows = driver.FindElements(By.CssSelector("table tbody tr"));
                foreach (var row in rows)
                {
                    var cells = row.FindElements(By.TagName("td"));
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

                var nextButtons = driver.FindElements(By.XPath("//a[contains(@class,'page-link') and normalize-space(text())='Tiếp']"));
                if (nextButtons.Count == 0 || !nextButtons[0].Enabled || nextButtons[0].GetAttribute("class").Contains("disabled"))
                    break;

                new Actions(driver).MoveToElement(nextButtons[0]).Click().Perform();
                wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);
            }

            return null;
        }
        public bool IsUserExists(string username)
        {
            var user = FindUserByUsername(username);
            return user != null;
        }
        /// <summary>
        /// Xóa tài khoản
        /// </summary>
        public void DeleteUser(string username)
        {
            var userInfo = FindUserByUsername(username);
            Assert.IsNotNull(userInfo, $"Không tìm thấy tài khoản [{username}] trong danh sách tài khoản");

            var row = userInfo.RowElement;
            Assert.IsNotNull(row, "Không xác định được dòng chứa tài khoản.");

            var deleteButton = row.FindElement(By.CssSelector("a.btn.btn-sm.btn-danger"));

            try
            {
                deleteButton.Click();

                WebDriverWait alertWait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                IAlert alert = alertWait.Until(ExpectedConditions.AlertIsPresent());
                alert.Accept(); // Xác nhận xóa

                Console.WriteLine($"Đã click xác nhận xóa tài khoản [{username}].");
            }
            catch (NoAlertPresentException)
            {
                Assert.Fail("Không thấy hộp thoại xác nhận xóa.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Lỗi khi thực hiện xóa: {ex.Message}");
            }
        }

    }
}
