using System.Globalization;
using System.Text.RegularExpressions;
using Application.Trades.Interfaces;
using Domain.Models.Trade;

namespace Infrastructure.Utils;

public class TradeDataBuilder : ITradeDataBuilder
{
	// Pattern: {Symbol}_{TimeFrame}_{StartDate}_{EndDate}.csv
	// TimeFrame can contain numbers (e.g., HOUR_1), so use non-greedy capture .*?

	private static readonly Regex FileNameRegex = new(
		@"^(?<Symbol>[A-Z0-9]+)_(?<TimeFrame>.*?)_(?<Start>\d{4}-\d{2}-\d{2})_(?<End>\d{4}-\d{2}-\d{2})_?\.csv$",
		RegexOptions.Compiled | RegexOptions.IgnoreCase
	);

	public IEnumerable<InstrumentFile> BuildInstrumentFiles(IEnumerable<string>? fileNames)
	{
		if (fileNames == null)
		{
			yield break;
		}

		foreach (var fileName in fileNames)
		{
			// Process only valid file names, skip invalid silently or log if needed
			var hasInstrumentFile = TryParseFileName(fileName, out var response);

			if (hasInstrumentFile)
			{
				yield return response;
			}
		}
	}

	private static bool TryParseFileName(string fileName, out InstrumentFile result)
	{
		result = null!;

		// Extract just the file name without path
		var nameOnly = Path.GetFileName(fileName);
		var match = FileNameRegex.Match(nameOnly);

		if (!match.Success)
			return false;

		// Try to parse dates with invariant culture to avoid locale issues
		var hasStartDate = DateTime.TryParseExact(
			match.Groups["Start"].Value,
			"yyyy-MM-dd",
			CultureInfo.InvariantCulture,
			DateTimeStyles.None,
			out var startDate
		);

		if (!hasStartDate)
			return false;

		var hasEndDate = DateTime.TryParseExact(
			match.Groups["End"].Value,
			"yyyy-MM-dd",
			CultureInfo.InvariantCulture,
			DateTimeStyles.None,
			out var endDate
		);

		if (!hasEndDate)
			return false;

		result = new InstrumentFile
		{
			Symbol = match.Groups["Symbol"].Value.ToUpperInvariant(),
			TimeFrame = match.Groups["TimeFrame"].Value,
			StartDate = startDate,
			EndDate = endDate,
		};

		return true;
	}
}
