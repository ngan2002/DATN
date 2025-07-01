using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using static DATN.Account.PageRepair;

namespace DATN.Account
{
    [TestClass]
    public class TestAccountRepair
    {
        private IWebDriver driver;
        private PageRepair repair;
        private WebDriverWait wait;

        [TestInitialize]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("http://localhost/qlks-ngan_hn/source/login.php");

            driver.FindElement(By.Id("username")).SendKeys("admin");
            driver.FindElement(By.Id("password")).SendKeys("admin123");
            driver.FindElement(By.CssSelector("button.w-100.btn.btn-lg.btn-primary")).Click();

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            repair = new PageRepair(driver);
        }

        [TestMethod]
        public void EditAccount1()
        {
            // B1: Khai báo thông tin
            string oldUsername = "NV122";
            string newPassword = "123456"; // Chỉ sửa mật khẩu

            // B2: Lấy thông tin gốc để đối chiếu sau sửa
            var accountBefore = repair.FindUserByUsername(oldUsername);
            Assert.IsNotNull(accountBefore, $"Không tìm thấy tài khoản [{oldUsername}] trước khi sửa.");

            // B3: Mở form sửa
            try
            {
                repair.OpenEditUserForm(oldUsername);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Không mở được form sửa tài khoản [{oldUsername}]: {ex.Message}");
            }

            // B4: Gọi hàm cập nhật, chỉ truyền trường cần sửa (password)
            repair.UpdateUser1("", newPassword, "", "");

            // B5: Kiểm tra thông báo cập nhật thành công
            var alert = wait.Until(d => d.FindElement(By.CssSelector("div.alert.alert-success")));
            Assert.IsTrue(alert.Text.Contains("Cập nhật tài khoản thành công"), "Không thấy thông báo cập nhật thành công.");

            // B6: Kiểm tra lại sự tồn tại của tài khoản cũ
            bool isExistAfter = repair.IsUserExists(oldUsername);
            Assert.IsTrue(isExistAfter, $"Không tìm thấy tài khoản [{oldUsername}] sau khi sửa.");
            Console.WriteLine($"Tài khoản [{oldUsername}] vẫn còn sau khi sửa.");

            // B7: Lấy lại thông tin sau khi sửa và đối chiếu
            var accountAfter = repair.FindUserByUsername(oldUsername);
            Assert.IsNotNull(accountAfter, $"Không tìm thấy thông tin tài khoản [{oldUsername}] sau khi sửa.");

            Assert.AreEqual(accountBefore.Username, accountAfter.Username, "Tên đăng nhập bị thay đổi.");
            Assert.AreEqual(accountBefore.Role.ToLower(), accountAfter.Role.ToLower(), "Vai trò bị thay đổi.");
            Assert.AreEqual(accountBefore.MaNV, accountAfter.MaNV, "Mã nhân viên bị thay đổi.");

            // Ghi chú: Nếu ngày tạo không hiện trong giao diện sửa thì không cần check CreatedDate

            // B8: Đăng xuất
            driver.FindElement(By.CssSelector("a.nav-link[href='logout.php']")).Click();

            // B9: Đăng nhập lại bằng tài khoản đã sửa
            driver.FindElement(By.Id("username")).SendKeys(oldUsername);
            driver.FindElement(By.Id("password")).SendKeys(newPassword);
            driver.FindElement(By.CssSelector("button.w-100.btn.btn-lg.btn-primary")).Click();

            // B10: Kiểm tra đăng nhập thành công
            wait.Until(driver => driver.Url.Contains("index.php"));
            Assert.IsTrue(
                driver.Url.Contains("http://localhost/qlks-ngan_hn/source/index.php"),
                "Đăng nhập thất bại: Không chuyển hướng đến trang chính"
            );

            Console.WriteLine("Đăng nhập thành công với mật khẩu mới.");
            Console.WriteLine("TestUserRepair1: PASSED - Chỉ sửa mật khẩu, các trường khác giữ nguyên.");
        }

        [TestMethod]
        public void EditAccount2()
        {
            int countBefore = repair.CountAllUsers();

            string oldUsername = "ngan"; // tài khoản cần sửa
            string newUsername = ""; // username đã tồn tại -> gây lỗi
            string newPassword = "123";
            string newMaNV = ""; // giữ nguyên
            string newRole = "Staff";

            repair.OpenEditUserForm(oldUsername);
            repair.UpdateUser(newUsername, newPassword, newMaNV, newRole);

            try
            {
                var errorElement = wait.Until(driver =>
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

                Assert.AreEqual("Tên đăng nhập đã tồn tại.", errorElement.Text.Trim(), "Nội dung thông báo không đúng.");
                Console.WriteLine($"Thông báo hiển thị là: {errorElement.Text.Trim()}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi với Username đã tồn tại khi sửa.");
            }

            int countAfter = repair.CountAllUsers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống đã tạo thêm tài khoản khi sửa với dữ liệu không hợp lệ.");
            Console.WriteLine("Không có tài khoản mới được tạo khi sửa tài khoản -> Pass");

            var details = repair.GetUserDetails(oldUsername);
            Assert.IsNotNull(details, "Không lấy được thông tin chi tiết tài khoản.");
            Assert.AreEqual(oldUsername, details.Username, "Tên đăng nhập đã bị thay đổi không đúng.");
            Assert.AreEqual(newRole, details.Role, "Quyền tài khoản không khớp.");
        }

        // trống username
        [TestMethod]
        public void EditAccount3()
        {
            int countBefore = repair.CountAllUsers();

            string oldUsername = "NV10";
            string newUsername = ""; 
            repair.OpenEditUserForm(oldUsername);
            repair.UpdateUser(newUsername, "12", "", "Staff"); 

          
            var usernameInput = driver.FindElement(By.Id("edit_username"));
            string validationMessage = usernameInput.GetAttribute("validationMessage");

            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Tên đăng nhập hiển thị thông báo lỗi: [{validationMessage}]");

            // Kiểm tra form sửa còn hiển thị không
            bool isModalStillOpen = driver.FindElement(By.Id("editUserModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form sửa tài khoản đã bị đóng dù dữ liệu không hợp lệ.");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            driver.FindElement(By.CssSelector("div#editUserModal button.btn.btn-secondary")).Click();
       

            int countAfter = repair.CountAllUsers();
            Assert.IsTrue(countAfter == countBefore || countAfter == -1, "Dữ liệu bị thay đổi hoặc modal chưa đóng.");
            Console.WriteLine("Không có tài khoản nào bị thay đổi khi sửa -> Pass");
        }

        [TestMethod]
        public void EditAccount4()
        {
            int countBefore = repair.CountAllUsers();

            string oldUsername = "NV01";
            string newUsername = "test"; 
            repair.OpenEditUserForm(oldUsername);
            repair.UpdateUser(newUsername, "", "NV123", "Staff");

            try
            {
                var errorElement = wait.Until(driver =>
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

                Assert.AreEqual("Nhân viên này đã có tài khoản khác.", errorElement.Text.Trim());
                Console.WriteLine($"Thông báo hiển thị: {errorElement.Text.Trim()}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi khi mã nhân viên đã có tài khoản.");
            }

            int countAfter = repair.CountAllUsers();
            Assert.AreEqual(countBefore, countAfter);
            Console.WriteLine("Không có thay đổi trong hệ thống khi sửa với mã nhân viên trùng -> Pass");
        }
        [TestMethod]
        public void EditAccount5()
        {
            int countBefore = repair.CountAllUsers();

            string oldUsername = "NV005"; 
            string newUsername = "NV005"; 
            string newPassword = "123";
            string newMaNV = "NV005"; // giữ nguyên
            string newRole = "Staff";
            string expectedError = "Nhân viên này đã có tài khoản khác.";
            repair.OpenEditUserForm(oldUsername);
            repair.UpdateUser(newUsername, newPassword, newMaNV, newRole);

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
            Assert.AreEqual(expectedError, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedError}\" - Thực tế: \"{actualError}\"");

            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            int countAfter = repair.CountAllUsers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống đã tạo thêm tài khoản khi sửa với dữ liệu không hợp lệ.");
            Console.WriteLine("Không có tài khoản mới được tạo khi sửa tài khoản -> Pass");

            var details = repair.GetUserDetails(oldUsername);
            Assert.IsNotNull(details, "Không lấy được thông tin chi tiết tài khoản.");
            Assert.AreEqual(oldUsername, details.Username, "Tên đăng nhập đã bị thay đổi không đúng.");
            Assert.AreEqual(newRole, details.Role, "Quyền tài khoản không khớp.");
        }
        [TestMethod]
        public void EditAccount6_Checkdata()
        {
            string username = "NV11";

            // B1: Lấy thông tin từ bảng trước khi click sửa
            var accountPage = new PageRepair(driver);
            var userInfo = accountPage.GetUserDetails(username);
            Assert.IsNotNull(userInfo, $"Không tìm thấy tài khoản [{username}] trong danh sách.");

            // Lưu thông tin mong đợi
            string expectedUsername = userInfo.Username;
            string expectedRole = userInfo.Role;
            string expectedMaNV = userInfo.MaNV;

            // B2: Mở form sửa
            accountPage.OpenEditUserForm(username);

            // B3: Lấy dữ liệu thực tế trong form sửa
            var actualUsername = driver.FindElement(By.Id("edit_username")).GetAttribute("value").Trim();
            var selectMaNV = new SelectElement(driver.FindElement(By.Id("edit_maNV")));
            var actualMaNVText = selectMaNV.SelectedOption.Text.Trim();
            var actualMaNVValue = selectMaNV.SelectedOption.GetAttribute("value").Trim();
            var actualRole = new SelectElement(driver.FindElement(By.Id("edit_role"))).SelectedOption.Text.Trim();

            // B4: So sánh
            Assert.AreEqual(expectedUsername, actualUsername, "Tên đăng nhập trong form sửa không đúng.");

            if (expectedMaNV == "Không có")
            {
                Assert.AreEqual("", actualMaNVValue, "Form sửa không có mã nhân viên liên kết, nhưng value không rỗng.");
            }
            else
            {
                Assert.AreEqual(expectedMaNV, actualMaNVValue, "Mã nhân viên trong form sửa không đúng.");
            }


            Assert.AreEqual(expectedRole, actualRole, "Vai trò trong form sửa không đúng.");


        }
    }
}
