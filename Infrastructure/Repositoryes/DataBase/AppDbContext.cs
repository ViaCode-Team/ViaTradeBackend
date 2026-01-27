using System;
using Domain.Entities.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositoryes.DataBase
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<TradeType> TradeTypes { get; set; }
        public DbSet<TradeCode> TradeCodes { get; set; }
        public DbSet<TradeStrategy> TradeStrategies { get; set; }
        public DbSet<UserTradeCode> UserTradeCodes { get; set; }
        public DbSet<UserTradeStrategy> UserTradeStrategies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TradeCode>()
                .HasIndex(x => x.ExchangeId)
                .IsUnique();

            modelBuilder.Entity<TradeStrategy>()
                .HasIndex(x => x.Name)
                .IsUnique();

            modelBuilder.Entity<TradeType>()
                .HasIndex(x => x.Name)
                .IsUnique();

            modelBuilder.Entity<UserTradeCode>()
                .HasIndex(x => new { x.UserId, x.TradeCodeId })
                .IsUnique();

            modelBuilder.Entity<UserTradeStrategy>()
                .HasIndex(x => new { x.UserId, x.TradeStrategyId })
                .IsUnique();
        }
    }
}
