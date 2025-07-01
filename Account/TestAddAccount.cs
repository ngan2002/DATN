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
        // hợp lệ
        public void AddAccount1()
        {
            string username = "NV121";
            string password = "123";
            string confirmPassword = "123";
            string role = "Staff";

            account.AddUserAccount(username, password, confirmPassword, role);

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
            Console.WriteLine($"Thông báo hiển thị thành công là: {successElement.Text.Trim()}");

            Assert.IsTrue(account.IsUserExists(username), $"Không tìm thấy tài khoản [{username}] trong danh sách.");
            Console.WriteLine($"Tài khoản [{username}] đã xuất hiện trong danh sách.");

            var details = account.GetUserDetails(username);
            Assert.IsNotNull(details, "Không lấy được thông tin chi tiết của tài khoản.");
            Assert.AreEqual(username, details.Username, "Tên đăng nhập không khớp.");
            Assert.AreEqual(role, details.Role, "Quyền tài khoản không khớp.");

            // Kiểm tra ngày tạo
            DateTime now = DateTime.Now;
            DateTime expectedDate = now.Date; // chỉ lấy phần ngày (bỏ giờ)
            DateTime actualDate = details.CreatedDate.Date;

            Assert.AreEqual(expectedDate, actualDate, $"Ngày tạo không đúng. Dự kiến: {expectedDate:dd/MM/yyyy}, thực tế: {actualDate:dd/MM/yyyy}");
            Console.WriteLine($"Tài khoản tạo lúc: {details.CreatedDate:dd/MM/yyyy HH:mm}");
        }

        //Username trùng
        [TestMethod]

        public void AddAccount2()
        {
          
            string username = "admin";
            string password = "123";
            string confirmPassword = "123";
            string role = "Staff";

            var before = account.GetUserDetails(username);
            Assert.IsNotNull(before, $"Tài khoản [{username}] không tồn tại trước khi test, không thể kiểm thử trùng.");

            int countBefore = account.CountAllUsers();
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

            var after = account.GetUserDetails(username);
            Assert.IsNotNull(after, "Không lấy được thông tin chi tiết của tài khoản.");
            Assert.AreEqual(before.Username, after.Username, "Username bị thay đổi.");
            Assert.AreEqual(before.Role, after.Role, "Role bị ghi đè.");
            Assert.AreEqual(before.CreatedDate, after.CreatedDate, "Ngày tạo tài khoản bị thay đổi.");
        }
        // Username trống
        [TestMethod]

        public void AddAccount3()
        {
            string username = "";
            account.AddUserAccount(username, "12", "12", "Staff");
            int countBefore = account.CountAllUsers(); 
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
   //Pass để trống
        [TestMethod]

        public void AddAccount4()
        {
            // 1. Lấy số lượng tài khoản trước khi thao tác
            int countBefore = account.CountAllUsers();

            // 2. Khai báo thông tin (cố tình để trống mật khẩu để bị chặn bởi HTML5)
            string username = "test_html5";
            string password = "";
            string confirmPassword = "";
            string role = "Staff";

            // 3. Gọi hàm nhập liệu (hàm này không nên submit nếu form bị lỗi HTML5)
            account.AddUserAccount(username, password, confirmPassword, role);

            // 4. Lấy ra textbox mật khẩu để kiểm tra thông báo lỗi trình duyệt
            var passwordInput = driver.FindElement(By.Id("password"));
            string validationMessage = passwordInput.GetAttribute("validationMessage");

            // 5. Kiểm tra nội dung cảnh báo đúng như mong đợi (trình duyệt tiếng Anh)
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Password hiển thị thông báo lỗi: [{validationMessage}]");

            // 6. Kiểm tra modal vẫn hiển thị (form chưa bị submit)
            bool isModalStillOpen = driver.FindElement(By.Id("addUserModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            // 7. Đếm lại số lượng tài khoản sau thao tác
            int countAfter = account.CountAllUsers();

            // 8. Kiểm tra không có tài khoản nào được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm tài khoản mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có tài khoản nào được thêm vào hệ thống -> PASS");
        }

        //Confirm Pass trống
        [TestMethod]
        public void AddAccount5()
        {
            int countBefore = account.CountAllUsers();
            string confirmPassword = "";
            account.AddUserAccount("12", "12", confirmPassword, "Staff");
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
        // CP Không khớp với pass
        [TestMethod]
        public void AddAccount6()
        {
            int countBefore = account.CountAllUsers();
            string username = "NV122";
            string password = "123";
            string confirmPassword = "12367";
            string role = "Staff";

            account.AddUserAccount(username, password, confirmPassword, role);
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
        // user trùng, pass trống
        [TestMethod]
        public void AddAccount7()
        {
            // 1. Lấy số lượng tài khoản trước khi test
            int countBefore = account.CountAllUsers();

            // 2. Thông tin đầu vào - cố tình thiếu mã nhân viên (nếu trường này đang bắt buộc ở client)
            string username = "test1";
            string password = "";
            string confirmPassword = "123";
            string role = "Staff";

            // 3. Gọi hàm nhập liệu (chưa bypass HTML5)
            account.AddUserAccount(username, password, confirmPassword, role);

            // 4. Kiểm tra lỗi HTML5 tại trường full_name
            var passInput = driver.FindElement(By.Id("password"));
            string validationMessage = passInput.GetAttribute("validationMessage");
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Thông báo lỗi HTML5 tại 'full_name': {validationMessage}");

            // 4. Kiểm tra: không có alert hiển thị vì form bị HTML5 chặn submit
            var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

            Assert.AreEqual(0, alerts.Count, "Không nên có alert vì trình duyệt đã chặn submit.");
            Console.WriteLine("Không có alert lỗi hiển thị (đúng do bị HTML5 chặn).");

            // 5. Kiểm tra modal vẫn đang hiển thị
            bool isModalOpen = driver.FindElement(By.Id("addUserModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn mở sau khi bị HTML5 chặn.");

            // 6. Kiểm tra hệ thống không thêm tài khoản mới
            int countAfter = account.CountAllUsers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm tài khoản dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có tài khoản nào được thêm vào hệ thống -> PASS");

            // 7. Kiểm tra dữ liệu khách hàng cũ không bị ghi đè
            string cccd = "123456789"; // CCCD giả định có sẵn
            var accountBefore = account.GetUserDetails(username);
            Assert.IsNotNull(accountBefore, $"Không tìm thấy khách hàng [{username}] trước khi test.");

            // Lấy lại thông tin tài khoản sau khi test
            var accountAfter = account.GetUserDetails(username);
            Assert.IsNotNull(accountAfter, $"Không tìm thấy tài khoản [{username}] sau khi test.");

            // So sánh các thuộc tính
            Assert.AreEqual(accountBefore.Role, accountAfter.Role, "Vai trò tài khoản bị thay đổi.");
            Console.WriteLine("Dữ liệu tài khoản không bị thay đổi -> PASS");
        }


        // user trùng, CP không khớp
        [TestMethod]
        public void AddAccount8()
        {
            string username = "NV11";
            string password = "12";
            string confirmPassword = "123"; // khác password -> lỗi
            string role = "Staff";
            string expectedMessage = "Mật khẩu xác nhận không khớp.";

            // 1. Lưu thông tin tài khoản ban đầu
            UserModel accountBefore = account.GetUserDetails(username);
            Assert.IsNotNull(accountBefore, $"Không tìm thấy tài khoản [{username}] để kiểm thử.");

            // 2. Gửi dữ liệu không hợp lệ
            account.AddUserAccount(username, password, confirmPassword, role);

            // 3. Kiểm tra alert hiển thị
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Any(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)));

            var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

            Assert.IsTrue(alerts.Count > 0, "Không có thông báo lỗi nào được hiển thị.");
            Assert.AreEqual(1, alerts.Count, "Có nhiều hơn 1 thông báo lỗi hiển thị.");

            string actualError = alerts[0].Text.Trim();
            Assert.AreEqual(expectedMessage, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedMessage}\" - Thực tế: \"{actualError}\"");

            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            // 4. Kiểm tra tài khoản không bị thay đổi
            UserModel accountAfter = account.GetUserDetails(username);
            Assert.IsNotNull(accountAfter, $"Không tìm thấy tài khoản [{username}] sau khi test.");

            Assert.AreEqual(accountBefore.Username, accountAfter.Username, "Tên đăng nhập bị thay đổi.");
            Assert.AreEqual(accountBefore.Role, accountAfter.Role, "Vai trò tài khoản bị thay đổi.");

            // 5. Đảm bảo không bị thêm trùng
            int userCount = account.CountUsername(username);
            Assert.AreEqual(1, userCount, $"Tài khoản [{username}] xuất hiện {userCount} lần — hệ thống đã thêm trùng!");

            Console.WriteLine("Dữ liệu tài khoản không bị thay đổi -> PASS.");
        }

        [TestMethod]
        public void AddAccount9()
        {
            // 1. Ghi nhận số lượng tài khoản ban đầu
            int totalBefore = account.CountAllUsers();

            string username = "NV11"; // tài khoản đã tồn tại
            string password = "123";
            string confirmPassword = ""; // để trống -> bị HTML5 chặn
            string role = "Staff";

            // 2. Lưu thông tin tài khoản trước khi test
            UserModel accountBefore = account.GetUserDetails(username);
            Assert.IsNotNull(accountBefore, $"Không tìm thấy tài khoản [{username}] để kiểm thử.");

            // 3. Kiểm tra trình duyệt chặn submit vì confirm password trống
            var confirmPassInput = driver.FindElement(By.Id("confirm_password")); // đúng ID
            string validationMessage = confirmPassInput.GetAttribute("validationMessage");
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Thông báo lỗi HTML5 tại 'confirm_password': {validationMessage}");

            // 4. Kiểm tra không có alert phía server vì form không submit
            var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

            Assert.AreEqual(0, alerts.Count, "Không nên có alert vì trình duyệt đã chặn submit.");
            Console.WriteLine("Không có alert lỗi hiển thị (đúng do bị HTML5 chặn).");

            // 5. Form modal vẫn hiển thị
            bool isModalOpen = driver.FindElement(By.Id("addUserModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalOpen, "Form đã bị đóng dù dữ liệu không hợp lệ.");
            Console.WriteLine("Form vẫn mở sau khi bị HTML5 chặn.");

            // 6. Kiểm tra hệ thống không thêm tài khoản mới
            int totalAfter = account.CountAllUsers();
            Assert.AreEqual(totalBefore, totalAfter, "Hệ thống vẫn thêm tài khoản dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có tài khoản nào được thêm vào hệ thống -> PASS");

            // 7. Kiểm tra dữ liệu tài khoản cũ không bị ghi đè
            UserModel accountAfter = account.GetUserDetails(username);
            Assert.IsNotNull(accountAfter, $"Không tìm thấy tài khoản [{username}] sau khi test.");
            Assert.AreEqual(accountBefore.Role, accountAfter.Role, "Vai trò tài khoản bị thay đổi.");
            Assert.AreEqual(accountBefore.Username, accountAfter.Username, "Tên đăng nhập bị thay đổi.");

            Console.WriteLine("Dữ liệu tài khoản không bị thay đổi -> PASS");
        }
        

       
        /// Đóng Form 
        ///--> Khi chưa nhập text
        [TestMethod]
        public void TestAccount10()
        {
            account.CloseForm1();
            bool isModalClosed = wait.Until(driver =>
            {
                try
                {
                    return !driver.FindElement(By.CssSelector("div.modal-content")).Displayed;

                }
                catch (NoSuchElementException)
                {
                    // Nếu phần tử không tồn tại nghĩa là modal đã đóng
                    return true;
                }
            });


            if (isModalClosed)
            {
                Console.WriteLine(" Form Thêm Tài Khoản đã được đóng thành công.");
            }
            else
            {
                Console.WriteLine("Form Thêm Tài Khoản vẫn đang hiển thị.");
            }

            Assert.IsTrue(isModalClosed, "Form Tài Khoản chưa được đóng sau khi nhấn nút Đóng.");
            Console.WriteLine("TestRoom14: Passed - Form Thêm Tài khoản chưa được đóng sau khi nhấn nút Đóng.");
        }
         ///--> Khi nhập text nhưng chưa lưu
        [TestMethod]
        public void TestAccount11()
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Nhập dữ liệu
            string username = "NV1123";
            string password = "123";
            string confirmPassword = "12345";
            string role = "Staff";

            // Gọi hàm đóng form sau khi điền
            account.CloseForm2(username, password, confirmPassword, role);

            // Kiểm tra form đã đóng chưa (giả định modal không còn hiển thị)
            bool isModalClosed = wait.Until(driver =>
            {
                try
                {
                    return !driver.FindElement(By.CssSelector("div.modal-content")).Displayed;
                }
                catch (NoSuchElementException)
                {
                    // Nếu phần tử không tồn tại nghĩa là modal đã đóng
                    return true;
                }
            });


            if (isModalClosed)
            {
                Console.WriteLine(" Form Thêm Tài Khoản đã được đóng thành công.");
            }
            else
            {
                Console.WriteLine("Form Thêm Tài Khoản vẫn đang hiển thị.");
            }

            Assert.IsTrue(isModalClosed, "Form Thêm Tài Khoản chưa được đóng sau khi nhấn nút Đóng.");
            Console.WriteLine("TestRoom15: Passed - Form Thêm Tài Khoản đã đóng sau khi nhấn nút Đóng.");
        }

        //[TestCleanup]
        //    public void TearDown()
        //    {
        //        driver.Quit(); // Luôn chạy sau mỗi test
        //    }
    }
}
