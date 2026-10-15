using Microsoft.EntityFrameworkCore;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Infrastructure.DataBase.Configuration;

internal static class DomainModelBuilderExtensions
{
	public static void ConfigureDomainModel(this ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Instrument>().HasIndex(x => x.Ticker).IsUnique();

		modelBuilder.Entity<Strategy>(entity =>
		{
			entity.HasIndex(x => x.Name).IsUnique();
			entity.HasQueryFilter(strategy => strategy.IsActive);
		});

		modelBuilder.Entity<TradeType>().HasIndex(x => x.Name).IsUnique();

		modelBuilder.Entity<User>(entity =>
		{
			entity.Property(x => x.Login).HasMaxLength(255);
			entity.Property(x => x.NormalizedLogin).HasMaxLength(255).UseCollation("utf8mb4_bin");
			entity.Property(x => x.Email).HasMaxLength(254);
			entity.Property(x => x.NormalizedEmail).HasMaxLength(254).UseCollation("utf8mb4_bin");
			entity.Property(x => x.PendingEmail).HasMaxLength(254);
			entity.Property(x => x.NormalizedPendingEmail).HasMaxLength(254).UseCollation("utf8mb4_bin");
			entity.Property(x => x.PreviousEmail).HasMaxLength(254);
			entity.Property(x => x.NormalizedPreviousEmail).HasMaxLength(254).UseCollation("utf8mb4_bin");
			entity.HasIndex(x => x.Login).IsUnique();
			entity.HasIndex(x => x.NormalizedLogin).IsUnique();
			entity.HasIndex(x => x.NormalizedEmail).IsUnique();
			entity.HasIndex(x => x.NormalizedPendingEmail);
			entity.HasIndex(x => x.NormalizedPreviousEmail);
			entity.HasIndex(x => new { x.EmailConfirmedAt, x.RegisteredAt });
			entity.HasIndex(x => x.TelegramId).IsUnique();
		});

		modelBuilder.Entity<UserInstrument>().HasIndex(x => new { x.UserId, x.InstrumentId }).IsUnique();

		modelBuilder.Entity<UserStrategy>(entity =>
		{
			entity.HasIndex(x => new { x.UserId, x.StrategyId }).IsUnique();
			entity.HasQueryFilter(userStrategy => userStrategy.Strategy!.IsActive);
		});

		modelBuilder.Entity<UserStrategyInstrument>(entity =>
		{
			entity.HasIndex(x => new { x.StrategyId, x.InstrumentId });

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
