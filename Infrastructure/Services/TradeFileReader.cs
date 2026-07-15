using Application.Interfaces.Utils;
using Domain.Entities.CSV;
using Domain.Models.ConfigOptions;
using Domain.Models.TradeLogic;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Infrastructure.Services;

public class TradeFileReader : IFileReader
{
	private readonly Dictionary<TradeDataType, string> _paths;
	private readonly ITradeDataBuilder _tradeDataBuilder;

	public TradeFileReader(IOptions<AnalyzerDataOption> options, ITradeDataBuilder tradeDataBuilder)
	{
		var basePath = options.Value.SourcePath;

		_paths = new Dictionary<TradeDataType, string>
		{
			[TradeDataType.Futures] = Path.Combine(basePath, options.Value.FuturesDataDirectoryName),
			[TradeDataType.Stocks] = Path.Combine(basePath, options.Value.StocksDataDirectoryName),
			[TradeDataType.Strategy] = Path.Combine(basePath, options.Value.StrategyResultDirectoryName),
			[TradeDataType.Screener] = Path.Combine(basePath, options.Value.ScrennerResultDirectoryName)
		};

		foreach (var key in _paths.Keys.ToList())
		{
			_paths[key] = Path.GetFullPath(_paths[key]);
		}

		_tradeDataBuilder = tradeDataBuilder;
	}

	public IEnumerable<TradeCodeFile> GetTradeCodes(TradeDataType dataType, IEnumerable<string>? filterCodes = null)
	{
		var directory = GetPath(dataType);
		if (!Directory.Exists(directory))
			yield break;

		var filterSet = filterCodes?
			.Where(c => !string.IsNullOrWhiteSpace(c))
			.Select(c => c.ToUpperInvariant())
			.ToHashSet();

		var hasFilter = filterSet != null && filterSet.Count > 0;

		// Collect matching file names first
		var matchingFiles = Directory.EnumerateFiles(directory, "*.csv")
			.Select(Path.GetFileName)
			.Where(fileName =>
			{
				var code = ExtractTradeCode(fileName);
				if (code == null) return false;
				return !hasFilter || filterSet.Contains(code);
			});

		// Use builder to parse file names into response objects
		foreach (var response in _tradeDataBuilder.BuildFileTradeResonse(matchingFiles))
		{
			yield return response;
		}
	}

	public IEnumerable<(string TradeCode, T Item)> ReadDataByCodes<T>(
		TradeDataType dataType,
		IEnumerable<string> tradeCodes,
		DateTime? startDate = null,
		DateTime? endDate = null) where T : class
	{
		foreach (var code in tradeCodes)
		{
			var filePath = FindFilePathByCode(dataType, code);
			if (filePath == null) continue;

			foreach (var item in ReadFile<T>(filePath, startDate, endDate))
			{
				// Return code context with each item
				yield return (code, item);
			}
		}
	}

	public IEnumerable<(string TradeCode, string StrategyName, T Item)> ReadDataByCodesWithStrategy<T>(
		TradeDataType dataType,
		IEnumerable<string> tradeCodes,
		DateTime? startDate = null,
		DateTime? endDate = null) where T : class
	{
		var directory = GetPath(dataType);
		if (!Directory.Exists(directory)) yield break;

		var tradeCodesSet = tradeCodes
			.Where(c => !string.IsNullOrWhiteSpace(c))
			.Select(c => c.ToUpperInvariant())
			.ToHashSet();

		var matchingFiles = Directory.EnumerateFiles(directory, "*.csv")
			.Select(filePath => new
			{
				Path = filePath,
				Code = ExtractTradeCode(Path.GetFileName(filePath))
			})
			.Where(x => x.Code != null && tradeCodesSet.Contains(x.Code));

		foreach (var file in matchingFiles)
		{
			var strategyName = ExtractStrategyNameFromPath(file.Path);
			if (string.IsNullOrEmpty(strategyName)) continue;

			foreach (var item in ReadFile<T>(file.Path, startDate, endDate))
			{
				yield return (file.Code!, strategyName, item);
			}
		}
	}

	private string ExtractStrategyNameFromPath(string filePath)
	{
		var fileName = Path.GetFileNameWithoutExtension(filePath);
		var parts = fileName.Split('_');

		// Ищем последнюю дату в формате YYYY-MM-DD
		var lastDateIndex = Array.FindLastIndex(parts, p =>
			Regex.IsMatch(p, @"^\d{4}-\d{2}-\d{2}$"));

		if (lastDateIndex >= 0 && lastDateIndex + 1 < parts.Length)
			return string.Join("_", parts.Skip(lastDateIndex + 1));

		return parts[^1];
	}

	private string GetPath(TradeDataType dataType)
	{
		if (!_paths.TryGetValue(dataType, out var path))
			throw new ArgumentException($"Unknown data type: {dataType}", nameof(dataType));
		return path;
	}

	private static string? ExtractTradeCode(string fileName)
	{
		var name = Path.GetFileNameWithoutExtension(fileName);
		var code = name.Split('_').FirstOrDefault();
		return string.IsNullOrWhiteSpace(code) ? null : code.ToUpperInvariant();
	}

	private string? FindFilePathByCode(TradeDataType dataType, string tradeCode)
	{
		var directory = GetPath(dataType);
		if (!Directory.Exists(directory)) return null;

		var prefix = $"{tradeCode.ToUpperInvariant()}_";

		return Directory.EnumerateFiles(directory, "*.csv")
			.FirstOrDefault(f => Path.GetFileName(f).StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
	}

	private IEnumerable<T> ReadFile<T>(string filePath, DateTime? startDate, DateTime? endDate) where T : class
	{
		return typeof(T) switch
		{
			var t when t == typeof(TradeBar) => ReadTradeBars(filePath, startDate, endDate).Cast<T>(),
			var t when t == typeof(StrategyResult) => ReadStrategyResults(filePath, startDate, endDate).Cast<T>(),
			var t when t == typeof(ScreenerData) => ReadScreenerDataInternal(filePath, startDate, endDate).Cast<T>(),
			_ => throw new NotSupportedException($"Type {typeof(T).Name} is not supported")
		};
	}

	private IEnumerable<TradeBar> ReadTradeBars(string filePath, DateTime? startDate, DateTime? endDate)
	{
		var lines = File.ReadAllLines(filePath);
		for (int i = 1; i < lines.Length; i++)
		{
			var parts = lines[i].Split(',');
			if (parts.Length < 6) continue;

			var begin = ParseDate(parts[0]);
			if (!IsInRange(begin, startDate, endDate)) continue;

			yield return new TradeBar
			{
				Begin = begin,
				Open = ParseDecimal(parts[1]),
				Close = ParseDecimal(parts[2]),
				High = ParseDecimal(parts[3]),
				Low = ParseDecimal(parts[4]),
				Volume = ParseLong(parts[5])
			};
		}
	}

	private IEnumerable<StrategyResult> ReadStrategyResults(string filePath, DateTime? startDate, DateTime? endDate)
	{
		var lines = File.ReadAllLines(filePath);
		for (int i = 1; i < lines.Length; i++)
		{
			var parts = lines[i].Split(',');
			if (parts.Length < 3) continue;

			var begin = ParseDate(parts[0]);
			if (!IsInRange(begin, startDate, endDate)) continue;

			yield return new StrategyResult
			{
				Date = begin,
				ClosePrice = ParseDecimal(parts[1]),
				Signal = parts[2].Trim().ToUpperInvariant(),
			};
		}
	}

	private IEnumerable<ScreenerData> ReadScreenerDataInternal(string filePath, DateTime? startDate, DateTime? endDate)
	{
		var lines = File.ReadAllLines(filePath);
		if (lines.Length <= 1) yield break;

		var headers = lines[0].Split(',').Select(h => h.Trim().ToLower()).ToArray();

		for (int i = 1; i < lines.Length; i++)
		{
			var parts = lines[i].Split(',');
			if (parts.Length < 6) continue;

			var begin = ParseDate(parts[0]);
			if (!IsInRange(begin, startDate, endDate)) continue;

			var data = new ScreenerData
			{
				Begin = begin,
				Open = ParseDecimal(parts[1]),
				Close = ParseDecimal(parts[2]),
				High = ParseDecimal(parts[3]),
				Low = ParseDecimal(parts[4]),
				Volume = ParseLong(parts[5])
			};

			if (parts.Length > 6) data.Ema20 = ParseNullableDecimal(parts[6]);
			if (parts.Length > 7) data.Ema50 = ParseNullableDecimal(parts[7]);
			if (parts.Length > 8) data.Ema200 = ParseNullableDecimal(parts[8]);
			if (parts.Length > 9) data.Rsi14 = ParseNullableDecimal(parts[9]);

			for (int j = 10; j < parts.Length && j < headers.Length; j++)
			{
				data.AdditionalIndicators[headers[j]] = ParseNullableDecimal(parts[j]);
			}

			yield return data;
		}
	}

	private static bool IsInRange(DateTime date, DateTime? startDate, DateTime? endDate)
	{
		if (startDate.HasValue && date < startDate.Value) return false;
		if (endDate.HasValue && date > endDate.Value) return false;
		return true;
	}

	private static DateTime ParseDate(string value)
	{
		if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
			return date;
		if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
			return date;
		throw new FormatException($"Invalid date format: {value}");
	}

	private static decimal ParseDecimal(string value) =>
		decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var r) ? r : 0m;

	private static decimal? ParseNullableDecimal(string value)
	{
		if (string.IsNullOrWhiteSpace(value) || value == "null" || value == "NaN") return null;
		return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var r) ? r : null;
	}

	private static long ParseLong(string value) =>
		long.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var r) ? r : 0;
}
