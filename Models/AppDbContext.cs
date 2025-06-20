using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RentAutoWeb.Models
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int, IdentityUserClaim<int>, IdentityUserRole<int>, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public new DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<AnalyticsReport> AnalyticsReports { get; set; }
        public StripeSettings StripeSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MaintenanceRecord>().ToTable("MaintenanceRecords");

            // Если у MaintenanceRecord есть связи (например, с Car):
            modelBuilder.Entity<MaintenanceRecord>()
                .HasOne(m => m.Car)
                .WithMany(c => c.MaintenanceRecords)
                .HasForeignKey(m => m.CarId);
        }

    }
    
    

    
    
}
