using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.DataBase.Configuration;

internal static class UtcDateTimeModelBuilderExtensions
{
	private static readonly ValueConverter<DateTime, DateTime> UtcDateTimeConverter = new(
		value => NormalizeUtc(value),
		value => DateTime.SpecifyKind(value, DateTimeKind.Utc)
	);

	private static readonly ValueConverter<DateTime?, DateTime?> NullableUtcDateTimeConverter = new(
		value => NormalizeUtc(value),
		value => SetUtcKind(value)
	);

	public static void ConfigureUtcDateTimeStorage(this ModelBuilder modelBuilder)
	{
		foreach (var entityType in modelBuilder.Model.GetEntityTypes())
		{
			foreach (var property in entityType.GetProperties())
			{
				if (property.ClrType == typeof(DateTime))
					property.SetValueConverter(UtcDateTimeConverter);
				else if (property.ClrType == typeof(DateTime?))
					property.SetValueConverter(NullableUtcDateTimeConverter);
			}
		}
	}

	private static DateTime NormalizeUtc(DateTime value)
	{
		if (value.Kind == DateTimeKind.Utc)
			return value;

		return value.ToUniversalTime();
	}

	private static DateTime? NormalizeUtc(DateTime? value)
	{
		if (!value.HasValue)
			return null;

		return NormalizeUtc(value.Value);
	}

	private static DateTime? SetUtcKind(DateTime? value)
	{
		if (!value.HasValue)
			return null;

		return DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
	}
}
