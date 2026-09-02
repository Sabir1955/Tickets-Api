
using Authentication.Models;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}
