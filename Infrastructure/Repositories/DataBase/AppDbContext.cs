using Domain.Notes.Entities;
using Domain.Reminders.Entities;
using Domain.Strategies.Entities;
using Domain.TradeCodes.Entities;
using Domain.Trades.Entities;
using Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<User> Users { get; set; }
	public DbSet<Trade> Trades { get; set; }
	public DbSet<TradeType> TradeTypes { get; set; }
	public DbSet<TradeCode> TradeCodes { get; set; }
	public DbSet<TradeStrategy> TradeStrategies { get; set; }
	public DbSet<UserTradeStrategy> UserTradeStrategies { get; set; }
	public DbSet<Note> Notes { get; set; }
	public DbSet<Reminder> Reminders { get; set; }
	public DbSet<UserStrategyTradeCode> UserStrategyTradeCodes { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<TradeCode>().HasIndex(x => x.ExchangeId).IsUnique();

		modelBuilder.Entity<TradeStrategy>().HasIndex(x => x.Name).IsUnique();

		modelBuilder.Entity<TradeType>().HasIndex(x => x.Name).IsUnique();

		modelBuilder.Entity<UserTradeCode>().HasIndex(x => new { x.UserId, x.TradeCodeId }).IsUnique();

		modelBuilder.Entity<UserTradeStrategy>().HasIndex(x => new { x.UserId, x.TradeStrategyId }).IsUnique();

		modelBuilder
			.Entity<UserStrategyTradeCode>()
			.HasIndex(x => new
			{
				x.UserId,
				x.TradeCodeId,
				x.StrategyId,
			})
			.IsUnique();

		modelBuilder.Entity<Reminder>().HasIndex(x => x.UserId);

		modelBuilder.Entity<Reminder>().ToTable("TradeReminds");

		modelBuilder.Entity<Note>(entity =>
		{
			entity.HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId).IsRequired();
			entity.HasOne(n => n.TradeCode).WithMany().HasForeignKey(n => n.TradeCodeId).IsRequired(false);
			entity.HasOne(n => n.TradeStrategy).WithMany().HasForeignKey(n => n.TradeStrategyId).IsRequired(false);

			entity.ToTable(t =>
				t.HasCheckConstraint(
					"CK_Note_ExclusiveTarget",
					"(`TradeCodeId` IS NOT NULL AND `TradeStrategyId` IS NULL) OR (`TradeCodeId` IS NULL AND `TradeStrategyId` IS NOT NULL)"
				)
			);

			entity.HasIndex(x => x.UserId);
		});

		modelBuilder.Entity<Trade>(entity =>
		{
			entity.HasIndex(x => x.UserId);
			entity.Property(t => t.Price).HasColumnType("decimal(18,2)");
			entity.Ignore(t => t.NetIncome);
		});

		modelBuilder
			.Entity<TradeStrategy>()
			.HasData(
				new
				{
					Id = 1,
					Name = "TrendFollowingStrategy",
					Description = "Basic trend-following strategy for an asset. Minimal risk, rare signals.",
					Accuracy = 81,
					SignalFrequency = "1-2 times a month",
					InvestmentHorizon = "1-3 weeks",
					LogicDesc = "Analysis of a long-term chart to confirm movement",
					UseDesc = "Follow the main trend, during low or medium volatility",
					LimitDesc = "Strategy exclusively for following the trend",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					IsActive = true,
				},
				new
				{
					Id = 2,
					Name = "Test",
					Description = "Test strategy. 100000% profit per nanosecond",
					Accuracy = 99,
					SignalFrequency = "3 times a month",
					InvestmentHorizon = "up to 1 week",
					LogicDesc = "Very clear",
					UseDesc = "Use it however you like",
					LimitDesc = "SuperStart",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					IsActive = true,
				}
			);

		modelBuilder
			.Entity<TradeCode>()
			.HasData(
				new
				{
					Id = 1,
					ExchangeId = "GAZP",
					Description = "Gazprom",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				},
				new
				{
					Id = 2,
					ExchangeId = "GMKN",
					Description = "Nornickel",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				}
			);

		modelBuilder
			.Entity<TradeType>()
			.HasData(
				new
				{
					Id = 1,
					Name = "Stock",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				},
				new
				{
					Id = 2,
					Name = "Futures",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				}
			);
	}
}
