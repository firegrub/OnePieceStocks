using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnePieceStocks.Models;

namespace OnePieceStocks.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<Holding> Holdings { get; set; }
        public DbSet<TradeHistory> TradeHistories { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<RulePage> RulePages { get; set; }
        public DbSet<GameSettings> GameSettings { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Holding>()
                .HasIndex(h => new { h.PlayerId, h.CharacterId })
                .IsUnique();
        }
    }
}