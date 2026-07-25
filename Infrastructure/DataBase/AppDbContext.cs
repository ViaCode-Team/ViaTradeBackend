using Domain.Entities;
using Domain.Notes.Entities;
using Domain.Reminders.Entities;
using Domain.Strategies.Entities;
using Domain.TradeCodes.Entities;
using Domain.Users.Entities;
using Infrastructure.DataBase.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase;

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
		base.OnModelCreating(modelBuilder);

		modelBuilder.ConfigureDatabaseSchema();
		modelBuilder.SeedReferenceData();
		modelBuilder.ConfigureUtcDateTimeStorage();
	}
}
