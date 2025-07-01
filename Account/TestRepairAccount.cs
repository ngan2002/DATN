using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATN.Account
{
    [TestClass]
    public class TestRepairAccount
    {
        private IWebDriver driver;
        private RepairPage repair;
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
            repair = new RepairPage(driver);

        }
        [TestClass]
        public class EditAccount        {
            private IWebDriver driver;
            private WebDriverWait wait;
            private RepairPage account;

            [TestInitialize]
            public void Setup()
            {
                driver = new ChromeDriver();
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                account = new RepairPage(driver);
                driver.Navigate().GoToUrl("http://localhost/your_app/users.php");
            }

            [TestCleanup]
            public void EditAccount()
            {
                driver.Quit();
            }

            // Test case 1: Sửa hợp lệ
            [TestMethod]
            public void EditAccount_Valid()
            {
                string username = "NV11";
                string password = "123";
                string maNV = "";
                string role = "Staff";

                account.EditUserAccount(username, password, maNV, role);

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

                Assert.AreEqual("Cập nhật tài khoản thành công!", successElement.Text.Trim());
                Console.WriteLine($"Thông báo hiển thị thành công là {successElement.Text.Trim()}");

                var details = account.GetUserDetails(username);
                Assert.IsNotNull(details);
                Assert.AreEqual(username, details.Username);
                Assert.AreEqual(role, details.Role);
            }

            // Test case 2: Username trống
            [TestMethod]
            public void EditAccount_Username_Empty()
            {
                account.EditUserAccount("", "123", "", "Staff");

                var input = driver.FindElement(By.Id("edit_username"));
                string validationMessage = input.GetAttribute("validationMessage");

                Assert.AreEqual("Please fill out this field.", validationMessage);
                Assert.IsTrue(driver.FindElement(By.Id("editUserModal")).GetAttribute("class").Contains("show"));
            }

            // Test case 3: Password trống
            [TestMethod]
            public void EditAccount_Password_Empty()
            {
                account.EditUserAccount("NV11", "", "", "Staff");

                var input = driver.FindElement(By.Id("edit_password"));
                string validationMessage = input.GetAttribute("validationMessage");

                Assert.AreEqual("Please fill out this field.", validationMessage);
                Assert.IsTrue(driver.FindElement(By.Id("editUserModal")).GetAttribute("class").Contains("show"));
            }

            // Test case 4: ConfirmPassword trống (nếu có)
            [TestMethod]
            public void EditAccount_ConfirmPassword_Empty()
            {
                var confirmInput = driver.FindElement(By.Id("confirm_password"));
                confirmInput.Clear();

                string validationMessage = confirmInput.GetAttribute("validationMessage");
                Assert.AreEqual("Please fill out this field.", validationMessage);
                Assert.IsTrue(driver.FindElement(By.Id("editUserModal")).GetAttribute("class").Contains("show"));
            }

            // Test case 5: Username bị trùng
            [TestMethod]
            public void EditAccount_Username_Duplicate()
            {
                string username = "NV11"; // Giả sử username này đã tồn tại
                int countBefore = account.CountAllUsers();

                account.EditUserAccount(username, "123", "", "Staff");

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

                Assert.AreEqual("Tên đăng nhập đã tồn tại.", errorElement.Text.Trim());
                int countAfter = account.CountAllUsers();
                Assert.AreEqual(countBefore, countAfter);
            }
            // Test case 6: Confirm password không khớp
            [TestMethod]
            public void EditAccount_ConfirmPassword_NotMatch()
            {
                account.EditUserAccount("NV122", "123", "", "Staff");

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

                Assert.AreEqual("Mật khẩu xác nhận không khớp.", errorElement.Text.Trim());
                Console.WriteLine("Hiển thị lỗi xác nhận mật khẩu không khớp -> PASS");
            }

            // Test case 7: Nhân viên đã có tài khoản
            [TestMethod]
            public void EditAccount7()
            {
                account.EditUserAccount("NV122", "123", "NV123", "Staff");

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

                Assert.AreEqual("Nhân viên này đã có tài khoản.", errorElement.Text.Trim());
                Console.WriteLine("Hiển thị lỗi nhân viên đã có tài khoản -> PASS");
            }
            [TestMethod]
            public void EditAccount_PasswordEmpty_UsernameDuplicate()
            {
                string username = "NV11";
                string password = "";
                string confirmPassword = "123";
                string maNV = "";
                string role = "Staff";

                RepairPage.UserModel accountBefore = account.GetUserDetails(username);
                Assert.IsNotNull(accountBefore);

                account.EditUserAccount(username, password, maNV, role);

                var passwordInput = driver.FindElement(By.Id("edit_password"));
                string validationMessage = passwordInput.GetAttribute("validationMessage");

                Assert.AreEqual("Please fill out this field.", validationMessage);
                Assert.IsTrue(driver.FindElement(By.Id("editUserModal")).GetAttribute("class").Contains("show"));

                // Đóng form
                driver.FindElement(By.CssSelector("button.btn.btn-secondary")).Click();

                int count = account.CountUsername(username);
                Assert.AreEqual(1, count, $"Username [{username}] bị trùng");
            }
            [TestMethod]
            public void EditAccount9()
            {
                string username = "NV11";
                string password = "123";
                string confirmPassword = "12345";
                string maNV = "";
                string role = "Staff";

                RepairPage.UserModel accountBefore = account.GetUserDetails(username);
                Assert.IsNotNull(accountBefore);

                account.EditUserAccount(username, password, maNV, role);

                var error = wait.Until(d =>
                {
                    var alert = d.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return alert.Displayed ? alert : null;
                });

                Assert.AreEqual("Mật khẩu xác nhận không khớp.", error.Text.Trim());

                var after = account.GetUserDetails(username);
                Assert.AreEqual(accountBefore.Username, after.Username);
                Assert.AreEqual(accountBefore.Role, after.Role);

                int count = account.CountUsername(username);
                Assert.AreEqual(1, count);
            }
            [TestMethod]
            public void EditAccount10()
            {
                string username = "NV11";
                string password = "123";
                string confirmPassword = "";
                string maNV = "NV003";
                string role = "Staff";

                RepairPage.UserModel accountBefore = account.GetUserDetails(username);
                Assert.IsNotNull(accountBefore);

                account.EditUserAccount(username, password, maNV, role);

                var confirmInput = driver.FindElement(By.Id("confirm_password"));
                string validationMessage = confirmInput.GetAttribute("validationMessage");

                Assert.AreEqual("Please fill out this field.", validationMessage);
                Assert.IsTrue(driver.FindElement(By.Id("editUserModal")).GetAttribute("class").Contains("show"));

                int count = account.CountUsername(username);
                Assert.AreEqual(1, count);
            }
            [TestMethod]
            public void EditAccount11()
            {
                string username = "NV11";
                string password = "123";
                string confirmPassword = "123456";
                string maNV = "NV003";
                string role = "Staff";

                RepairPage.UserModel accountBefore = account.GetUserDetails(username);
                Assert.IsNotNull(accountBefore);

                account.EditUserAccount(username, password, maNV, role);

                var alert = wait.Until(d =>
                {
                    var elem = d.FindElement(By.CssSelector("div.alert.alert-danger"));
                    return elem.Displayed ? elem : null;
                });

                Assert.AreEqual("Mật khẩu xác nhận không khớp.", alert.Text.Trim());

                var after = account.GetUserDetails(username);
                Assert.AreEqual(accountBefore.Username, after.Username);
                Assert.AreEqual(accountBefore.Role, after.Role);

                int count = account.CountUsername(username);
                Assert.AreEqual(1, count);
            }
            [TestMethod]
            public void EditAccount12()
            {
                string username = "NV11";
                string password = "123";
                string confirmPassword = "";
                string maNV = "";
                string role = "Staff";

                account.EditUserAccount(username, password, maNV, role);

                var confirmInput = driver.FindElement(By.Id("confirm_password"));
                string validationMessage = confirmInput.GetAttribute("validationMessage");

                Assert.AreEqual("Please fill out this field.", validationMessage);
                Assert.IsTrue(driver.FindElement(By.Id("editUserModal")).GetAttribute("class").Contains("show"));
            }
            [TestMethod]
            public void EditAccount13()
            {
                string username = "NV1123";
                string password = "123";
                string confirmPassword = "12345";
                string maNV = "NV003";
                string role = "Staff";

                account.EditUserAccount(username, password, maNV, role);

                var alert = wait.Until(d =>
                {
                    var elems = d.FindElements(By.CssSelector("div.alert.alert-danger"))
                        .Where(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text)).ToList();
                    return elems.FirstOrDefault();
                });

                Assert.IsNotNull(alert, "Không có thông báo lỗi hiển thị.");
                Assert.AreEqual("Mật khẩu xác nhận không khớp.", alert.Text.Trim());

                int countAfter = account.CountUsername(username);
                Assert.AreEqual(0, countAfter, "Hệ thống vẫn thay đổi thông tin với dữ liệu sai.");
            }



        }


    }
}
