using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAutoWeb.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

public class RentalController : Controller
{
    private readonly AppDbContext _context;

    public RentalController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Rental(int carId)
    {
        var car = await _context.Cars.FindAsync(carId);
        Console.WriteLine($"Rental request carId={carId}, found={(car != null ? "yes" : "no")}, IsAvailable={(car?.IsAvailable.ToString() ?? "N/A")}");
        if (car == null || !car.IsAvailable)
            return NotFound();

        return PartialView("~/Views/Home/Rental.cshtml", car);
    }

   [HttpPost]
    public async Task<IActionResult> ConfirmRental(int CarId, DateTime StartDate, DateTime EndDate)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized("Пользователь не авторизован.");
        }

        var car = await _context.Cars.FindAsync(CarId);
        if (car == null)
            return BadRequest("Машина не найдена.");

        var duration = EndDate - StartDate;
        if (duration.TotalHours < 2)
            return BadRequest("Минимальный срок аренды — 2 часа.");

        if (duration.TotalMinutes <= 0)
            return BadRequest("Некорректные даты аренды");

        // Завершаем активную аренду, если такая есть
        var existingRental = await _context.Rentals
        .Include(r => r.Car) // <- вот это важно
        .Where(r => r.CarId == CarId &&
                    (r.Status == RentalStatus.PendingPayment || r.Status == RentalStatus.Active))
        .FirstOrDefaultAsync();


        if (existingRental != null)
        {
            existingRental.Status = RentalStatus.Cancelled; // Или Completed, если по времени
            _context.Rentals.Update(existingRental);
        }

        // Создаем новую аренду
        var rental = new Rental
        {
            CarId = CarId,
            UserId = userId,
            StartDate = StartDate,
            EndDate = EndDate,
            Status = RentalStatus.PendingPayment,
            TotalPrice = (decimal)duration.TotalHours * (car.Price / 24)
        };

        _context.Rentals.Add(rental);

        car.IsAvailable = false;
        await _context.SaveChangesAsync();

        return RedirectToAction("PersonalCabinet", "Home", new { id = rental.Id });
    }




    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized("Пользователь не авторизован.");
        }

        var rentals = await _context.Rentals
        .Include(r => r.Car)
        .Where(r => r.UserId == userId)
        .OrderByDescending(r => r.StartDate)
        .ToListAsync();


        if (rentals == null)
            return NotFound();

        return View(rentals);
    }

    // Изменение статуса существующей аренды
    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int rentalId, string newStatus)
    {
        var rental = await _context.Rentals.FindAsync(rentalId);
        if (rental == null)
            return NotFound("Аренда не найдена");

        if (!Enum.TryParse<RentalStatus>(newStatus, true, out var parsedStatus))
            return BadRequest("Недопустимый статус");

        rental.Status = parsedStatus;
        await _context.SaveChangesAsync();

        return Ok("Статус обновлен");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> EndRental(int rentalId)
    {
        var rental = await _context.Rentals
            .Include(r => r.Car)
            .FirstOrDefaultAsync(r => r.Id == rentalId);

        if (rental == null)
            return NotFound();

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            return Unauthorized();

        var isAdmin = User.IsInRole("Admin");

        // Обычный пользователь не может завершить аренду до конца срока
        if (!isAdmin && rental.EndDate > DateTime.Now)
        {
            return Forbid("Вы не можете закончить аренду до окончания времени.");
        }

        rental.Status = RentalStatus.Cancelled;
        rental.Car.IsAvailable = true;

        _context.Update(rental);
        await _context.SaveChangesAsync();

        return RedirectToAction("PersonalCabinet", "Home");
    }


    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult MarkAsPaid(int rentalId)
    {
        var rental = _context.Rentals.FirstOrDefault(r => r.Id == rentalId);

        if (rental == null)
            return NotFound();

        if (rental.Status == RentalStatus.PendingPayment)
        {
            rental.Status = RentalStatus.Active; // Или Completed, если хочешь завершить
            _context.SaveChanges();
        }

        return RedirectToAction("Admin", "Home");
    }
}
