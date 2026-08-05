using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Configuration;

internal static class ModelBuilderExtensions
{
	public static void ConfigureDatabaseSchema(this ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Instrument>().HasIndex(x => x.Symbol).IsUnique();

		modelBuilder.Entity<Strategy>().HasIndex(x => x.Name).IsUnique();

		modelBuilder.Entity<TradeType>().HasIndex(x => x.Name).IsUnique();

		modelBuilder.Entity<User>().Property(x => x.Login).HasMaxLength(255);

		modelBuilder.Entity<User>().HasIndex(x => x.Login).IsUnique();

		modelBuilder.Entity<User>().HasIndex(x => x.TelegramId).IsUnique();

		modelBuilder.Entity<UserInstrument>().HasIndex(x => new { x.UserId, x.InstrumentId }).IsUnique();

		modelBuilder.Entity<UserStrategy>().HasIndex(x => new { x.UserId, x.StrategyId }).IsUnique();

		modelBuilder.Entity<UserStrategyInstrument>(entity =>
		{
			entity
				.HasIndex(x => new
				{
					x.UserId,
					x.InstrumentId,
					x.StrategyId,
				})
				.IsUnique();
			entity.HasIndex(x => new
			{
				x.UserId,
				x.StrategyId,
				x.InstrumentId,
			});
			entity.HasOne(x => x.Strategy).WithMany().HasForeignKey(x => x.StrategyId).IsRequired();
		});

		modelBuilder.Entity<Reminder>(entity =>
		{
			entity.HasIndex(x => x.UserId);
		});

		modelBuilder.Entity<Note>(entity =>
		{
			entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).IsRequired();
			entity.HasOne(x => x.Instrument).WithMany().HasForeignKey(x => x.InstrumentId).IsRequired(false);
			entity.HasOne(x => x.Strategy).WithMany().HasForeignKey(x => x.StrategyId).IsRequired(false);
			entity.HasIndex(x => new { x.UserId, x.InstrumentId }).IsUnique();
			entity.HasIndex(x => new { x.UserId, x.StrategyId }).IsUnique();
			entity.HasIndex(x => x.UserId);
			entity.ToTable(table =>
				table.HasCheckConstraint(
					"CK_Notes_ExclusiveTarget",
					"(`InstrumentId` IS NOT NULL AND `StrategyId` IS NULL) OR (`InstrumentId` IS NULL AND `StrategyId` IS NOT NULL)"
				)
			);
		});

		modelBuilder.Entity<Trade>(entity =>
		{
			entity.HasIndex(x => x.UserId);
			entity.HasIndex(x => new { x.UserId, x.ClosedAt });
			entity.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
			entity.ToTable(table => table.HasCheckConstraint("CK_Trades_PositiveQuantity", "`Quantity` > 0"));
		});
	}
}
