using System.Globalization;
using Application.Interfaces;
using Domain.Entities.CSV;
using Domain.Models;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class TradeFileReader : IFileReader
    {
        private readonly Dictionary<DataType, string> _paths;

        public TradeFileReader(IOptions<AnalyzerDataOption> options)
        {
            var basePath = options.Value.SourcePath;
            _paths = new Dictionary<DataType, string>
            {
                [DataType.Futures] = Path.Combine(basePath, options.Value.FuturesDataDirectoryName),
                [DataType.Stocks] = Path.Combine(basePath, options.Value.StocksDataDirectoryName),
                [DataType.Strategy] = Path.Combine(basePath, options.Value.StrategyResultDirectoryName),
                [DataType.Screener] = Path.Combine(basePath, options.Value.ScrennerResultDirectoryName)
            };
        }

        public IEnumerable<string> GetFileNames(DataType dataType)
        {
            var path = GetPath(dataType);
            return Directory.Exists(path)
                ? Directory.GetFiles(path, "*.csv").Select(Path.GetFileName)
                : Enumerable.Empty<string>();
        }

        public IEnumerable<T> ReadData<T>(DataType dataType, string fileName, DateTime? startDate = null, DateTime? endDate = null) where T : class
        {
            var filePath = Path.Combine(GetPath(dataType), fileName);
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            return typeof(T) switch
            {
                var t when t == typeof(TradeBar) => ReadTradeBars(filePath, startDate, endDate).Cast<T>(),
                var t when t == typeof(StrategyResult) => ReadStrategyResults(filePath, startDate, endDate).Cast<T>(),
                var t when t == typeof(ScreenerData) => ReadScreenerDataInternal(filePath, startDate, endDate).Cast<T>(),
                _ => throw new NotSupportedException($"Type {typeof(T).Name} is not supported")
            };
        }

        private string GetPath(DataType dataType)
        {
            if (!_paths.TryGetValue(dataType, out var path))
                throw new ArgumentException($"Unknown data type: {dataType}", nameof(dataType));
            return path;
        }

        private IEnumerable<TradeBar> ReadTradeBars(string filePath, DateTime? startDate, DateTime? endDate)
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length <= 1)
                yield break;

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length < 6)
                    continue;

                var begin = ParseDate(parts[0]);
                if (!IsInRange(begin, startDate, endDate))
                    continue;

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
            if (lines.Length <= 1)
                yield break;

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length < 3)
                    continue;

                var begin = ParseDate(parts[0]);
                if (!IsInRange(begin, startDate, endDate))
                    continue;

                yield return new StrategyResult
                {
                    Begin = begin,
                    Close = ParseDecimal(parts[1]),
                    Signal = parts[2].Trim().ToUpperInvariant()
                };
            }
        }

        private IEnumerable<ScreenerData> ReadScreenerDataInternal(string filePath, DateTime? startDate, DateTime? endDate)
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length <= 1)
                yield break;

            var headers = lines[0].Split(',').Select(h => h.Trim().ToLower()).ToArray();

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length < 6)
                    continue;

                var begin = ParseDate(parts[0]);
                if (!IsInRange(begin, startDate, endDate))
                    continue;

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
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var r) ? r : (decimal?)null;
        }

        private static long ParseLong(string value) =>
            long.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var r) ? r : 0;

        private static int ParseInt(string value) =>
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) ? r : 0;
    }
}