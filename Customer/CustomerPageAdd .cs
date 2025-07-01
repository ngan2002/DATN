using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using OpenQA.Selenium.Interactions;

namespace DATN.Customer
{
    public class CustomerPageAdd
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;

        public CustomerPageAdd(IWebDriver browser)
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

        // Buttons
        private By BtnAddCustomerLocator => By.CssSelector("button.btn.btn-primary");
        private IWebElement BtnAddCustomer => driver.FindElement(BtnAddCustomerLocator);

        // Form Add Customer
        private By CCCDInputLocator => By.Id("cccd");
        private IWebElement TxtCCCD => driver.FindElement(CCCDInputLocator);

        private By FullNameInputLocator => By.Id("full_name");
        private IWebElement TxtFullName => driver.FindElement(FullNameInputLocator);

        private By PhoneInputLocator => By.Id("phone");
        private IWebElement TxtPhone => driver.FindElement(PhoneInputLocator);

        private By EmailInputLocator => By.Id("email");
        private IWebElement TxtEmail => driver.FindElement(EmailInputLocator);

        private By AddressInputLocator => By.Id("address");
        private IWebElement TxtAddress => driver.FindElement(AddressInputLocator);

        private By BtnSubmitAddCustomerLocator => By.CssSelector("button[type='submit'][name='add_customer']");
        private IWebElement BtnSubmitAddCustomer => driver.FindElement(BtnSubmitAddCustomerLocator);

        private By BtnCloseAddModalLocator => By.CssSelector("#addCustomerModal .modal-footer button.btn-secondary");
        private IWebElement BtnCloseAddModal => driver.FindElement(BtnCloseAddModalLocator);


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
        public void OpenCustomerPage()
        {
            OpenSidebarIfNeeded();
            BtnCustomer.Click();
        }
        /// <summary>
        /// Mở form thêm tài khoản
        /// </summary>
        public void OpenAddCustomerSection()
        {
            OpenSidebarIfNeeded();
            BtnCustomer.Click();
            BtnAddCustomer.Click();
            wait.Until(d => d.FindElement(By.Id("addCustomerModal")).GetAttribute("class").Contains("show"));
        }
        public void AddCustomer(string cccd, string fullName, string phone, string email, string address)
        {
            TxtCCCD.Clear();
            TxtCCCD.SendKeys(cccd);

            TxtFullName.Clear();
            TxtFullName.SendKeys(fullName);

            TxtPhone.Clear();
            TxtPhone.SendKeys(phone);

            TxtEmail.Clear();
            TxtEmail.SendKeys(email);

            TxtAddress.Clear();
            TxtAddress.SendKeys(address);
            BtnSubmitAddCustomer.Click();
        }

        public IWebElement FindCustomerRowByCCCD(string cccd)
        {
            OpenSidebarIfNeeded();
            BtnCustomer.Click(); // Nút mở trang khách hàng

            wait.Until(d => d.FindElements(By.CssSelector("table tbody tr")).Count > 0);

            int maxPages = 20;

            for (int page = 0; page < maxPages; page++)
            {
                var rows = driver.FindElements(By.CssSelector("table tbody tr"));
                foreach (var row in rows)
                {
                    var cells = row.FindElements(By.TagName("td"));
                    if (cells.Count >= 5)
                    {
                        string currentCCCD = cells[0].Text.Trim();
                        if (currentCCCD.Equals(cccd, StringComparison.OrdinalIgnoreCase))
                            return row;
                    }
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
        //Kiểm tra khách hàng tồn tại

        public bool IsCustomerExists(string cccd)
        {
            var row = FindCustomerRowByCCCD(cccd);
            return row != null;
        }
        //Đếm số lần CCCD xuất hiện

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
                    if (cells.Count >= 2)
                    {
                        string cccdText = cells[0].Text.Trim();
                        if (cccdText.Equals(cccd, StringComparison.OrdinalIgnoreCase))
                            count++;
                    }
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
        ///Đếm tổng
        public int CountAllCustomers()
        {
            try
            {
                var modal = driver.FindElement(By.Id("addCustomerModal"));
                if (modal.Displayed && modal.GetAttribute("class").Contains("show"))
                {
                    var closeBtn = modal.FindElement(By.CssSelector(".btn-close"));
                    closeBtn.Click();
                    wait.Until(d => !d.FindElement(By.Id("addCustomerModal")).Displayed);
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
        /// Đóng form thêm khách hàng nếu đang mở
        /// </summary>
        public void CloseForm1()
        {
            try
            {
                var modal = driver.FindElement(By.Id("addCustomerModal"));
                if (modal.Displayed && modal.GetAttribute("class").Contains("show"))
                {
                    // Đóng form bằng nút Đóng
                    BtnCloseAddModal.Click();

                    // Chờ modal đóng hẳn
                    wait.Until(d => !modal.GetAttribute("class").Contains("show"));
                }
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("Modal thêm khách hàng không tồn tại hoặc đã đóng.");
            }
        }

        /// <summary>
        /// Nhập dữ liệu vào form rồi đóng form (không submit)
        /// </summary>
        public void CloseForm2(string cccd, string fullName, string phone, string email, string address)
        {
            TxtCCCD.Clear();
            TxtCCCD.SendKeys(cccd);

            TxtFullName.Clear();
            TxtFullName.SendKeys(fullName);

            TxtPhone.Clear();
            TxtPhone.SendKeys(phone);

            TxtEmail.Clear();
            TxtEmail.SendKeys(email);

            TxtAddress.Clear();
            TxtAddress.SendKeys(address);

            BtnCloseAddModal.Click();

            wait.Until(d => !d.FindElement(By.Id("addCustomerModal")).GetAttribute("class").Contains("show"));
        }


    }
}
