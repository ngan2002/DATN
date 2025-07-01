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
            string cccd = "2021050751";               
            string fullName = "Nguyễn Văn A";            
            string phone = "0912345678";                 
            string email = "nguyenvana12@gmail.com";       
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

            Console.WriteLine("TestAddCustomer1: PASSED - Thêm khách hàng và kiểm tra thông tin thành công.");
        }
        // CCCD trùng, hợp lệ
        [TestMethod]
        public void TestAddCustomer2()
        {
            string cccd = "202105075111";
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
        //CCCD =< 10 
        public void TestAddCustomer4()
        {
            string cccd = "1234";  // Trường hợp bỏ trống
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
        //CCCD >= 13 
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
        //[TestMethod]
        //public void TestAddCustomer6()
        //{

        //    string cccd = "2222";
        //    string fullName = "test";
        //    string phone = "test";
        //    string email = "test@gmail.com";
        //    string address = "123 Đường ABC, Hà Nội";
        //    int countBefore = customer.CountAllCustomers();
        //    // Mở form thêm khách hàng
        //    customer.OpenAddCustomerSection();

        //    // Thực hiện nhập dữ liệu
        //    customer.AddCustomer(cccd, fullName, phone, email, address);

        //    // Kiểm tra thông báo lỗi mặc định trình duyệt 
        //    var cccdField = driver.FindElement(By.Id("phone"));
        //    string validationMessage = cccdField.GetAttribute("validationMessage");

        //    Assert.AreEqual("Please fill out this field.", validationMessage);
        //    Console.WriteLine($"Thông báo lỗi: {validationMessage}");
        //    // Đảm bảo form vẫn đang mở (không submit được)
        //    bool isModalStillOpen = driver.FindElement(By.Id("addCustomerModal")).GetAttribute("class").Contains("show");
        //    Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
        //    Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

        //    int countAfter = customer.CountAllCustomers();

        //    // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
        //    Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
        //    Console.WriteLine("Không có khách hàng nào được thêm vào hệ thống -> Pass");

        //}
        //[TestMethod]
        //// email sai định dạng
        //public void TestAddCustomer7()
        //{
        //    // Thiết lập dữ liệu đầu vào
        //    string cccd = "2021";
        //    string fullName = "Nguyễn Văn A";
        //    string phone = "0912345678";
        //    string email = "nguyen";
        //    string address = "123 Nguyễn Trãi, Hà Nội";
        //    int countBefore = customer.CountAllCustomers();
        //    // Mở form thêm khách hàng
        //    customer.OpenAddCustomerSection();

        //    // Thực hiện thêm khách hàng
        //    customer.AddCustomer(cccd, fullName, phone, email, address);

        //    // Kiểm tra thông báo lỗi mặc định trình duyệt 
        //    var cccdField = driver.FindElement(By.Id("email"));
        //    string validationMessage = cccdField.GetAttribute("validationMessage");

        //    string expected = $"Please include an '@' in the email address. '{email}' is missing an '@'.";
        //    Assert.AreEqual(expected, validationMessage);
        //    // Đảm bảo form vẫn đang mở (không submit được)
        //    bool isModalStillOpen = driver.FindElement(By.Id("addCustomerModal")).GetAttribute("class").Contains("show");
        //    Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ");
        //    Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

        //    int countAfter = customer.CountAllCustomers();

        //    // So sánh số lượng phòng để đảm bảo không có phòng mới được thêm
        //    Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm phòng mới dù dữ liệu không hợp lệ.");
        //    Console.WriteLine("Không có khách hàng nào được thêm vào hệ thống -> Pass");
        //}
        //[TestMethod]
        //public void TestAddCustomer8()
        //{
        //    // 1. Thiết lập dữ liệu đầu vào
        //    string cccd = "12"; // Giả định đã tồn tại
        //    string fullName = ""; // Bỏ trống fullname để trigger HTML5 validation
        //    string phone = "0912345678";
        //    string email = "nguyen";
        //    string address = "123 Nguyễn Trãi, Hà Nội";

        //    int countBefore = customer.CountAllCustomers();

        //    // 2. Mở form thêm khách hàng
        //    customer.OpenAddCustomerSection();

        //    // 3. Nhập và submit form
        //    customer.AddCustomer(cccd, fullName, phone, email, address);

        //    // 4. Kiểm tra lỗi HTML5 tại trường full_name
        //    var fullNameInput = driver.FindElement(By.Id("full_name"));
        //    string validationMessage = fullNameInput.GetAttribute("validationMessage");
        //    Assert.AreEqual("Please fill out this field.", validationMessage);
        //    Console.WriteLine($"Thông báo lỗi HTML5 tại 'full_name': {validationMessage}");

        //    // 5. Kiểm tra form vẫn đang mở
        //    bool isModalStillOpen = driver.FindElement(By.Id("addCustomerModal"))
        //        .GetAttribute("class").Contains("show");
        //    Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ.");
        //    Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

        //    // 6. Kiểm tra không có khách hàng nào được thêm
        //    int countAfter = customer.CountAllCustomers();
        //    Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");

        //    // 7. Kiểm tra không có alert vì form bị chặn trước khi submit
        //    var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
        //        .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

        //    Assert.AreEqual(0, alerts.Count, "Không nên có alert vì trình duyệt đã chặn submit.");
        //    Console.WriteLine("Không có alert lỗi hiển thị (đúng do bị HTML5 chặn).");

        //    // 8. Đảm bảo thông tin khách hàng có sẵn không bị ghi đè
        //    var customerAfter = customer.GetCustomerDetails(cccd);
        //    Assert.IsNotNull(customerAfter, $"Không tìm thấy khách hàng [{cccd}] sau khi test.");

        //    // So sánh với dữ liệu gốc (giả định đã tồn tại từ trước)
        //    Assert.AreNotEqual("", customerAfter.FullName, "Tên khách hàng bị ghi đè.");
        //    Assert.AreNotEqual(phone, customerAfter.Phone, "Số điện thoại bị ghi đè.");
        //    Assert.AreNotEqual(address, customerAfter.Address, "Địa chỉ bị ghi đè.");
        //    Console.WriteLine("Dữ liệu khách hàng không bị thay đổi -> PASS");
        //}


        //[TestMethod]
        //// CCCD trùng, , fullname trống
        //public void TestAddCustomer9()
        //{
        //    // 1. Thiết lập dữ liệu đầu vào
        //    string cccd = "12"; // Giả định đã tồn tại
        //    string fullName = "Trang"; 
        //    string phone = "";
        //    string email = "nguyen";
        //    string address = "123 Nguyễn Trãi, Hà Nội";

        //    int countBefore = customer.CountAllCustomers();

        //    // 2. Mở form thêm khách hàng
        //    customer.OpenAddCustomerSection();

        //    // 3. Nhập và submit form
        //    customer.AddCustomer(cccd, fullName, phone, email, address);

        //    // 4. Kiểm tra lỗi HTML5 tại trường full_name
        //    var phoneInput = driver.FindElement(By.Id("phone"));
        //    string validationMessage = phoneInput.GetAttribute("validationMessage");
        //    Assert.AreEqual("Please fill out this field.", validationMessage);
        //    Console.WriteLine($"Thông báo lỗi HTML5 tại 'full_name': {validationMessage}");

        //    // 5. Kiểm tra form vẫn đang mở
        //    bool isModalStillOpen = driver.FindElement(By.Id("addCustomerModal"))
        //        .GetAttribute("class").Contains("show");
        //    Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ.");
        //    Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

        //    // 6. Kiểm tra không có khách hàng nào được thêm
        //    int countAfter = customer.CountAllCustomers();
        //    Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");

        //    // 7. Kiểm tra thông báo lỗi giao diện (nếu có)

        //    var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
        //        .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

        //    Assert.AreEqual(0, alerts.Count, "Không nên có alert vì trình duyệt đã chặn submit.");
        //    Console.WriteLine("Không có alert lỗi hiển thị (đúng do bị HTML5 chặn).");

        //    // 8. Đảm bảo thông tin khách hàng vẫn giữ nguyên
        //    var customerAfter = customer.GetCustomerDetails(cccd);
        //    Assert.IsNotNull(customerAfter, $"Không tìm thấy khách hàng [{cccd}] sau khi test.");

        //    // So sánh với dữ liệu gốc (giả định đã tồn tại từ trước)
        //    Assert.AreNotEqual(fullName, customerAfter.FullName, "Tên khách hàng bị ghi đè.");
        //    Assert.AreNotEqual(phone, customerAfter.Phone, "Số điện thoại bị ghi đè.");
        //    Assert.AreNotEqual(address, customerAfter.Address, "Địa chỉ bị ghi đè.");
        //    Console.WriteLine("Dữ liệu khách hàng không bị thay đổi -> PASS");
        //}
        //[TestMethod]
        //public void TestAddCustomer10()
        //{
        //    // 1. Thiết lập dữ liệu đầu vào
        //    string cccd = "12"; // Giả định đã tồn tại
        //    string fullName = "Trang";
        //    string phone = "12345678912333333"; // quá 11 số
        //    string email = "abc@gmail.com"; // cũng sai định dạng
        //    string address = "123 Nguyễn Trãi, Hà Nội";

        //    int countBefore = customer.CountAllCustomers();
        //    string expectedMessage = "Số điện thoại không hợp lệ.";
        //    // 2. Mở form thêm khách hàng
        //    customer.OpenAddCustomerSection();

        //    // 3. Thực hiện thêm khách hàng
        //    customer.AddCustomer(cccd, fullName, phone, email, address);

        //    // 4. Đợi thông báo lỗi hiển thị

        //    var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
        //        .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

        //    // Kiểm tra có ít nhất 1 lỗi
        //    if (alerts.Count == 0)
        //    {
        //        Assert.Fail("Không có thông báo lỗi nào được hiển thị.");
        //    }

        //    // Kiểm tra chỉ có 1 lỗi
        //    Assert.AreEqual(1, alerts.Count, "Có nhiều hơn 1 thông báo lỗi hiển thị.");

        //    // Kiểm tra nội dung lỗi đúng
        //    string actualError = alerts[0].Text.Trim();
        //    Assert.AreEqual(expectedMessage, actualError,
        //        $"Nội dung lỗi không đúng. Mong đợi: \"{expectedMessage}\" - Thực tế: \"{actualError}\"");

        //    Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

        //    // 6. Đảm bảo khách hàng không bị thêm mới
        //    int countAfter = customer.CountAllCustomers();
        //    Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng mới dù có lỗi.");

        //    // 7. Đảm bảo khách hàng cũ không bị thay đổi
        //    var customerAfter = customer.GetCustomerDetails(cccd);
        //    Assert.IsNotNull(customerAfter, $"Không tìm thấy khách hàng [{cccd}] sau khi test.");

        //    // So sánh với dữ liệu gốc (giả định đã tồn tại từ trước)
        //    Assert.AreNotEqual(fullName, customerAfter.FullName, "Tên khách hàng bị ghi đè.");
        //    Assert.AreNotEqual(phone, customerAfter.Phone, "Số điện thoại bị ghi đè.");
        //    Assert.AreNotEqual(address, customerAfter.Address, "Địa chỉ bị ghi đè.");
        //    Console.WriteLine("Dữ liệu khách hàng không bị thay đổi -> PASS");
        //}
        //[TestMethod]
        //public void TestAddCustomer11()
        //{
        //    // 1. Thiết lập dữ liệu đầu vào
        //    string cccd = "121"; // Giả định đã tồn tại
        //    string fullName = "Trang";
        //    string phone = "1234"; // <10
        //    string email = "abc@gmail.com"; // cũng sai định dạng
        //    string address = "123 Nguyễn Trãi, Hà Nội";
        //    string expectedMessage = "Số điện thoại không hợp lệ.";
        //    int countBefore = customer.CountAllCustomers();

        //    // 2. Mở form thêm khách hàng
        //    customer.OpenAddCustomerSection();

        //    // 3. Thực hiện thêm khách hàng
        //    customer.AddCustomer(cccd, fullName, phone, email, address);



        //    var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
        //        .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

        //    // Kiểm tra có ít nhất 1 lỗi
        //    if (alerts.Count == 0)
        //    {
        //        Assert.Fail("Không có thông báo lỗi nào được hiển thị.");
        //    }

        //    // Kiểm tra chỉ có 1 lỗi
        //    Assert.AreEqual(1, alerts.Count, "Có nhiều hơn 1 thông báo lỗi hiển thị.");

        //    // Kiểm tra nội dung lỗi đúng
        //    string actualError = alerts[0].Text.Trim();
        //    Assert.AreEqual(expectedMessage, actualError,
        //        $"Nội dung lỗi không đúng. Mong đợi: \"{expectedMessage}\" - Thực tế: \"{actualError}\"");

        //    Console.WriteLine("Hiển thị đúng một lỗi duy nhất: " + actualError);

        //    // 6. Đảm bảo khách hàng không bị thêm mới
        //    int countAfter = customer.CountAllCustomers();
        //    Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng mới dù có lỗi.");

        //    // 7. Đảm bảo khách hàng cũ không bị thay đổi
        //    var customerAfter = customer.GetCustomerDetails(cccd);
        //    Assert.IsNotNull(customerAfter, $"Không tìm thấy khách hàng [{cccd}] sau khi test.");

        //    // So sánh với dữ liệu gốc (giả định đã tồn tại từ trước)
        //    Assert.AreNotEqual(fullName, customerAfter.FullName, "Tên khách hàng bị ghi đè.");
        //    Assert.AreNotEqual(phone, customerAfter.Phone, "Số điện thoại bị ghi đè.");
        //    Assert.AreNotEqual(address, customerAfter.Address, "Địa chỉ bị ghi đè.");
        //    Console.WriteLine("Dữ liệu khách hàng không bị thay đổi -> PASS");
        //}
        //[TestMethod]
        //public void TestAddCustomer12()
        //{
        //    // 1. Thiết lập dữ liệu đầu vào
        //    string cccd = "12"; 
        //    string fullName = "Trang";
        //    string phone = "test"; 
        //    string email = "nguyen"; // cũng sai định dạng
        //    string address = "123 Nguyễn Trãi, Hà Nội";

        //    int countBefore = customer.CountAllCustomers();

        //    // 2. Mở form thêm khách hàng
        //    customer.OpenAddCustomerSection();

        //    // 3. Thực hiện thêm khách hàng
        //    customer.AddCustomer(cccd, fullName, phone, email, address);

        //    // 4. Đợi thông báo lỗi hiển thị
        //    var phoneInput = driver.FindElement(By.Id("phone"));
        //    string validationMessage = phoneInput.GetAttribute("validationMessage");
        //    Assert.AreEqual("Please fill out this field.", validationMessage);
        //    Console.WriteLine($"Thông báo lỗi HTML5 tại 'SĐT': {validationMessage}");

        //    // 5. Kiểm tra form vẫn đang mở
        //    bool isModalStillOpen = driver.FindElement(By.Id("addCustomerModal"))
        //        .GetAttribute("class").Contains("show");
        //    Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ.");
        //    Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

        //    // 6. Kiểm tra không có khách hàng nào được thêm
        //    int countAfter = customer.CountAllCustomers();
        //    Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");

        //    // 7. Kiểm tra thông báo lỗi giao diện (nếu có)

        //    var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
        //        .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

        //    Assert.AreEqual(0, alerts.Count, "Không nên có alert vì trình duyệt đã chặn submit.");
        //    Console.WriteLine("Không có alert lỗi hiển thị (đúng do bị HTML5 chặn).");

        //    // 8. Đảm bảo thông tin khách hàng vẫn giữ nguyên
        //    var customerAfter = customer.GetCustomerDetails(cccd);
        //    Assert.IsNotNull(customerAfter, $"Không tìm thấy khách hàng [{cccd}] sau khi test.");

        //    // So sánh với dữ liệu gốc (giả định đã tồn tại từ trước)
        //    Assert.AreNotEqual(fullName, customerAfter.FullName, "Tên khách hàng bị ghi đè.");
        //    Assert.AreNotEqual(phone, customerAfter.Phone, "Số điện thoại bị ghi đè.");
        //    Assert.AreNotEqual(address, customerAfter.Address, "Địa chỉ bị ghi đè.");
        //    Console.WriteLine("Dữ liệu khách hàng không bị thay đổi -> PASS");
        //}
        //[TestMethod]
        //public void TestAddCustomer13()
        //{
        //    // 1. Thiết lập dữ liệu đầu vào
        //    string cccd = "12"; // Giả định đã tồn tại
        //    string fullName = "Trang";
        //    string phone = "123456876"; 
        //    string email = "nguyen"; // cũng sai định dạng
        //    string address = "123 Nguyễn Trãi, Hà Nội";

        //    int countBefore = customer.CountAllCustomers();
        //    string expectedMessage = "Số điện thoại không hợp lệ.";
        //    // 2. Mở form thêm khách hàng
        //    customer.OpenAddCustomerSection();

        //    // 3. Thực hiện thêm khách hàng
        //    customer.AddCustomer(cccd, fullName, phone, email, address);

        //    var cccdField = driver.FindElement(By.Id("email"));
        //    string validationMessage = cccdField.GetAttribute("validationMessage");

        //    string expected = $"Please include an '@' in the email address. '{email}' is missing an '@'.";
        //    Assert.AreEqual(expected, validationMessage);
        //    // 5. Kiểm tra form vẫn đang mở
        //    bool isModalStillOpen = driver.FindElement(By.Id("addCustomerModal"))
        //        .GetAttribute("class").Contains("show");
        //    Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ.");
        //    Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

        //    // 6. Kiểm tra không có khách hàng nào được thêm
        //    int countAfter = customer.CountAllCustomers();
        //    Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");

        //    // 7. Kiểm tra thông báo lỗi giao diện (nếu có)

        //    var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
        //        .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

        //    Assert.AreEqual(0, alerts.Count, "Không nên có alert vì trình duyệt đã chặn submit.");
        //    Console.WriteLine("Không có alert lỗi hiển thị (đúng do bị HTML5 chặn).");

        //    // 7. Đảm bảo khách hàng cũ không bị thay đổi
        //    var customerAfter = customer.GetCustomerDetails(cccd);
        //    Assert.IsNotNull(customerAfter, $"Không tìm thấy khách hàng [{cccd}] sau khi test.");

        //    // So sánh với dữ liệu gốc (giả định đã tồn tại từ trước)
        //    Assert.AreNotEqual(fullName, customerAfter.FullName, "Tên khách hàng bị ghi đè.");
        //    Assert.AreNotEqual(phone, customerAfter.Phone, "Số điện thoại bị ghi đè.");
        //    Assert.AreNotEqual(address, customerAfter.Address, "Địa chỉ bị ghi đè.");
        //    Console.WriteLine("Dữ liệu khách hàng không bị thay đổi -> PASS");
        //}
        //[TestMethod]
        //public void TestAddCustomer14()
        //{
        //    // 1. Thiết lập dữ liệu đầu vào
        //    string cccd = "12"; // Giả định đã tồn tại
        //    string fullName = "Trang";
        //    string phone = "12345678912333333"; 
        //    string email = "nguyen"; // cũng sai định dạng
        //    string address = "123 Nguyễn Trãi, Hà Nội";

        //    int countBefore = customer.CountAllCustomers();
        //    string expectedMessage = "Số điện thoại không hợp lệ.";
        //    // 2. Mở form thêm khách hàng
        //    customer.OpenAddCustomerSection();

        //    // 3. Thực hiện thêm khách hàng
        //    customer.AddCustomer(cccd, fullName, phone, email, address);

        //    var cccdField = driver.FindElement(By.Id("email"));
        //    string validationMessage = cccdField.GetAttribute("validationMessage");

        //    string expected = $"Please include an '@' in the email address. '{email}' is missing an '@'.";
        //    Assert.AreEqual(expected, validationMessage);
        //    // 5. Kiểm tra form vẫn đang mở
        //    bool isModalStillOpen = driver.FindElement(By.Id("addCustomerModal"))
        //        .GetAttribute("class").Contains("show");
        //    Assert.IsTrue(isModalStillOpen, "Form đã bị đóng dù dữ liệu không hợp lệ.");
        //    Console.WriteLine("Form vẫn hiển thị sau khi nhập dữ liệu không hợp lệ.");

        //    // 6. Kiểm tra không có khách hàng nào được thêm
        //    int countAfter = customer.CountAllCustomers();
        //    Assert.AreEqual(countBefore, countAfter, "Hệ thống vẫn thêm khách hàng dù dữ liệu không hợp lệ.");

        //    // 7. Kiểm tra thông báo lỗi giao diện (nếu có)

        //    var alerts = driver.FindElements(By.CssSelector("div.alert.alert-danger"))
        //        .Where(a => a.Displayed && !string.IsNullOrWhiteSpace(a.Text)).ToList();

        //    Assert.AreEqual(0, alerts.Count, "Không nên có alert vì trình duyệt đã chặn submit.");
        //    Console.WriteLine("Không có alert lỗi hiển thị (đúng do bị HTML5 chặn).");

        //    // 7. Đảm bảo khách hàng cũ không bị thay đổi
        //    var customerAfter = customer.GetCustomerDetails(cccd);
        //    Assert.IsNotNull(customerAfter, $"Không tìm thấy khách hàng [{cccd}] sau khi test.");

        //    // So sánh với dữ liệu gốc (giả định đã tồn tại từ trước)
        //    Assert.AreNotEqual(fullName, customerAfter.FullName, "Tên khách hàng bị ghi đè.");
        //    Assert.AreNotEqual(phone, customerAfter.Phone, "Số điện thoại bị ghi đè.");
        //    Assert.AreNotEqual(address, customerAfter.Address, "Địa chỉ bị ghi đè.");
        //    Console.WriteLine("Dữ liệu khách hàng không bị thay đổi -> PASS");
        //}

    }
}

