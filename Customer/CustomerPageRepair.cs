using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DATN.Customer.CustomerPageAdd;
using System.Threading;
using OpenQA.Selenium.Interactions;

namespace DATN.Customer
{
    public class CustomerPageRepair
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;

        public CustomerPageRepair(IWebDriver browser)
        {
            driver = browser ?? throw new ArgumentNullException(nameof(browser));
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // Sidebar menu
        private By SidebarMenuLocator => By.Id("sidebarMenu");
        private IWebElement SidebarMenu => driver.FindElement(SidebarMenuLocator);

        // Navigation menu
        private By CustomerMenuLocator => By.CssSelector("#sidebarMenu .nav-link[href='customers.php']");
        private IWebElement BtnCustomer => driver.FindElement(CustomerMenuLocator);

        // Modal sửa khách hàng
        private By EditCustomerModalLocator => By.Id("editCustomerModal");
        private IWebElement EditCustomerModal => driver.FindElement(EditCustomerModalLocator);

        // Trường CCCD (readonly)
        private By EditCCCDLocator => By.Id("edit_cccd");
        private IWebElement TxtEditCCCD => driver.FindElement(EditCCCDLocator);

        // Trường Họ tên
        private By EditFullNameLocator => By.Id("edit_full_name");
        private IWebElement TxtEditFullName => driver.FindElement(EditFullNameLocator);

        // Trường Số điện thoại
        private By EditPhoneLocator => By.Id("edit_phone");
        private IWebElement TxtEditPhone => driver.FindElement(EditPhoneLocator);

        // Trường Email
        private By EditEmailLocator => By.Id("edit_email");
        private IWebElement TxtEditEmail => driver.FindElement(EditEmailLocator);

        // Trường Địa chỉ
        private By EditAddressLocator => By.Id("edit_address");
        private IWebElement TxtEditAddress => driver.FindElement(EditAddressLocator);

        // Nút lưu thay đổi
        private By BtnSubmitEditCustomerLocator => By.CssSelector("button[type='submit'][name='update_customer']");
        private IWebElement BtnSubmitEditCustomer => driver.FindElement(BtnSubmitEditCustomerLocator);

        // Nút đóng modal
        private By BtnCloseEditCustomerModalLocator => By.CssSelector("button.btn.btn-secondary[data-bs-dismiss='modal']");
        private IWebElement BtnCloseEditCustomerModal => driver.FindElement(BtnCloseEditCustomerModalLocator);


        // hàm cập nhật
        public void EditCustomer(string cccd, string newName, string newPhone, string newEmail, string newAddress)
        {
            wait.Until(d => EditCustomerModal.Displayed && EditCustomerModal.GetAttribute("class").Contains("show"));

            // CCCD là readonly nên không cập nhật
            if (!string.IsNullOrEmpty(newName))
            {
                TxtEditFullName.Clear();
                TxtEditFullName.SendKeys(newName);
            }

            if (!string.IsNullOrEmpty(newPhone))
            {
                TxtEditPhone.Clear();
                TxtEditPhone.SendKeys(newPhone);
            }

            if (!string.IsNullOrEmpty(newEmail))
            {
                TxtEditEmail.Clear();
                TxtEditEmail.SendKeys(newEmail);
            }

            if (!string.IsNullOrEmpty(newAddress))
            {
                TxtEditAddress.Clear();
                TxtEditAddress.SendKeys(newAddress);
            }

            BtnSubmitEditCustomer.Click();
        }
        public void OpenEditCustomerModal(string oldcccd)
        {
            var row = FindCustomerRowByCCCD(oldcccd);
            Assert.IsNotNull(row, $"Không tìm thấy khách hàng có CCCD: {oldcccd}");

            var editButton = row.FindElement(By.CssSelector("button.edit-customer"));
            editButton.Click();

            wait.Until(d => d.FindElement(By.Id("editCustomerModal")).GetAttribute("class").Contains("show"));
        }

        public IWebElement FindCustomerRowByCCCD(string cccd)
        {
            OpenSidebarIfNeeded();
            BtnCustomer.Click();

            wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);

            int maxPages = 20;

            for (int page = 0; page < maxPages; page++)
            {
                var rows = driver.FindElements(By.CssSelector("table tbody tr"));
                foreach (var row in rows)
                {
                    var cells = row.FindElements(By.TagName("td"));
                    if (cells.Count >= 5 && cells[0].Text.Trim().Equals(cccd, StringComparison.OrdinalIgnoreCase))
                        return row;
                }

                var nextButtons = driver.FindElements(By.XPath("//a[@class='page-link' and normalize-space(text())='Tiếp']"));
                if (nextButtons.Count == 0) break;

                var nextButton = nextButtons[0];
                driver.FindElement(By.TagName("body")).SendKeys(Keys.PageDown);
                Thread.Sleep(300);

                wait.Until(d => nextButton.Displayed && nextButton.Enabled);

                try { nextButton.Click(); }
                catch (ElementClickInterceptedException) { return null; }

                wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);
            }

            Console.WriteLine($"Không tìm thấy khách hàng CCCD: {cccd}");
            return null;
        }

        public bool IsCustomerExists(string cccd)
        {
            return FindCustomerRowByCCCD(cccd) != null;
        }

        public int CountCustomer(string cccd)
        {
            OpenSidebarIfNeeded();
            BtnCustomer.Click();

            int count = 0;
            int maxPages = 20;
            int currentPage = 0;

            while (currentPage < maxPages)
            {
                var rows = driver.FindElements(By.CssSelector("table tbody tr"));

                foreach (var row in rows)
                {
                    var cells = row.FindElements(By.TagName("td"));
                    if (cells.Count >= 1 && cells[0].Text.Trim().Equals(cccd, StringComparison.OrdinalIgnoreCase))
                        count++;
                }

                try
                {
                    var nextButton = driver.FindElement(By.XPath("//a[@class='page-link' and text()='Tiếp']"));
                    if (!nextButton.Enabled || nextButton.GetAttribute("class").Contains("disabled")) break;

                    nextButton.Click();
                    Thread.Sleep(1000);
                    currentPage++;
                }
                catch { break; }
            }

            return count;
        }

        public class CustomerModel
        {
            public string CCCD { get; set; }
            public string FullName { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Address { get; set; }
        }

        public CustomerModel GetCustomerDetails(string cccd)
        {
            var row = FindCustomerRowByCCCD(cccd);
            if (row == null) return null;

            var cells = row.FindElements(By.TagName("td"));
            if (cells.Count < 5) return null;

            return new CustomerModel
            {
                CCCD = cells[0].Text.Trim(),
                FullName = cells[1].Text.Trim(),
                Phone = cells[2].Text.Trim(),
                Email = cells[3].Text.Trim(),
                Address = cells[4].Text.Trim()
            };
        }

        public int CountAllCustomers()
        {
            try
            {
                var modal = driver.FindElement(By.Id("editCustomerModal"));
                if (modal.Displayed && modal.GetAttribute("class").Contains("show"))
                {
                    var closeBtn = modal.FindElement(By.CssSelector(".btn-close"));
                    closeBtn.Click();
                    wait.Until(d => !d.FindElement(By.Id("editCustomerModal")).Displayed);
                }
            }
            catch (NoSuchElementException) { }

            OpenSidebarIfNeeded();
            BtnCustomer.Click();

            int totalCount = 0;
            int maxPages = 50;

            for (int page = 1; page <= maxPages; page++)
            {
                wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);
                var rows = driver.FindElements(By.CssSelector("table tbody tr"));
                totalCount += rows.Count;

                var nextButtons = driver.FindElements(By.XPath("//a[contains(@class,'page-link') and normalize-space(text())='Tiếp']"));
                if (nextButtons.Count == 0) break;

                var nextButton = nextButtons[0];
                if (!nextButton.Enabled || nextButton.GetAttribute("class").Contains("disabled")) break;

                try
                {
                    new Actions(driver).MoveToElement(nextButton).Click().Perform();
                    wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);
                }
                catch { break; }
            }

            return totalCount;
        }

        public void CloseEditCustomerModal()
        {
            if (EditCustomerModal.Displayed && EditCustomerModal.GetAttribute("class").Contains("show"))
            {
                BtnCloseEditCustomerModal.Click();
                wait.Until(d => !EditCustomerModal.Displayed);
            }
        }

        public CustomerModel GetEditedCustomerDetails(string cccd)
        {
            return GetCustomerDetails(cccd);
        }

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
    }
}
