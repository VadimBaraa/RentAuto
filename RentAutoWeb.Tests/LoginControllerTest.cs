using Moq;
using Microsoft.AspNetCore.Mvc;
using RentAutoWeb.Controllers;
using RentAutoWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace RentAutoWeb.Tests
{
    public class LoginControllerTests
    {
        [Fact]
        public async Task Login_ReturnsSuccess_WhenCredentialsAreValid()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new AppDbContext(options))
            {
                var user = new User
                {
                    UserName = "testuser",
                    Email = "test@test.com",
                    FirstName = "Вадим",
                    LastName = "Иванов",
                    MiddleName = "Сергеевич"
                };
                context.Users.Add(user);
                context.SaveChanges();

                var userManager = GetMockUserManager();
                var signInManager = GetMockSignInManager(userManager);

                userManager.Setup(x => x.FindByEmailAsync("test@test.com")).ReturnsAsync(user);
                signInManager.Setup(s => s.PasswordSignInAsync(
                    user.UserName,
                    "password123",
                    true,
                    false
                )).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

                var logger = new Mock<ILogger<LoginController>>();
                var controller = new LoginController(context, signInManager.Object, logger.Object);

                // Добавляем HttpContext и сессию
                var httpContext = new DefaultHttpContext();
                httpContext.Session = new TestSession();
                controller.ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                };

                var loginViewModel = new LoginViewModel
                {
                    Email = "test@test.com",
                    Password = "password123"
                };

                var result = await controller.Login(loginViewModel);

                var jsonResult = Assert.IsType<JsonResult>(result);
                dynamic data = jsonResult.Value;
                Assert.Equal("Вход выполнен успешно!", (string)data.message);
                Assert.Equal("Вадим", (string)data.userName);
            }

        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            var userManager = GetMockUserManager();
            var signInManager = GetMockSignInManager(userManager);
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "InvalidModelTestDb")
                .Options;

            using var context = new AppDbContext(options);

            var logger = new Mock<ILogger<LoginController>>();
            var controller = new LoginController(context, signInManager.Object, logger.Object);

            controller.ModelState.AddModelError("Email", "Email is required");

            var model = new LoginViewModel(); // пустая модель

            var result = await controller.Login(model);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<LoginController.ErrorResponse>(badRequest.Value);
            Assert.True(response.Errors.ContainsKey("Email"));
        }

        private static Mock<UserManager<User>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                store.Object,
                null, null, null, null, null, null, null, null
            );
        }

        private static Mock<SignInManager<User>> GetMockSignInManager(Mock<UserManager<User>> userManager)
        {
            return new Mock<SignInManager<User>>(
                userManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<User>>().Object,
                null, null, null, null
            );
        }

        // Вспомогательная сессия
        private class TestSession : ISession
        {
            private Dictionary<string, byte[]> _sessionStorage = new();

            public IEnumerable<string> Keys => _sessionStorage.Keys;
            public bool IsAvailable => true;
            public string Id => Guid.NewGuid().ToString();
            public void Clear() => _sessionStorage.Clear();
            public void Remove(string key) => _sessionStorage.Remove(key);
            public void Set(string key, byte[] value) => _sessionStorage[key] = value;
            public bool TryGetValue(string key, out byte[] value) => _sessionStorage.TryGetValue(key, out value);
            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        }
    }
}
