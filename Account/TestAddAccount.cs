using DATN.Rooms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DATN.Account.AccountPage;
using OpenQA.Selenium.Interactions;
using System.Threading;

namespace DATN.Account
{
    [TestClass]
    public class TestAddAccount
    {
        private IWebDriver driver;
        private AccountPage account;
        private WebDriverWait wait;

        [TestInitialize] // chạy trước mỗi test
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost/qlks-ngan_hn/source/login.php");
            driver.FindElement(By.Id("username")).SendKeys("admin");
            driver.FindElement(By.Id("password")).SendKeys("admin123");
            driver.FindElement(By.CssSelector("button.w-100.btn.btn-lg.btn-primary")).Click();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            account = new AccountPage(driver);

        }
        [TestMethod]
     
        public void AddAccount1()
        {
            string username = "NV11";
            string password = "123";
            string confirmPassword = "123";
            string maNV = ""; 
            string role = "Staff";

            account.AddUserAccount(username, password, confirmPassword, maNV, role);

            var successElement = wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(By.CssSelector("div.alert.alert-success"));
                    return element.Displayed ? element : null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
            });

            Assert.AreEqual("Thêm tài khoản thành công!", successElement.Text.Trim(), "Nội dung thông báo thành công không đúng.");
            Console.WriteLine($"Thông báo hiển thị thành công là {successElement.Text.Trim()}");

            Assert.IsTrue(account.IsUserExists(username), $"Không tìm thấy tài khoản [{username}] trong danh sách.");
            Console.WriteLine($"Tài khoản [{username}] đã xuất hiện trong danh sách.");

            var details = account.GetUserDetails(username);
            Assert.IsNotNull(details, "Không lấy được thông tin chi tiết của tài khoản.");
            Assert.AreEqual(username, details.Username, "Tên đăng nhập không khớp.");
            Assert.AreEqual(role, details.Role, "Quyền tài khoản không khớp.");
        }
        //Username trùng
        [TestMethod]

        public void AddAccount2()
        {
            int countBefore = account.CountAllUsers();
            string username = "NV11";
            string password = "123";
            string confirmPassword = "123";
            string maNV = "";
            string role = "Staff";
            
            account.AddUserAccount(username, password, confirmPassword, maNV, role);
            try
            {
                var successElement = wait.Until(driver =>
                {
                    try
                    {
                        var element = driver.FindElement(By.CssSelector("div.alert.alert-danger"));
                        return element.Displayed ? element : null;
                    }
                    catch (NoSuchElementException)
                    {
                        return null;
                    }
                });

                Assert.AreEqual("Tên đăng nhập đã tồn tại.", successElement.Text.Trim(), "Nội dung thông báo không đúng.");
                Console.WriteLine($"Thông báo hiển thị là: {successElement.Text.Trim()}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi với Username đã tồn tại");
            }
            int countAfter = account.CountAllUsers();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm tài khoản mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có tài khoản nào được thêm vào hệ thống -> Pass");

            var details = account.GetUserDetails(username);
            Assert.IsNotNull(details, "Không lấy được thông tin chi tiết của tài khoản.");
            Assert.AreEqual(username, details.Username, "Tên đăng nhập không khớp.");
            Assert.AreEqual(role, details.Role, "Quyền tài khoản không khớp.");
        }
        // Username trống
        [TestMethod]

        public void AddAccount3()
        {
            int countBefore = account.CountAllUsers();
            string username = "";
            account.AddUserAccount(username, "12", "12", "", "Staff");
            var UsernameInput = driver.FindElement(By.Id("username"));
            string validationMessage = UsernameInput.GetAttribute("validationMessage");

            // Kiểm tra thông báo lỗi có đúng như mong đợi
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Tên đăng nhập hiển thị thông báo lỗi: [{validationMessage}]");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addUserModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");


            int countAfter = account.CountAllUsers();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có phòng nào được thêm vào hệ thống -> Pass");
        }
        [TestMethod]

        public void AddAccount4()
        {
            int countBefore = account.CountAllUsers();
            string password = "";
            account.AddUserAccount("12", password, "12", "", "Staff");
            var UsernameInput = driver.FindElement(By.Id("password"));
            string validationMessage = UsernameInput.GetAttribute("validationMessage");

            // Kiểm tra thông báo lỗi có đúng như mong đợi
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Password hiển thị thông báo lỗi: [{validationMessage}]");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addUserModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");


            int countAfter = account.CountAllUsers();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có phòng nào được thêm vào hệ thống -> Pass");
        }
        [TestMethod]
        public void AddAccount5()
        {
            int countBefore = account.CountAllUsers();
            string confirmPassword = "";
            account.AddUserAccount("12", "12", confirmPassword, "", "Staff");
            var ConfirmPasseInput = driver.FindElement(By.Id("confirm_password"));
            string validationMessage = ConfirmPasseInput.GetAttribute("validationMessage");

            // Kiểm tra thông báo lỗi có đúng như mong đợi
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Confirm Password hiển thị thông báo lỗi: [{validationMessage}]");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addUserModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");


            int countAfter = account.CountAllUsers();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có phòng nào được thêm vào hệ thống -> Pass");
        }
        [TestMethod]
        public void AddAccount6()
        {
            int countBefore = account.CountAllUsers();
            string username = "NV122";
            string password = "123";
            string confirmPassword = "12367";
            string maNV = "";
            string role = "Staff";

            account.AddUserAccount(username, password, confirmPassword, maNV, role);
            try
            {
                var successElement = wait.Until(driver =>
                {
                    try
                    {
                        var element = driver.FindElement(By.CssSelector("div.alert.alert-danger"));
                        return element.Displayed ? element : null;
                    }
                    catch (NoSuchElementException)
                    {
                        return null;
                    }
                });

                Assert.AreEqual("Mật khẩu xác nhận không khớp.", successElement.Text.Trim(), "Nội dung thông báo không đúng.");
                Console.WriteLine($"Thông báo hiển thị là: {successElement.Text.Trim()}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi ");
            }
            int countAfter = account.CountAllUsers();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm tài khoản mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có tài khoản nào được thêm vào hệ thống -> Pass");

         
        }
        [TestMethod]
        public void AddAccount7()
        {
            int countBefore = account.CountAllUsers();
            string username = "NV122";
            string password = "123";
            string confirmPassword = "123";
            string maNV = "NV123";
            string role = "Staff";

            account.AddUserAccount(username, password, confirmPassword, maNV, role);
            try
            {
                var successElement = wait.Until(driver =>
                {
                    try
                    {
                        var element = driver.FindElement(By.CssSelector("div.alert.alert-danger"));
                        return element.Displayed ? element : null;
                    }
                    catch (NoSuchElementException)
                    {
                        return null;
                    }
                });

                Assert.AreEqual("Nhân viên này đã có tài khoản.", successElement.Text.Trim(), "Nội dung thông báo không đúng.");
                Console.WriteLine($"Thông báo hiển thị là: {successElement.Text.Trim()}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi ");
            }
            int countAfter = account.CountAllUsers();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm tài khoản mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có tài khoản nào được thêm vào hệ thống -> Pass");
        }
        [TestMethod]
        public void AddAccount8()
        {
            int countBefore = account.CountAllUsers();
     
            string expectedMessage = "Tên đăng nhập đã tồn tại";
           
            string username = "NV11";
            string password = "";
            string confirmPassword = "123";
            string maNV = "";
            string role = "Staff";
            UserModel userBefore = account.GetUserDetails(username);
            account.AddUserAccount(username, password, confirmPassword, maNV, role);
            var UsernameInput = driver.FindElement(By.Id("password"));
            string validationMessage = UsernameInput.GetAttribute("validationMessage");

            // Kiểm tra thông báo lỗi có đúng như mong đợi
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Password hiển thị thông báo lỗi: [{validationMessage}]");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addUserModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");
            // Đóng form
            driver.FindElement(By.CssSelector("button.btn.btn-secondary")).Click();
            Console.WriteLine("Đã nhấn nút Đóng form Thêm Tài khoản.");

            // 2. Kiểm tra xem có hiển thị lỗi tài khoản trùng không
            bool ErrorDisplayed = false;
            try
            {
                var errorElement = wait.Until(driver =>
                {
                    var elem = driver.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return (elem.Displayed && !string.IsNullOrWhiteSpace(elem.Text)) ? elem : null;
                });

                if (errorElement.Text.Contains(expectedMessage))
                {
                    ErrorDisplayed = true;
                    Console.WriteLine("Tìm thấy lỗi tài khoản trùng: " + expectedMessage);
                }
                else
                {
                    Console.WriteLine("Có hiển thị lỗi khác, nhưng không phải lỗi username trùng.");
                }
            }
            catch (WebDriverTimeoutException)
            {
                ErrorDisplayed = false;
                Console.WriteLine("Không tìm thấy lỗi tài khoản trùng.");
            }
            // 4. Đảm bảo phòng gốc vẫn tồn tại (không bị ghi đè, xóa)
            UserModel accountAfter = account.GetUserDetails(username);
            Assert.IsNotNull(accountAfter, $"Không tìm thấy tài khoản [{username}] sau khi test.");

            Assert.AreEqual(userBefore.Username, accountAfter.Username, "Tên đăng nhập bị ghi đè thay đổi!");
            Assert.AreEqual(userBefore.Role, accountAfter.Role, "vai trò tài khoản bị ghi đè thay đổi!");
            

            // 5. Đảm bảo không có bản ghi trùng
            int roomCount = account.CountUsername(username);
            Assert.AreEqual(1, roomCount, $"Tài khoản [{username}] xuất hiện {username} lần — hệ thống đã cho thêm trùng username!");
            Console.WriteLine("Dữ liệu tài khoản không bị thay đổi -> Không bị ghi đè -> Đúng.");
        }
        [TestMethod]
        public void AddAccount9()
        {
            int countBefore = account.CountAllUsers();

            string expectedMessage = "Mật khẩu xác nhận không khớp.";

            string username = "NV11";
            string password = "123";
            string confirmPassword = "12345";
            string maNV = "";
            string role = "Staff";

            // 1. Lưu thông tin phòng ban đầu
            UserModel accountBefore = account.GetUserDetails(username);
            Assert.IsNotNull(accountBefore, $"Không tìm thấy tài khoản [{username}] để kiểm thử.");

            account.AddUserAccount(username, password, confirmPassword, maNV, role);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            //Check số  lỗi giá hiển thị
            wait.Until(d => d.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Any(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)));

            var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

            // Kiểm tra có ít nhất 1 lỗi
            if (alerts.Count == 0)
            {
                Assert.Fail("Không có thông báo lỗi nào được hiển thị.");
            }

            // Kiểm tra chỉ có 1 lỗi
            Assert.AreEqual(1, alerts.Count, "Có nhiều hơn 1 thông báo lỗi hiển thị.");

            // Kiểm tra nội dung lỗi đúng
            string actualError = alerts[0].Text.Trim();
            Assert.AreEqual(expectedMessage, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedMessage}\" - Thực tế: \"{actualError}\"");

            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

         
            UserModel accountAfter = account.GetUserDetails(username);
            Assert.IsNotNull(accountAfter, $"Không tìm thấy tài khoản [{username}] sau khi test.");

            Assert.AreEqual(accountBefore.Username, accountAfter.Username, "Tên đăng nhập bị ghi đè thay đổi!");
            Assert.AreEqual(accountBefore.Role, accountAfter.Role, "vai trò tài khoản bị ghi đè thay đổi!");

            // 5. Đảm bảo không có bản ghi trùng
            int roomCount = account.CountUsername(username);
            Assert.AreEqual(1, roomCount, $"Tài khoản [{username}] xuất hiện {username} lần — hệ thống đã cho thêm trùng username!");
            Console.WriteLine("Dữ liệu tài khoản không bị thay đổi -> Không bị ghi đè -> Đúng.");
        }
        [TestMethod]
        public void AddAccount10()
        {
            int countBefore = account.CountAllUsers();

            string expectedMessage = "Tên đăng nhập đã tồn tại";

            string username = "NV11";
            string password = "123";
            string confirmPassword = "123456";
            string maNV = "NV003";
            string role = "Staff";

            // 1. Lưu thông tin phòng ban đầu
            UserModel accountBefore = account.GetUserDetails(username);
            Assert.IsNotNull(accountBefore, $"Không tìm thấy tài khoản [{username}] để kiểm thử.");

            account.AddUserAccount(username, password, confirmPassword, maNV, role);

            var ConfirmPasseInput = driver.FindElement(By.Id("confirm_password"));
            string validationMessage = ConfirmPasseInput.GetAttribute("validationMessage");

            // Kiểm tra thông báo lỗi có đúng như mong đợi
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Confirm Password hiển thị thông báo lỗi: [{validationMessage}]");
            // Đảm bảo form vẫn đang mở (không submit được)
           
            bool isModalStillOpen = driver.FindElement(By.Id("addUserModal"))
        .GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ.");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            // 5. Kiểm tra không có thêm user mới
            int countAfter = account.CountAllUsers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm tài khoản mới dù dữ liệu không hợp lệ.");

            // 6. Kiểm tra thông báo lỗi nếu có (có thể không có do trình duyệt tự xử lý HTML5 form)
            var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

            if (alerts.Count > 0)
            {
                Console.WriteLine("Thông báo lỗi giao diện hiển thị: " + alerts[0].Text.Trim());
                Assert.AreEqual(1, alerts.Count, "Có nhiều hơn 1 thông báo lỗi hiển thị.");
                Assert.AreEqual("Tên đăng nhập đã tồn tại", alerts[0].Text.Trim(), "Nội dung lỗi không đúng.");
            }
            else
            {
                Console.WriteLine("Không có alert lỗi hiển thị (do form bị chặn bởi trình duyệt).");
            }

            // 7. Đảm bảo thông tin người dùng không bị thay đổi
            var accountAfter = account.GetUserDetails(username);
            Assert.IsNotNull(accountAfter, $"Không tìm thấy tài khoản [{username}] sau khi test.");
            Assert.AreEqual(accountBefore.Username, accountAfter.Username, "Tên đăng nhập bị ghi đè.");
            Assert.AreEqual(accountBefore.Role, accountAfter.Role, "Vai trò tài khoản bị thay đổi.");

            // 8. Đảm bảo username không bị trùng bản ghi
            int duplicateCount = account.CountUsername(username);
            Assert.AreEqual(1, duplicateCount, $"Tài khoản [{username}] bị thêm trùng {duplicateCount} lần!");
            Console.WriteLine("Dữ liệu tài khoản không bị ghi đè hay nhân bản -> ĐÚNG.");
        }
        [TestMethod]
        public void AddAccount11()
        {
            int countBefore = account.CountAllUsers();

            string expectedMessage = "Mật khẩu xác nhận không khớp.";

            string username = "NV11";
            string password = "123";
            string confirmPassword = "123456";
            string maNV = "NV003";
            string role = "Staff";

            // 1. Lưu thông tin phòng ban đầu
            UserModel accountBefore = account.GetUserDetails(username);
            Assert.IsNotNull(accountBefore, $"Không tìm thấy tài khoản [{username}] để kiểm thử.");

            account.AddUserAccount(username, password, confirmPassword, maNV, role);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            //Check số  lỗi giá hiển thị
            wait.Until(d => d.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Any(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)));

            var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

            // Kiểm tra có ít nhất 1 lỗi
            if (alerts.Count == 0)
            {
                Assert.Fail("Không có thông báo lỗi nào được hiển thị.");
            }

            // Kiểm tra chỉ có 1 lỗi
            Assert.AreEqual(1, alerts.Count, "Có nhiều hơn 1 thông báo lỗi hiển thị.");

            // Kiểm tra nội dung lỗi đúng
            string actualError = alerts[0].Text.Trim();
            Assert.AreEqual(expectedMessage, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedMessage}\" - Thực tế: \"{actualError}\"");

            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);


            UserModel accountAfter = account.GetUserDetails(username);
            Assert.IsNotNull(accountAfter, $"Không tìm thấy tài khoản [{username}] sau khi test.");

            Assert.AreEqual(accountBefore.Username, accountAfter.Username, "Tên đăng nhập bị ghi đè thay đổi!");
            Assert.AreEqual(accountBefore.Role, accountAfter.Role, "vai trò tài khoản bị ghi đè thay đổi!");

            // 5. Đảm bảo không có bản ghi trùng
            int roomCount = account.CountUsername(username);
            Assert.AreEqual(1, roomCount, $"Tài khoản [{username}] xuất hiện {username} lần — hệ thống đã cho thêm trùng username!");
            Console.WriteLine("Dữ liệu tài khoản không bị thay đổi -> Không bị ghi đè -> Đúng.");
        }
        [TestMethod]
        public void AddAccount12()
        {
            int countBefore = account.CountAllUsers();
            string confirmPassword = "";
            account.AddUserAccount("12", "12", confirmPassword, "", "Staff");
            var ConfirmPasseInput = driver.FindElement(By.Id("confirm_password"));
            string validationMessage = ConfirmPasseInput.GetAttribute("validationMessage");

            // Kiểm tra thông báo lỗi có đúng như mong đợi
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Confirm Password hiển thị thông báo lỗi: [{validationMessage}]");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addUserModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");


            int countAfter = account.CountAllUsers();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có phòng nào được thêm vào hệ thống -> Pass");
        }
        [TestMethod]
        public void AddAccount13()
        {
            int countBefore = account.CountAllUsers();

            string expectedMessage = "Mật khẩu xác nhận không khớp.";

            string username = "NV1123";
            string password = "123";
            string confirmPassword = "12345";
            string maNV = "NV003";
            string role = "Staff";

            account.AddUserAccount(username, password, confirmPassword, maNV, role);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            //Check số  lỗi giá hiển thị
            wait.Until(d => d.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Any(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)));

            var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

            // Kiểm tra có ít nhất 1 lỗi
            if (alerts.Count == 0)
            {
                Assert.Fail("Không có thông báo lỗi nào được hiển thị.");
            }

            // Kiểm tra chỉ có 1 lỗi
            Assert.AreEqual(1, alerts.Count, "Có nhiều hơn 1 thông báo lỗi hiển thị.");

            // Kiểm tra nội dung lỗi đúng
            string actualError = alerts[0].Text.Trim();
            Assert.AreEqual(expectedMessage, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedMessage}\" - Thực tế: \"{actualError}\"");

            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            int countAfter = account.CountAllUsers();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm tài khoản mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có tài khoản nào được thêm vào hệ thống -> Pass");

          
        }
        [TestMethod]
        //-> Tổng số phòng vượt quá max giưới hạn
        public void TestRoom14()
        {
            account.OpenSidebarIfNeeded();
            var BtnUser = driver.FindElement(By.CssSelector("#sidebarMenu .nav-link[href='user.php']"));
            BtnUser.Click();

            // Chờ nút hiển thị
            wait.Until(d => d.FindElement(By.CssSelector("button.btn.btn-primary")));
            var addUser = driver.FindElement(By.CssSelector("button.btn.btn-primary"));

            // Kiểm tra nút bị vô hiệu hóa
            bool isDisabled = addUser.GetAttribute("disabled") != null;
            Assert.IsTrue(isDisabled, "Nút Thêm Tài khoản đáng lẽ phải bị disable khi vượt quá giới hạn.");
            Console.WriteLine("Nút Thêm tài khoản ở trạng thái Disable đúng mong đợi");
            
        }
    }
}
