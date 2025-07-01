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
using static DATN.Rooms.RoomPage;
using static DATN.Account.AccountPage;

namespace DATN.Customer
{
    [TestClass]
    public class TestAddCustomer
    {
        private IWebDriver driver;
        private CustomerPageAdd customer;
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


            customer = new CustomerPageAdd(driver);

        }
        [TestMethod]
   
        public void TestAddCustomer1()
        {
            // Thiết lập dữ liệu đầu vào
            string cccd = "02021050764";               
            string fullName = "Nguyễn Văn B";            
            string phone = "0912345678";                 
            string email = "nguyenvana2@gmail.com";       
            string address = "123 Nguyễn Trãi, Hà Nội";  

            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();

            // Thực hiện thêm khách hàng
            customer.AddCustomer(cccd, fullName, phone, email, address);

            // Chờ thông báo thành công
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

            Assert.IsNotNull(successElement, "Không hiển thị thông báo thành công.");
            Assert.AreEqual("Thêm khách hàng thành công!", successElement.Text.Trim(), "Nội dung thông báo thành công không đúng.");
            Console.WriteLine($"Thông báo: {successElement.Text.Trim()}");

            // Kiểm tra khách hàng đã xuất hiện trong danh sách
            Assert.IsTrue(customer.IsCustomerExists(cccd), $"Không tìm thấy khách hàng có CCCD [{cccd}] trong danh sách.");
            Console.WriteLine($"Khách hàng [{cccd}] đã được thêm thành công.");

            // Kiểm tra thông tin chi tiết khách hàng
            var details = customer.GetCustomerDetails(cccd);
            Assert.IsNotNull(details, "Không lấy được thông tin chi tiết khách hàng.");

            Assert.AreEqual(cccd, details.CCCD, "CCCD không khớp.");
            Assert.AreEqual(fullName, details.FullName, "Họ tên không khớp.");
            Assert.AreEqual(phone, details.Phone, "Số điện thoại không khớp.");
            Assert.AreEqual(email, details.Email, "Email không khớp.");
            Assert.AreEqual(address, details.Address, "Địa chỉ không khớp.");

            customer.OpenBookingPage();
            var select = wait.Until(d => d.FindElement(By.Id("cccd")));
            var exists = new SelectElement(select).Options.Any(o => o.Text.Contains(cccd));
            Assert.IsTrue(exists, $" Khách hàng [{cccd}] không có trong combobox đặt phòng.");
            Console.WriteLine($"Khách hàng [{cccd}] đã có trong combobox đặt phòng.");

            Console.WriteLine("TestAddCustomer1: PASSED - Thêm khách hàng và kiểm tra thông tin thành công.");
        }
        // CCCD trùng, hợp lệ
        [TestMethod]
        public void TestAddCustomer2()
        {
            string cccd = "02021050751";
            string fullName = "Nguyễn Văn A";
            string phone = "0912345678";
            string email = "test@gmail.com";
            string address = "123 Đường ABC, Hà Nội";

            // Kiểm tra khách hàng tồn tại trước đó (setup dữ liệu sẵn có)
            var before = customer.GetCustomerDetails(cccd);
            Assert.IsNotNull(before, $"Khách hàng với CCCD [{cccd}] không tồn tại trước khi test, không thể kiểm thử trùng.");

            int countBefore = customer.CountAllCustomers();

            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();

            // Nhập thông tin trùng CCCD
            customer.AddCustomer(cccd, fullName, phone, email, address);

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

                Assert.AreEqual("CCCD đã tồn tại trong hệ thống.", errorElement.Text.Trim(), "Nội dung thông báo không đúng.");
                Console.WriteLine($"Thông báo hiển thị là: {errorElement.Text.Trim()}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi với CCCD đã tồn tại");
            }

            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù CCCD bị trùng.");

            var after = customer.GetCustomerDetails(cccd);
            Assert.IsNotNull(after, "Không lấy được thông tin chi tiết của khách hàng.");
            Assert.AreEqual(before.FullName, after.FullName, "Họ tên bị thay đổi.");
            Assert.AreEqual(before.Phone, after.Phone, "Số điện thoại bị ghi đè.");
            Assert.AreEqual(before.Email, after.Email, "Email bị ghi đè.");
            Assert.AreEqual(before.Address, after.Address, "Địa chỉ bị ghi đè.");

        }
        [TestMethod]
        //bỏ trống CCCD
        public void TestAddCustomer3()
        {
            string cccd = "";  // Trường hợp bỏ trống
            string fullName = "Nguyễn Văn A";
            string phone = "0912345678";
            string email = "nguyenvana12@gmail.com";
            string address = "123 Nguyễn Trãi, Hà Nội";

            int countBefore = customer.CountAllCustomers();

            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();

            // Thực hiện thêm khách hàng (CCCD rỗng)
            customer.AddCustomer(cccd, fullName, phone, email, address);

            // Tìm lại ô CCCD để kiểm tra validation trình duyệt
            var cccdField = driver.FindElement(By.Id("cccd"));
            string validationMessage = cccdField.GetAttribute("validationMessage");

            // Kiểm tra thông báo lỗi của trình duyệt (ngôn ngữ tiếng Anh)
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Thông báo lỗi: {validationMessage}");

            // Kiểm tra form vẫn mở (modal chưa đóng lại)
            bool isModalStillOpen = driver.FindElement(By.Id("addCustomerModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            // Kiểm tra số lượng khách hàng không thay đổi
            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có khách hàng nào được thêm vào hệ thống -> Pass");
        }
        [TestMethod]
        //CCCD < 10 
        public void TestAddCustomer4()
        {
            string cccd = "012345";  
            string fullName = "Nguyễn Văn A";
            string phone = "0912345678";
            string email = "nguyenvana12@gmail.com";
            string address = "123 Nguyễn Trãi, Hà Nội";

            int countBefore = customer.CountAllCustomers();

            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();

            // Thực hiện thêm khách hàng (CCCD rỗng)
            customer.AddCustomer(cccd, fullName, phone, email, address);

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

                Assert.AreEqual("CCCD không hợp lệ.", errorElement.Text.Trim(), "Nội dung thông báo không đúng.");
                Console.WriteLine($"Thông báo hiển thị là: {errorElement.Text.Trim()}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi với CCCD không hợp lệ");
            }

            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù CCCD không hợp lệ.");
            Console.WriteLine("Không có khách hàng nào được thêm vào hệ thống -> Pass");
        }
        [TestMethod]
        //CCCD > 13 
        public void TestAddCustomer5()
        {
            string cccd = "01212348923581934823";  // Trường hợp bỏ trống
            string fullName = "Nguyễn Văn A";
            string phone = "0912345678";
            string email = "nguyenvana12@gmail.com";
            string address = "123 Nguyễn Trãi, Hà Nội";

            int countBefore = customer.CountAllCustomers();

            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();

            // Thực hiện thêm khách hàng (CCCD rỗng)
            customer.AddCustomer(cccd, fullName, phone, email, address);

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

                Assert.AreEqual("CCCD không hợp lệ.", errorElement.Text.Trim(), "Nội dung thông báo không đúng.");
                Console.WriteLine($"Thông báo hiển thị là: {errorElement.Text.Trim()}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi với CCCD không hợp lệ");
            }

            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù CCCD không hợp lệ.");
            Console.WriteLine("Không có khách hàng nào được thêm vào hệ thống -> Pass");
        }
        [TestMethod]
        //CCCD # kí tự sô
        public void TestAddCustomer6()
        {
            string cccd = "test";  // Trường hợp bỏ trống
            string fullName = "Nguyễn Văn A";
            string phone = "0912345678";
            string email = "nguyenvana12@gmail.com";
            string address = "123 Nguyễn Trãi, Hà Nội";

            int countBefore = customer.CountAllCustomers();

            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();

            // Thực hiện thêm khách hàng (CCCD rỗng)
            customer.AddCustomer(cccd, fullName, phone, email, address);

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

                Assert.AreEqual("CCCD không hợp lệ.", errorElement.Text.Trim(), "Nội dung thông báo không đúng.");
                Console.WriteLine($"Thông báo hiển thị là: {errorElement.Text.Trim()}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi với CCCD không hợp lệ");
            }

            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù CCCD không hợp lệ.");
            Console.WriteLine("Không có khách hàng nào được thêm vào hệ thống -> Pass");
        }
        [TestMethod]
        //CCCD số đầu # 0
        public void TestAddCustomer7()
        {
            string cccd = "12345678911";  // Trường hợp bỏ trống
            string fullName = "Nguyễn Văn A";
            string phone = "0912345678";
            string email = "nguyenvana12@gmail.com";
            string address = "123 Nguyễn Trãi, Hà Nội";

            int countBefore = customer.CountAllCustomers();

            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();

          
            customer.AddCustomer(cccd, fullName, phone, email, address);

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

                Assert.AreEqual("CCCD không hợp lệ.", errorElement.Text.Trim(), "Nội dung thông báo không đúng.");
                Console.WriteLine($"Thông báo hiển thị là: {errorElement.Text.Trim()}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi với CCCD không hợp lệ");
            }

            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù CCCD không hợp lệ.");
            Console.WriteLine("Không có khách hàng nào được thêm vào hệ thống -> Pass");
        }
        // trống họ tên
        [TestMethod]
        public void TestAddCustomer8()
        {
            string cccd = "2222222222";
            string fullName = "";
            string phone = "0912345678";
            string email = "test@gmail.com";
            string address = "123 Đường ABC, Hà Nội";
            int countBefore = customer.CountAllCustomers();
            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();

            // Thực hiện nhập dữ liệu
            customer.AddCustomer(cccd, fullName, phone, email, address);

            // Kiểm tra thông báo lỗi mặc định trình duyệt 
            var cccdField = driver.FindElement(By.Id("full_name"));
            string validationMessage = cccdField.GetAttribute("validationMessage");

            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Thông báo lỗi: {validationMessage}");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addCustomerModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            int countAfter = customer.CountAllCustomers();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có khách hàng nào được thêm vào hệ thống -> Pass");

        }
        [TestMethod]
        public void TestAddCustomer9()
        {

            string cccd = "02234342222";
            string fullName = "test";
            string phone = "";
            string email = "test@gmail.com";
            string address = "123 Đường ABC, Hà Nội";
            int countBefore = customer.CountAllCustomers();
            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();

            // Thực hiện nhập dữ liệu
            customer.AddCustomer(cccd, fullName, phone, email, address);

            // Kiểm tra thông báo lỗi mặc định trình duyệt 
            var cccdField = driver.FindElement(By.Id("phone"));
            string validationMessage = cccdField.GetAttribute("validationMessage");

            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Thông báo lỗi: {validationMessage}");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addCustomerModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            int countAfter = customer.CountAllCustomers();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có khách hàng nào được thêm vào hệ thống -> Pass");

        }
        [TestMethod]
        public void TestAddCustomer10()
        {

            string cccd = "0000022221";
            string fullName = "test";
            string phone = "1234567";
            string email = "test@gmail.com";
            string address = "123 Đường ABC, Hà Nội";
            int countBefore = customer.CountAllCustomers();
            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();

            // Thực hiện nhập dữ liệu
            customer.AddCustomer(cccd, fullName, phone, email, address);
            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();


            customer.AddCustomer(cccd, fullName, phone, email, address);

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

                Assert.AreEqual("Số điện thoại không hợp lệ.", errorElement.Text.Trim(), "Nội dung thông báo không đúng.");
                Console.WriteLine($"Thông báo hiển thị là: {errorElement.Text.Trim()}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi với SĐT không hợp lệ");
            }

            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù SĐT không hợp lệ.");
            Console.WriteLine("Không có khách hàng nào được thêm vào hệ thống -> Pass");

        }

        [TestMethod]
        public void TestAddCustomer11()
        {

            string cccd = "0000022221";
            string fullName = "test";
            string phone = "0234567555555555";
            string email = "test@gmail.com";
            string address = "123 Đường ABC, Hà Nội";
            int countBefore = customer.CountAllCustomers();
            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();

            // Thực hiện nhập dữ liệu
            customer.AddCustomer(cccd, fullName, phone, email, address);
            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();


            customer.AddCustomer(cccd, fullName, phone, email, address);

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

                Assert.AreEqual("Số điện thoại không hợp lệ.", errorElement.Text.Trim(), "Nội dung thông báo không đúng.");
                Console.WriteLine($"Thông báo hiển thị là: {errorElement.Text.Trim()}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi với SĐT không hợp lệ");
            }

            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù SĐT không hợp lệ.");
            Console.WriteLine("Không có khách hàng nào được thêm vào hệ thống -> Pass");

        }
        [TestMethod]
        public void TestAddCustomer12()
        {

            string cccd = "0000022221";
            string fullName = "test";
            string phone = "test";
            string email = "test@gmail.com";
            string address = "123 Đường ABC, Hà Nội";
            int countBefore = customer.CountAllCustomers();
            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();

            // Thực hiện nhập dữ liệu
            customer.AddCustomer(cccd, fullName, phone, email, address);
            // Mở form thêm khách hàng

            // Kiểm tra thông báo lỗi mặc định trình duyệt 
            var cccdField = driver.FindElement(By.Id("phone"));
            string validationMessage = cccdField.GetAttribute("validationMessage");

            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Thông báo lỗi: {validationMessage}");
            // Đảm bảo form vẫn đang mở (không submit được)
            bool isModalStillOpen = driver.FindElement(By.Id("addCustomerModal")).GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            int countAfter = customer.CountAllCustomers();

            // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
            Console.WriteLine("Không có khách hàng nào được thêm vào hệ thống -> Pass");

        }
        [TestMethod]
        public void TestAddCustomer13()
        {

            string cccd = "0000022221";
            string fullName = "test";
            string phone = "1234567558";
            string email = "test@gmail.com";
            string address = "123 Đường ABC, Hà Nội";
            int countBefore = customer.CountAllCustomers();
            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();

            // Thực hiện nhập dữ liệu
            customer.AddCustomer(cccd, fullName, phone, email, address);
            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();


            customer.AddCustomer(cccd, fullName, phone, email, address);

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

                Assert.AreEqual("Số điện thoại không hợp lệ.", errorElement.Text.Trim(), "Nội dung thông báo không đúng.");
                Console.WriteLine($"Thông báo hiển thị là: {errorElement.Text.Trim()}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi với SĐT không hợp lệ");
            }

            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù SĐT không hợp lệ.");
            Console.WriteLine("Không có khách hàng nào được thêm vào hệ thống -> Pass");

        }
        [TestMethod]
        public void TestAddCustomer14()
        {

            string cccd = "0000022221";
            string fullName = "test";
            string phone = "0234567558";
            string email = "testgmail.com";
            string address = "123 Đường ABC, Hà Nội";
            int countBefore = customer.CountAllCustomers();
            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();

            // Thực hiện nhập dữ liệu
            customer.AddCustomer(cccd, fullName, phone, email, address);
            // Mở form thêm khách hàng
            customer.OpenAddCustomerSection();


            customer.AddCustomer(cccd, fullName, phone, email, address);

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

                Assert.AreEqual("Email không hợp lệ.", errorElement.Text.Trim(), "Nội dung thông báo không đúng.");
                Console.WriteLine($"Thông báo hiển thị là: {errorElement.Text.Trim()}");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("Không tìm thấy thông báo lỗi với email không hợp lệ");
            }

            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù email không hợp lệ.");
            Console.WriteLine("Không có khách hàng nào được thêm vào hệ thống -> Pass");

        }
        [TestMethod]
        public void TestAddCustomer15()
        {
          
            string cccd = "121"; 
            string fullName = ""; 
            string phone = "09123423429";
            string email = "nguyen";
            string address = "123 Nguyễn Trãi, Hà Nội";

            var customerBefore = customer.GetCustomerDetails(cccd);
            int countBefore = customer.CountAllCustomers();

          
            customer.OpenAddCustomerSection();

       
            customer.AddCustomer(cccd, fullName, phone, email, address);

            // 4. Kiểm tra lỗi 
            var fullNameInput = driver.FindElement(By.Id("full_name"));
            string validationMessage = fullNameInput.GetAttribute("validationMessage");
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Thông báo lỗi HTML5 tại 'full_name': {validationMessage}");

            // 5. Kiểm tra form vẫn đang mở
            bool isModalStillOpen = driver.FindElement(By.Id("addCustomerModal"))
                .GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ.");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            // 6. Kiểm tra không có khách hàng nào được thêm
            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");

            // 7. Kiểm tra không có alert vì form bị chặn trước khi submit
            var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

            Assert.AreEqual(0, alerts.Count, "Không nên có alert vì trình duyệt đã chặn submit.");
            Console.WriteLine("Không có alert lỗi hiển thị (đúng do bị HTML5 chặn).");

            // 8. Đảm bảo thông tin khách hàng có sẵn không bị ghi đè
            var customerAfter = customer.GetCustomerDetails(cccd);
            Assert.IsNotNull(customerAfter, $"Không tìm thấy khách hàng [{cccd}] sau khi test.");

            // So sánh với dữ liệu gốc (giả định đã tồn tại từ trước)

            Assert.AreEqual(customerBefore.FullName, customerAfter.FullName, "Tên bị ghi đè.");
            Assert.AreEqual(customerBefore.Phone, customerAfter.Phone, "SĐT bị ghi đè.");
            Assert.AreEqual(customerBefore.Email, customerAfter.Email, "Email bị ghi đè.");
            Assert.AreEqual(customerBefore.Address, customerAfter.Address, "Địa chỉ bị ghi đè.");
            Console.WriteLine("Dữ liệu khách hàng không bị thay đổi -> PASS");
        }
        //CCCD trùng, SDT trống
        [TestMethod]
        public void TestAddCustomer16()
        {
       
            string cccd = "121";
            string fullName = "test";
            string phone = "";
            string email = "nguyen";
            string address = "123 Nguyễn Trãi, Hà Nội";

            var customerBefore = customer.GetCustomerDetails(cccd);
            int countBefore = customer.CountAllCustomers();

          
            customer.OpenAddCustomerSection();

            customer.AddCustomer(cccd, fullName, phone, email, address);

            // 4. Kiểm tra lỗi 
            var fullNameInput = driver.FindElement(By.Id("phone"));
            string validationMessage = fullNameInput.GetAttribute("validationMessage");
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Thông báo lỗi HTML5 tại 'phone': {validationMessage}");

            // 5. Kiểm tra form vẫn đang mở
            bool isModalStillOpen = driver.FindElement(By.Id("addCustomerModal"))
                .GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ.");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            // 6. Kiểm tra không có khách hàng nào được thêm
            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");

            // 7. Kiểm tra không có alert vì form bị chặn trước khi submit
            var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

            Assert.AreEqual(0, alerts.Count, "Không nên có alert vì trình duyệt đã chặn submit.");
            Console.WriteLine("Không có alert lỗi hiển thị (đúng do bị HTML5 chặn).");

            // 8. Đảm bảo thông tin khách hàng có sẵn không bị ghi đè
            var customerAfter = customer.GetCustomerDetails(cccd);
            Assert.IsNotNull(customerAfter, $"Không tìm thấy khách hàng [{cccd}] sau khi test.");

            // So sánh với dữ liệu gốc 

            Assert.AreEqual(customerBefore.FullName, customerAfter.FullName, "Tên bị ghi đè.");
            Assert.AreEqual(customerBefore.Phone, customerAfter.Phone, "SĐT bị ghi đè.");
            Assert.AreEqual(customerBefore.Email, customerAfter.Email, "Email bị ghi đè.");
            Assert.AreEqual(customerBefore.Address, customerAfter.Address, "Địa chỉ bị ghi đè.");
            Console.WriteLine("Dữ liệu khách hàng không bị thay đổi -> PASS");
        }
        //CCCD trùng, SDT <10
        [TestMethod]
        public void TestAddCustomer17()
        {

            string cccd = "02021050751";
            string fullName = "test";
            string phone = "1234";
            string email = "nguyen";
            string address = "123 Nguyễn Trãi, Hà Nội";
            string expectedMessage = "Số điện thoại không hợp lệ.";
            var customerBefore = customer.GetCustomerDetails(cccd);
            int countBefore = customer.CountAllCustomers();


            customer.OpenAddCustomerSection();

            customer.AddCustomer(cccd, fullName, phone, email, address);

            // 4. Kiểm tra lỗi 
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

            // 6. Kiểm tra không có khách hàng nào được thêm
            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");

       
           

            // 8. Đảm bảo thông tin khách hàng có sẵn không bị ghi đè
            var customerAfter = customer.GetCustomerDetails(cccd);
            Assert.IsNotNull(customerAfter, $"Không tìm thấy khách hàng [{cccd}] sau khi test.");

            // So sánh với dữ liệu gốc 

            Assert.AreEqual(customerBefore.FullName, customerAfter.FullName, "Tên bị ghi đè.");
            Assert.AreEqual(customerBefore.Phone, customerAfter.Phone, "SĐT bị ghi đè.");
            Assert.AreEqual(customerBefore.Email, customerAfter.Email, "Email bị ghi đè.");
            Assert.AreEqual(customerBefore.Address, customerAfter.Address, "Địa chỉ bị ghi đè.");
            Console.WriteLine("Dữ liệu khách hàng không bị thay đổi -> PASS");
        }
        //CCCD trùng, SDT >13
        [TestMethod]
        public void TestAddCustomer18()
        {
            // 1. Thiết lập dữ liệu đầu vào không hợp lệ
            string cccd = "02021050751";
            string fullName = "test";
            string phone = "01234678915777"; // quá dài
            string email = "nguyen";
            string address = "123 Nguyễn Trãi, Hà Nội";
            string expectedMessage = "Số điện thoại không hợp lệ.";

            // 2. Lưu lại thông tin khách hàng gốc (nếu có)
            var customerBefore = customer.GetCustomerDetails(cccd);
            int countBefore = customer.CountAllCustomers();

            // 3. Mở form thêm khách hàng
            customer.OpenAddCustomerSection();
            customer.AddCustomer(cccd, fullName, phone, email, address);

            // 4. Đợi alert lỗi xuất hiện
            var alerts = wait.Until(driver =>
            {
                var found = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                    .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();
                return found.Count > 0 ? found : null;
            });

            // 5. Kiểm tra số lượng và nội dung thông báo lỗi
            Assert.AreEqual(1, alerts.Count, $"Có {alerts.Count} thông báo lỗi hiển thị, mong đợi chỉ 1.");
            string actualError = alerts[0].Text.Trim();
            Assert.AreEqual(expectedMessage, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedMessage}\" - Thực tế: \"{actualError}\"");
            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            // 6. Kiểm tra không có khách hàng nào được thêm
            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");

            // 7. Kiểm tra dữ liệu khách hàng không bị ghi đè
            var customerAfter = customer.GetCustomerDetails(cccd);
            Assert.IsNotNull(customerAfter, $"Không tìm thấy khách hàng [{cccd}] sau khi test.");

            // Nếu trước đó đã có khách hàng, kiểm tra không bị ghi đè
            //if (customerBefore != null)
            //{
                Assert.AreEqual(customerBefore.FullName, customerAfter.FullName, "Tên bị ghi đè.");
                Assert.AreEqual(customerBefore.Phone, customerAfter.Phone, "SĐT bị ghi đè.");
                Assert.AreEqual(customerBefore.Email, customerAfter.Email, "Email bị ghi đè.");
                Assert.AreEqual(customerBefore.Address, customerAfter.Address, "Địa chỉ bị ghi đè.");
                Console.WriteLine("Dữ liệu khách hàng không bị thay đổi -> PASS");
            //}
            //else
            //{
            //    Console.WriteLine("Không có khách hàng ban đầu -> Bỏ qua kiểm tra ghi đè.");
            //}
        }
        // CCCD trùng, SDT kh phải kí tự số
        [TestMethod]
        public void TestAddCustomer19()
        {
            // 1. Thiết lập dữ liệu đầu vào không hợp lệ
            string cccd = "02021050751";
            string fullName = "test";
            string phone = "test"; 
            string email = "nguyen";
            string address = "123 Nguyễn Trãi, Hà Nội";
            string expectedMessage = "Số điện thoại không hợp lệ.";

            // 2. Lưu lại thông tin khách hàng gốc (nếu có)
            var customerBefore = customer.GetCustomerDetails(cccd);
            int countBefore = customer.CountAllCustomers();

            // 3. Mở form thêm khách hàng
            customer.OpenAddCustomerSection();
            customer.AddCustomer(cccd, fullName, phone, email, address);

            // 4. Kiểm tra lỗi 
            var fullNameInput = driver.FindElement(By.Id("phone"));
            string validationMessage = fullNameInput.GetAttribute("validationMessage");
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Thông báo lỗi HTML5 tại 'phone': {validationMessage}");

            // 5. Kiểm tra form vẫn đang mở
            bool isModalStillOpen = driver.FindElement(By.Id("addCustomerModal"))
                .GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ.");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            // 6. Kiểm tra không có khách hàng nào được thêm
            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");

            // 7. Kiểm tra không có alert vì form bị chặn trước khi submit
            var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

            Assert.AreEqual(0, alerts.Count, "Không nên có alert vì trình duyệt đã chặn submit.");
            Console.WriteLine("Không có alert lỗi hiển thị (đúng do bị HTML5 chặn).");

            // 8. Đảm bảo thông tin khách hàng có sẵn không bị ghi đè
            var customerAfter = customer.GetCustomerDetails(cccd);
            Assert.IsNotNull(customerAfter, $"Không tìm thấy khách hàng [{cccd}] sau khi test.");

            // So sánh với dữ liệu gốc 
            Assert.AreEqual(customerBefore.FullName, customerAfter.FullName, "Tên bị ghi đè.");
            Assert.AreEqual(customerBefore.Phone, customerAfter.Phone, "SĐT bị ghi đè.");
            Assert.AreEqual(customerBefore.Email, customerAfter.Email, "Email bị ghi đè.");
            Assert.AreEqual(customerBefore.Address, customerAfter.Address, "Địa chỉ bị ghi đè.");
            Console.WriteLine("Dữ liệu khách hàng không bị thay đổi -> PASS");

        }
        //CCCD trùng, SDT đầu  # 0
        [TestMethod]
        public void TestAddCustomer20()
        {
            // 1. Thiết lập dữ liệu đầu vào không hợp lệ
            string cccd = "02021050751";
            string fullName = "test";
            string phone = "123467891"; // quá dài
            string email = "nguyen";
            string address = "123 Nguyễn Trãi, Hà Nội";
            string expectedMessage = "Số điện thoại không hợp lệ.";

            // 2. Lưu lại thông tin khách hàng gốc (nếu có)
            var customerBefore = customer.GetCustomerDetails(cccd);
            int countBefore = customer.CountAllCustomers();

            // 3. Mở form thêm khách hàng
            customer.OpenAddCustomerSection();
            customer.AddCustomer(cccd, fullName, phone, email, address);

            // 4. Đợi alert lỗi xuất hiện
            var alerts = wait.Until(driver =>
            {
                var found = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                    .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();
                return found.Count > 0 ? found : null;
            });

            // 5. Kiểm tra số lượng và nội dung thông báo lỗi
            Assert.AreEqual(1, alerts.Count, $"Có {alerts.Count} thông báo lỗi hiển thị, mong đợi chỉ 1.");
            string actualError = alerts[0].Text.Trim();
            Assert.AreEqual(expectedMessage, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedMessage}\" - Thực tế: \"{actualError}\"");
            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            // 6. Kiểm tra không có khách hàng nào được thêm
            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");

            // 7. Kiểm tra dữ liệu khách hàng không bị ghi đè
            var customerAfter = customer.GetCustomerDetails(cccd);
            Assert.IsNotNull(customerAfter, $"Không tìm thấy khách hàng [{cccd}] sau khi test.");

            
            Assert.AreEqual(customerBefore.FullName, customerAfter.FullName, "Tên bị ghi đè.");
            Assert.AreEqual(customerBefore.Phone, customerAfter.Phone, "SĐT bị ghi đè.");
            Assert.AreEqual(customerBefore.Email, customerAfter.Email, "Email bị ghi đè.");
            Assert.AreEqual(customerBefore.Address, customerAfter.Address, "Địa chỉ bị ghi đè.");
            Console.WriteLine("Dữ liệu khách hàng không bị thay đổi -> PASS");
          
        }
        //CCCD trùng, email sai dịnh dạng
        [TestMethod]
        public void TestAddCustomer21()
        {
            // 1. Thiết lập dữ liệu đầu vào không hợp lệ
            string cccd = "02021050751";
            string fullName = "test";
            string phone = "0123467891"; // quá dài
            string email = "nguyenfff";
            string address = "123 Nguyễn Trãi, Hà Nội";
            string expectedMessage = "Email không hợp lệ.";

            // 2. Lưu lại thông tin khách hàng gốc (nếu có)
            var customerBefore = customer.GetCustomerDetails(cccd);
            int countBefore = customer.CountAllCustomers();

            // 3. Mở form thêm khách hàng
            customer.OpenAddCustomerSection();
            customer.AddCustomer(cccd, fullName, phone, email, address);

            // 4. Đợi alert lỗi xuất hiện
            var alerts = wait.Until(driver =>
            {
                var found = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                    .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();
                return found.Count > 0 ? found : null;
            });

            // 5. Kiểm tra số lượng và nội dung thông báo lỗi
            Assert.AreEqual(1, alerts.Count, $"Có {alerts.Count} thông báo lỗi hiển thị, mong đợi chỉ 1.");
            string actualError = alerts[0].Text.Trim();
            Assert.AreEqual(expectedMessage, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedMessage}\" - Thực tế: \"{actualError}\"");
            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            // 6. Kiểm tra không có khách hàng nào được thêm
            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");

            // 7. Kiểm tra dữ liệu khách hàng không bị ghi đè
            var customerAfter = customer.GetCustomerDetails(cccd);
            Assert.IsNotNull(customerAfter, $"Không tìm thấy khách hàng [{cccd}] sau khi test.");


            Assert.AreEqual(customerBefore.FullName, customerAfter.FullName, "Tên bị ghi đè.");
            Assert.AreEqual(customerBefore.Phone, customerAfter.Phone, "SĐT bị ghi đè.");
            Assert.AreEqual(customerBefore.Email, customerAfter.Email, "Email bị ghi đè.");
            Assert.AreEqual(customerBefore.Address, customerAfter.Address, "Địa chỉ bị ghi đè.");
            Console.WriteLine("Dữ liệu khách hàng không bị thay đổi -> PASS");

        }
        [TestMethod]
        public void TestAddCustomer22()
        {
            // 1. Thiết lập dữ liệu đầu vào không hợp lệ
            string cccd = "02021050766";
            string fullName = "test";
            string phone = "012346"; // quá dài
            string email = "nguyenfff";
            string address = "123 Nguyễn Trãi, Hà Nội";
            string expectedMessage = "Số điện thoại không hợp lệ.";

           
            int countBefore = customer.CountAllCustomers();

            // 3. Mở form thêm khách hàng
            customer.OpenAddCustomerSection();
            customer.AddCustomer(cccd, fullName, phone, email, address);

            // 4. Đợi alert lỗi xuất hiện
            var alerts = wait.Until(driver =>
            {
                var found = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                    .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();
                return found.Count > 0 ? found : null;
            });

            // 5. Kiểm tra số lượng và nội dung thông báo lỗi
            Assert.AreEqual(1, alerts.Count, $"Có {alerts.Count} thông báo lỗi hiển thị, mong đợi chỉ 1.");
            string actualError = alerts[0].Text.Trim();
            Assert.AreEqual(expectedMessage, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedMessage}\" - Thực tế: \"{actualError}\"");
            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            // 6. Kiểm tra không có khách hàng nào được thêm
            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");
            Console.WriteLine("Dữ liệu khách hàng không thêm -> PASS");

        }
        [TestMethod]
        public void TestAddCustomer23()
        {
            // 1. Thiết lập dữ liệu đầu vào không hợp lệ
            string cccd = "02021050766";
            string fullName = "test";
            string phone = "012346343243443"; 
            string email = "nguyenfff";
            string address = "123 Nguyễn Trãi, Hà Nội";
            string expectedMessage = "Số điện thoại không hợp lệ.";


            int countBefore = customer.CountAllCustomers();

            // 3. Mở form thêm khách hàng
            customer.OpenAddCustomerSection();
            customer.AddCustomer(cccd, fullName, phone, email, address);

            // 4. Đợi alert lỗi xuất hiện
            var alerts = wait.Until(driver =>
            {
                var found = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                    .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();
                return found.Count > 0 ? found : null;
            });

            // 5. Kiểm tra số lượng và nội dung thông báo lỗi
            Assert.AreEqual(1, alerts.Count, $"Có {alerts.Count} thông báo lỗi hiển thị, mong đợi chỉ 1.");
            string actualError = alerts[0].Text.Trim();
            Assert.AreEqual(expectedMessage, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedMessage}\" - Thực tế: \"{actualError}\"");
            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            // 6. Kiểm tra không có khách hàng nào được thêm
            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");
            Console.WriteLine("Dữ liệu khách hàng không thêm -> PASS");

        }
        [TestMethod]
        public void TestAddCustomer24()
        {
            // 1. Thiết lập dữ liệu đầu vào không hợp lệ
            string cccd = "02021050766";
            string fullName = "test";
            string phone = "test";
            string email = "nguyenfff";
            string address = "123 Nguyễn Trãi, Hà Nội";
            string expectedMessage = "Số điện thoại không hợp lệ.";


            int countBefore = customer.CountAllCustomers();

            // 3. Mở form thêm khách hàng
            customer.OpenAddCustomerSection();
            customer.AddCustomer(cccd, fullName, phone, email, address);

            var fullNameInput = driver.FindElement(By.Id("phone"));
            string validationMessage = fullNameInput.GetAttribute("validationMessage");
            Assert.AreEqual("Please fill out this field.", validationMessage);
            Console.WriteLine($"Thông báo lỗi HTML5 tại 'phone': {validationMessage}");

            // 5. Kiểm tra form vẫn đang mở
            bool isModalStillOpen = driver.FindElement(By.Id("addCustomerModal"))
                .GetAttribute("class").Contains("show");
            Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ.");
            Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

            // 6. Kiểm tra không có khách hàng nào được thêm
            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");
            Console.WriteLine("Dữ liệu khách hàng không thêm -> PASS");

        }
        [TestMethod]
        public void TestAddCustomer25()
        {
            // 1. Thiết lập dữ liệu đầu vào không hợp lệ
            string cccd = "02021050766";
            string fullName = "test";
            string phone = "123463432";
            string email = "nguyenfff";
            string address = "123 Nguyễn Trãi, Hà Nội";
            string expectedMessage = "Số điện thoại không hợp lệ.";


            int countBefore = customer.CountAllCustomers();

            // 3. Mở form thêm khách hàng
            customer.OpenAddCustomerSection();
            customer.AddCustomer(cccd, fullName, phone, email, address);

            // 4. Đợi alert lỗi xuất hiện
            var alerts = wait.Until(driver =>
            {
                var found = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
                    .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();
                return found.Count > 0 ? found : null;
            });

            // 5. Kiểm tra số lượng và nội dung thông báo lỗi
            Assert.AreEqual(1, alerts.Count, $"Có {alerts.Count} thông báo lỗi hiển thị, mong đợi chỉ 1.");
            string actualError = alerts[0].Text.Trim();
            Assert.AreEqual(expectedMessage, actualError,
                $"Nội dung lỗi không đúng. Mong đợi: \"{expectedMessage}\" - Thực tế: \"{actualError}\"");
            Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

            // 6. Kiểm tra không có khách hàng nào được thêm
            int countAfter = customer.CountAllCustomers();
            Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");
            Console.WriteLine("Dữ liệu khách hàng không thêm -> PASS");

        }
        /// Đóng Form 
        ///--> Khi chưa nhập text
        [TestMethod]
        public void TestCustomer26()
        {
            customer.OpenAddCustomerSection(); // Mở form thêm khách hàng

            customer.CloseForm1(); // Đóng form mà không nhập

            bool isModalClosed = wait.Until(driver =>
            {
                try
                {
                    return !driver.FindElement(By.Id("addCustomerModal")).GetAttribute("class").Contains("show");
                }
                catch (NoSuchElementException)
                {
                    return true; // Modal đã bị remove
                }
            });

            if (isModalClosed)
                Console.WriteLine("Form thêm khách hàng đã được đóng thành công.");
            else
                Console.WriteLine("Form thêm khách hàng vẫn đang hiển thị.");

            Assert.IsTrue(isModalClosed, "Form thêm khách hàng chưa được đóng sau khi nhấn nút Đóng.");
        }

        ///--> Khi nhập text nhưng chưa lưu
        [TestMethod]
        public void TestCustomer27()
        {
            customer.OpenAddCustomerSection(); // Mở form thêm

            // Nhập dữ liệu rồi đóng (không submit)
            customer.CloseForm2("123456789099", "Nguyễn Văn Test", "0912345678", "test@example.com", "123 Đường ABC");

            bool isModalClosed = wait.Until(driver =>
            {
                try
                {
                    return !driver.FindElement(By.Id("addCustomerModal")).GetAttribute("class").Contains("show");
                }
                catch (NoSuchElementException)
                {
                    return true;
                }
            });

            if (isModalClosed)
                Console.WriteLine("Form thêm khách hàng đã được đóng thành công sau khi nhập dữ liệu.");
            else
                Console.WriteLine("Form thêm khách hàng vẫn đang hiển thị.");

            Assert.IsTrue(isModalClosed, "Form thêm khách hàng chưa được đóng sau khi nhấn nút Đóng.");
        }


    }
}

