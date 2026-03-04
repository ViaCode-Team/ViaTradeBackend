using Application.Interfaces;
using Domain.Entities.CSV;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Utils
{
    public class CsvHelper
    {
        private readonly IFileReader _fileReader;
        private readonly AnalyzerDataOption _options;
        private readonly ILogger<CsvHelper> _logger;
        private readonly Dictionary<DataType, string> _paths;

        public CsvHelper(
            IFileReader fileReader,
            IOptions<AnalyzerDataOption> options,
            ILogger<CsvHelper> logger)
        {
            _fileReader = fileReader;
            _options = options.Value;
            _logger = logger;

            var basePath = _options.SourcePath;
            _paths = new Dictionary<DataType, string>
            {
                [DataType.Futures] = Path.Combine(basePath, _options.FuturesDataDirectoryName),
                [DataType.Stocks] = Path.Combine(basePath, _options.StocksDataDirectoryName),
                [DataType.Strategy] = Path.Combine(basePath, _options.StrategyResultDirectoryName),
                [DataType.Screener] = Path.Combine(basePath, _options.ScrennerResultDirectoryName)
            };
        }

        public IEnumerable<string> GetFileNames(DataType dataType)
        {
            return _fileReader.GetFileNames(dataType);
        }

        public async Task<Dictionary<DataType, List<string>>> GetAllFilesAsync(CancellationToken ct = default)
        {
            await Task.Yield();

            return Enum.GetValues<DataType>().ToDictionary(
                type => type,
                type => _fileReader.GetFileNames(type).ToList()
            );
        }

        public async Task<List<string>> FindFilesByPatternAsync(
            DataType dataType,
            string pattern,
            CancellationToken ct = default)
        {
            await Task.Yield();

            _logger.LogDebug("Finding files for {DataType} with pattern {Pattern}", dataType, pattern);

            return _fileReader.GetFileNames(dataType)
                .Where(f => f.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public IEnumerable<T> ReadData<T>(DataType dataType, string fileName, DateTime? startDate = null, DateTime? endDate = null) where T : class
        {
            return _fileReader.ReadData<T>(dataType, fileName, startDate, endDate);
        }

        public List<TradeBar> LoadTradingData(
            DataType dataType,
            IEnumerable<string> fileNames,
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Loading trading data for {DataType}, files: {Count}",
                dataType, fileNames.Count());

            var allBars = new List<TradeBar>();

            foreach (var fileName in fileNames)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var bars = _fileReader.ReadData<TradeBar>(dataType, fileName, startDate, endDate);
                    allBars.AddRange(bars);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read file {FileName}", fileName);
                }
            }

            return allBars.OrderBy(b => b.Begin).ToList();
        }

        public List<StrategyResult> LoadStrategyResults(
            IEnumerable<string> fileNames,
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Loading strategy results, files: {Count}", fileNames.Count());

            var allResults = new List<StrategyResult>();

            foreach (var fileName in fileNames)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var results = _fileReader.ReadData<StrategyResult>(DataType.Strategy, fileName, startDate, endDate);
                    allResults.AddRange(results);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read strategy file {FileName}", fileName);
                }
            }

            return allResults.OrderBy(r => r.Begin).ToList();
        }

        public List<ScreenerData> LoadScreenerData(
            IEnumerable<string> fileNames,
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Loading screener data, files: {Count}", fileNames.Count());

            var allData = new List<ScreenerData>();

            foreach (var fileName in fileNames)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var data = _fileReader.ReadData<ScreenerData>(DataType.Screener, fileName, startDate, endDate);
                    allData.AddRange(data);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read screener file {FileName}", fileName);
                }
            }

            return allData.OrderBy(d => d.Begin).ToList();
        }

        public async Task<TradingInstrumentData> GetInstrumentDataAsync(
            string instrumentCode,
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Getting data for instrument {InstrumentCode}", instrumentCode);

            var result = new TradingInstrumentData
            {
                InstrumentCode = instrumentCode
            };

            var futuresFiles = _fileReader.GetFileNames(DataType.Futures)
                .Where(f => f.StartsWith(instrumentCode, StringComparison.OrdinalIgnoreCase));

            if (futuresFiles.Any())
            {
                result.DataType = DataType.Futures;
                result.Bars = LoadTradingData(DataType.Futures, futuresFiles, startDate, endDate, ct);
            }
            else
            {
                var stocksFiles = _fileReader.GetFileNames(DataType.Stocks)
                    .Where(f => f.StartsWith(instrumentCode, StringComparison.OrdinalIgnoreCase));

                if (stocksFiles.Any())
                {
                    result.DataType = DataType.Stocks;
                    result.Bars = LoadTradingData(DataType.Stocks, stocksFiles, startDate, endDate, ct);
                }
            }

            var strategyFiles = _fileReader.GetFileNames(DataType.Strategy)
                .Where(f => f.StartsWith(instrumentCode, StringComparison.OrdinalIgnoreCase));

            if (strategyFiles.Any())
            {
                result.StrategyResults = LoadStrategyResults(strategyFiles, startDate, endDate, ct);
            }

            var screenerFiles = _fileReader.GetFileNames(DataType.Screener)
                .Where(f => f.StartsWith(instrumentCode, StringComparison.OrdinalIgnoreCase));

            if (screenerFiles.Any())
            {
                result.ScreenerData = LoadScreenerData(screenerFiles, startDate, endDate, ct);
            }

            return result;
        }

        public async Task<Dictionary<string, int>> GetSignalStatisticsAsync(
            IEnumerable<string> strategyFiles,
            CancellationToken ct = default)
        {
            _logger.LogInformation("Calculating signal statistics");

            var statistics = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var fileName in strategyFiles)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var results = _fileReader.ReadData<StrategyResult>(DataType.Strategy, fileName);

                    foreach (var result in results)
                    {
                        if (!statistics.ContainsKey(result.Signal))
                        {
                            statistics[result.Signal] = 0;
                        }
                        statistics[result.Signal]++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process file {FileName} for statistics", fileName);
                }
            }

            return statistics;
        }

        public async Task<decimal?> GetLatestClosePriceAsync(
            DataType dataType,
            string fileName,
            CancellationToken ct = default)
        {
            await Task.Yield();

            try
            {
                var bars = _fileReader.ReadData<TradeBar>(dataType, fileName);
                return bars.LastOrDefault()?.Close;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get latest close price from {FileName}", fileName);
                return null;
            }
        }
    }
}
