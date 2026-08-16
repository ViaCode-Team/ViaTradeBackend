using Microsoft.EntityFrameworkCore;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Infrastructure.DataBase.Configuration.MySql;

internal static class MySqlModelBuilderExtensions
{
	private const string NetIncomeComputedColumnSql = """
		CASE
			WHEN `ClosePrice` IS NULL OR `OpenPrice` = 0 OR `Signal` = 0 THEN NULL
			ELSE ROUND((`ClosePrice` - `OpenPrice`) / `OpenPrice` * 100 * `Signal`, 2)
		END
		""";

	public static void ConfigureMySqlModel(this ModelBuilder modelBuilder)
	{
		modelBuilder
			.Entity<Note>()
			.ToTable(table =>
				table.HasCheckConstraint(
					"CK_Notes_ExclusiveTarget",
					"(`InstrumentId` IS NOT NULL AND `StrategyId` IS NULL) OR (`InstrumentId` IS NULL AND `StrategyId` IS NOT NULL)"
				)
			);

		modelBuilder.Entity<Trade>(entity =>
		{
			entity
				.Property(x => x.NetIncome)
				.HasColumnType("double")
				.HasComputedColumnSql(NetIncomeComputedColumnSql, stored: true);
			entity.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
			entity.ToTable(table => table.HasCheckConstraint("CK_Trades_PositiveQuantity", "`Quantity` > 0"));
		});
	}
}
