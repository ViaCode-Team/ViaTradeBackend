using Microsoft.EntityFrameworkCore;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Infrastructure.DataBase.Configuration;

internal static class DomainModelBuilderExtensions
{
	public static void ConfigureDomainModel(this ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Instrument>().HasIndex(x => x.Symbol).IsUnique();

		modelBuilder.Entity<Strategy>(entity =>
		{
			entity.HasIndex(x => x.Name).IsUnique();
			entity.HasQueryFilter(strategy => strategy.IsActive);
		});

		modelBuilder.Entity<TradeType>().HasIndex(x => x.Name).IsUnique();

		modelBuilder.Entity<User>().Property(x => x.Login).HasMaxLength(255);

		modelBuilder.Entity<User>().HasIndex(x => x.Login).IsUnique();

		modelBuilder.Entity<User>().HasIndex(x => x.TelegramId).IsUnique();

		modelBuilder.Entity<UserInstrument>().HasIndex(x => new { x.UserId, x.InstrumentId }).IsUnique();

		modelBuilder.Entity<UserStrategy>(entity =>
		{
			entity.HasIndex(x => new { x.UserId, x.StrategyId }).IsUnique();
			entity.HasQueryFilter(userStrategy => userStrategy.Strategy!.IsActive);
		});

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
			entity.HasQueryFilter(userStrategyInstrument => userStrategyInstrument.Strategy!.IsActive);
			entity.HasOne(x => x.Strategy).WithMany().HasForeignKey(x => x.StrategyId).IsRequired();
		});

		modelBuilder.Entity<Reminder>(entity =>
		{
			entity.HasIndex(x => x.UserId);
			entity.HasIndex(x => x.DeliveredAt);
			entity.HasIndex(x => new
			{
				x.UserId,
				x.DeliveredAt,
				x.RemindAt,
			});
			entity.HasIndex(x => new { x.PublishedAt, x.RemindAt });
		});

		modelBuilder.Entity<Note>(entity =>
		{
			entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).IsRequired();
			entity.HasOne(x => x.Instrument).WithMany().HasForeignKey(x => x.InstrumentId).IsRequired(false);
			entity.HasOne(x => x.Strategy).WithMany().HasForeignKey(x => x.StrategyId).IsRequired(false);
			entity.HasIndex(x => new { x.UserId, x.InstrumentId }).IsUnique();
			entity.HasIndex(x => new { x.UserId, x.StrategyId }).IsUnique();
			entity.HasIndex(x => x.UserId);
		});

		modelBuilder.Entity<Trade>(entity =>
		{
			entity.HasIndex(x => x.UserId);
			entity.HasIndex(x => new { x.UserId, x.ClosedAt });
		});
	}
}
