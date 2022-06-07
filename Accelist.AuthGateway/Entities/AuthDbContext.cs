using Microsoft.EntityFrameworkCore;

namespace Accelist.AuthGateway.Entities
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> dbContextOptions) : base(dbContextOptions)
        {
        }

        public DbSet<LoginChallenge> LoginChallenge => Set<LoginChallenge>();

        public DbSet<LoginClaims> LoginClaims => Set<LoginClaims>();
    }
}
