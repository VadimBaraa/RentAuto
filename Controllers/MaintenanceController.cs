using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAutoWeb.Models;
using Microsoft.AspNetCore.Identity;

[Route("api/[controller]")]
[ApiController]
public class MaintenanceController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<User> _userManager;

    public MaintenanceController(AppDbContext db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    
    [HttpGet("by-car/{carId}")]
    public async Task<IActionResult> GetByCar(int carId)
    {
        var records = await _db.MaintenanceRecords
            .Where(r => r.CarId == carId)
            .OrderByDescending(r => r.MaintenanceDate)
            .ToListAsync();

        return Ok(records);
    }

  
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] MaintenanceRecord model)
    {
        // Получаем логин из заголовка
        var userLogin = Request.Headers["X-User-Login"].ToString();

        // Ищем пользователя по UserName
        var currentUser = await _userManager.FindByNameAsync(userLogin);

        if (currentUser == null)
            return Forbid("Пользователь не найден");

        // Проверяем роль (админ или менеджер)
        bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
        bool isManager = await _userManager.IsInRoleAsync(currentUser, "Manager");

        if (!isAdmin && !isManager)
        {
            return Forbid("Только администратор или менеджер может добавлять ТО.");
        }

        model.MaintenanceDate = DateTime.UtcNow;
        _db.MaintenanceRecords.Add(model);
        await _db.SaveChangesAsync();

        return Ok(model);
    }

}
