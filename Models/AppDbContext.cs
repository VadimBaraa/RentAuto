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
        public DbSet<UserAdditionalInfo> UserAdditionalInfos { get; set; }
    }
}
