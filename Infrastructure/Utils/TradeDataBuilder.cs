using System.Globalization;
using System.Text.RegularExpressions;
using Application.Interfaces;
using Domain.Models.TradeLogic;

namespace Infrastructure.Utils
{
    public class TradeDataBuilder : ITradeDataBuilder
    {
        // Pattern: {TradeCode}_{TimeFrame}_{StartDate}_{EndDate}.csv
        // TimeFrame может содержать цифры (HOUR_1), поэтому используем нежадный захват .*?
        private static readonly Regex FileNameRegex = new(
            @"^(?<TradeCode>[A-Z0-9]+)_(?<TimeFrame>.*?)_(?<Start>\d{4}-\d{2}-\d{2})_(?<End>\d{4}-\d{2}-\d{2})_?\.csv$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        public IEnumerable<TradeCodeResonse> BuildFileTradeResonse(IEnumerable<string>? fileNames)
        {
            if (fileNames == null)
            {
                yield break;
            }

            foreach (var fileName in fileNames)
            {
                // Process only valid file names, skip invalid silently or log if needed
                if (TryParseFileName(fileName, out var response))
                {
                    yield return response;
                }
            }
        }

        private static bool TryParseFileName(string fileName, out TradeCodeResonse result)
        {
            result = null!;

            // Extract just the file name without path
            var nameOnly = Path.GetFileName(fileName);
            var match = FileNameRegex.Match(nameOnly);

            if (!match.Success)
            {
                return false;
            }

            // Try to parse dates with invariant culture to avoid locale issues
            if (!DateTime.TryParseExact(
                    match.Groups["Start"].Value,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var startDate) ||
                !DateTime.TryParseExact(
                    match.Groups["End"].Value,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var endDate))
            {
                return false;
            }

            result = new TradeCodeResonse
            {
                TradeCode = match.Groups["TradeCode"].Value.ToUpperInvariant(),
                TimeFrame = match.Groups["TimeFrame"].Value,
                StartDate = startDate,
                EndDate = endDate,
            };

            return true;
        }
    }
}