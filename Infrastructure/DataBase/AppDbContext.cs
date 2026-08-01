using Domain.Entities;
using Infrastructure.DataBase.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<User> Users { get; set; }
	public DbSet<Trade> Trades { get; set; }
	public DbSet<TradeType> TradeTypes { get; set; }
	public DbSet<Instrument> Instruments { get; set; }
	public DbSet<UserInstrument> UserInstruments { get; set; }
	public DbSet<Strategy> Strategies { get; set; }
	public DbSet<UserStrategy> UserStrategies { get; set; }
	public DbSet<Note> Notes { get; set; }
	public DbSet<Reminder> Reminders { get; set; }
	public DbSet<UserStrategyInstrument> UserStrategyInstruments { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ConfigureDatabaseSchema();
		modelBuilder.SeedReferenceData();
		modelBuilder.ConfigureUtcDateTimeStorage();
	}
}
