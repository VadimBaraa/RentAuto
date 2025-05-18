using Microsoft.EntityFrameworkCore;
using RentAutoWeb.Models;

namespace RentAutoWeb.Services
{
    public class RentalExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Проверять каждые 5 минут

        public RentalExpirationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var now = DateTime.Now;

                    var expiredRentals = await dbContext.Rentals
                        .Include(r => r.Car)
                        .Where(r =>
                            (r.Status == RentalStatus.Active || r.Status == RentalStatus.PendingPayment) &&
                            r.EndDate < now)
                        .ToListAsync(stoppingToken);

                    foreach (var rental in expiredRentals)
                    {
                        rental.Status = RentalStatus.Cancelled;

                        if (rental.Car != null)
                        {
                            rental.Car.IsAvailable = true;
                        }
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}

