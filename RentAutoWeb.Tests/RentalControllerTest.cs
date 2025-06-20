using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using RentAutoWeb.Models;  // Подставь свой namespace
using System;
using System.Collections.Generic;
using System.Linq;

public class RentalControllerTests
{
    [Fact]
    public async Task ConfirmRental_CreatesRentalAndRedirects_WhenDataIsValid()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDB")
            .Options;

        using var context = new AppDbContext(options);

        // Добавляем машину в контекст
        var car = new Car
        {
            Id = 1,
            Price = 2400, // допустим 2400 за сутки
            IsAvailable = true
        };
        context.Cars.Add(car);
        await context.SaveChangesAsync();

        var controller = new RentalController(context);

        // Создаем fake пользователя с Id = 1
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "mock"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var startDate = DateTime.Now.AddHours(3);
        var endDate = startDate.AddHours(5); // аренда 5 часов > 2 часа

        // Act
        var result = await controller.ConfirmRental(
            CarId: 1,
            StartDate: startDate,
            EndDate: endDate);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("PersonalCabinet", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);

        // Проверяем, что создана запись аренды
        var rental = context.Rentals.FirstOrDefault(r => r.CarId == 1 && r.UserId == 1);
        Assert.NotNull(rental);
        Assert.Equal(RentalStatus.PendingPayment, rental.Status);

        // Проверяем, что машина стала недоступна
        var updatedCar = await context.Cars.FindAsync(1);
        Assert.False(updatedCar.IsAvailable);

        // Проверяем расчет цены: (5 часов * 2400/24) = 500
        Assert.Equal(5 * (2400m / 24m), rental.TotalPrice);
    }
}
