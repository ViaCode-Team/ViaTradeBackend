using Domain.Entities.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<TradeType> TradeTypes { get; set; }
        public DbSet<TradeCode> TradeCodes { get; set; }
        public DbSet<TradeStrategy> TradeStrategies { get; set; }
        public DbSet<UserTradeStrategy> UserTradeStrategies { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<TradeRemind> TradeReminds { get; set; }
        public DbSet<UserStrategyTradeCode> UserStrategyTradeCodes { get; set; }

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

            modelBuilder.Entity<UserStrategyTradeCode>()
                .HasIndex(x => new { x.UserId, x.TradeCodeId, x.StrategyId })
                .IsUnique();

            modelBuilder.Entity<TradeRemind>()
                .HasIndex(x => x.Id)
                .IsUnique();

            modelBuilder.Entity<Note>(entity =>
            {
                entity.HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId).IsRequired();
                entity.HasOne(n => n.TradeCode).WithMany().HasForeignKey(n => n.TradeCodeId).IsRequired(false);
                entity.HasOne(n => n.TradeStrategy).WithMany().HasForeignKey(n => n.TradeStrategyId).IsRequired(false);

                entity.ToTable(t => t.HasCheckConstraint("CK_Note_ExclusiveTarget",
                  "(`TradeCodeId` IS NOT NULL AND `TradeStrategyId` IS NULL) OR (`TradeCodeId` IS NULL AND `TradeStrategyId` IS NOT NULL)"));
            });

            // Base Data
            modelBuilder.Entity<TradeStrategy>().HasData(
                new TradeStrategy
                {
                    Id = 1,
                    Name = "TrendFollowingStrategy",
                    Description = "Базовая стратегия следования биржевому тренду инструмента. Минамальный риск, редкие сигналы.",
                    Accuracy = 81,
                    SignalFrequency = "1-2 раза в месяц",
                    InvestmentHorizon = "1-3 недели",
                    LogicDesc = "Анализ длительного времение гшрафика для подтвержеденгия движдениея",
                    UseDesc = "Следовать основному тренду, при низкой или средней валотильности",
                    LimitDesc = "Стратегия исключительно для слелования тренду"
                },
                new TradeStrategy
                {
                    Id = 2,
                    Name = "Test",
                    Description = "Тестовая стратегия. 100000% прибыли в наносекунду",
                    Accuracy = 99,
                    SignalFrequency = "3 раза в месяц",
                    InvestmentHorizon = "до 1 недели",
                    LogicDesc = "Ващё чётко",
                    UseDesc = "Как по кайфу так и используй",
                    LimitDesc = "СуперСтарта"
                }
            );

            modelBuilder.Entity<TradeCode>().HasData(
                new TradeCode { Id = 1, ExchangeId = "GAZP", Description = "Газпром" },
                new TradeCode { Id = 2, ExchangeId = "GMKN", Description = "Норникель" }
            );

            modelBuilder.Entity<TradeType>().HasData(
                new TradeType { Id = 1, Name = "Акция" },
                new TradeType { Id = 2, Name = "Фьючерс" }
            );

        }
    }
}
