using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Trades.Models;

public sealed class ProfitChartFilter : IValidatableObject
{
	public DateOnly? StartDate { get; set; }

	public DateOnly? EndDate { get; set; }

	[Required]
	public ProfitChartGranularity Granularity { get; set; } = ProfitChartGranularity.Day;

	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (StartDate.HasValue && EndDate.HasValue && StartDate.Value > EndDate.Value)
			yield return new ValidationResult(
				"startDate must be less than or equal to endDate.",
				[nameof(StartDate), nameof(EndDate)]
			);
	}
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProfitChartGranularity
{
	[JsonStringEnumMemberName("day")]
	Day,

	[JsonStringEnumMemberName("week")]
	Week,

	[JsonStringEnumMemberName("month")]
	Month,
}

public record ProfitChartAggregateRow(
	int? Year,
	int? Month,
	int? Day,
	double? WeekIndex,
	double NetIncome,
	double BuyNetIncome,
	double SellNetIncome
);

public record ProfitChartBucketDto(DateOnly Date, double NetIncome, double BuyNetIncome, double SellNetIncome);

public record TradeDateRangeDto(DateOnly? MinDate, DateOnly? MaxDate);
