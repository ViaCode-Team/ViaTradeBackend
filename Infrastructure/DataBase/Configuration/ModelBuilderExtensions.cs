using Domain.Entities;
using Domain.Notes.Entities;
using Domain.Reminders.Entities;
using Domain.Strategies.Entities;
using Domain.TradeCodes.Entities;
using Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Configuration;

internal static class ModelBuilderExtensions
{
	public static void ConfigureDatabaseSchema(this ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<TradeCode>().HasIndex(x => x.ExchangeId).IsUnique();

		modelBuilder.Entity<TradeStrategy>().HasIndex(x => x.Name).IsUnique();

		modelBuilder.Entity<TradeType>().HasIndex(x => x.Name).IsUnique();

		modelBuilder.Entity<User>().Property(x => x.Login).HasMaxLength(255);

		modelBuilder.Entity<User>().HasIndex(x => x.Login).IsUnique();

		modelBuilder.Entity<UserTradeCode>().HasIndex(x => new { x.UserId, x.TradeCodeId }).IsUnique();

		modelBuilder.Entity<UserTradeStrategy>().HasIndex(x => new { x.UserId, x.TradeStrategyId }).IsUnique();

		modelBuilder.Entity<UserStrategyTradeCode>(entity =>
		{
			entity
				.HasIndex(x => new
				{
					x.UserId,
					x.TradeCodeId,
					x.StrategyId,
				})
				.IsUnique();
			entity.HasIndex(x => new { x.UserId, x.StrategyId, x.TradeCodeId });
			entity.HasOne(x => x.TradeStrategy).WithMany().HasForeignKey(x => x.StrategyId).IsRequired();
		});

		modelBuilder.Entity<Reminder>(entity =>
		{
			entity.HasIndex(x => x.UserId);
			entity.ToTable("TradeReminds");
		});

		modelBuilder.Entity<Note>(entity =>
		{
			entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).IsRequired();
			entity.HasOne(x => x.TradeCode).WithMany().HasForeignKey(x => x.TradeCodeId).IsRequired(false);
			entity.HasOne(x => x.TradeStrategy).WithMany().HasForeignKey(x => x.TradeStrategyId).IsRequired(false);
			entity.HasIndex(x => new { x.UserId, x.TradeCodeId }).IsUnique();
			entity.HasIndex(x => new { x.UserId, x.TradeStrategyId }).IsUnique();
			entity.HasIndex(x => x.UserId);
			entity.ToTable(table =>
				table.HasCheckConstraint(
					"CK_Note_ExclusiveTarget",
					"(`TradeCodeId` IS NOT NULL AND `TradeStrategyId` IS NULL) OR (`TradeCodeId` IS NULL AND `TradeStrategyId` IS NOT NULL)"
				)
			);
		});

		modelBuilder.Entity<Trade>(entity =>
		{
			entity.HasIndex(x => x.UserId);
			entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
			entity.ToTable(table => table.HasCheckConstraint("CK_Trade_PositiveCount", "`Count` > 0"));
		});
	}
}
