using Domain.Entities.CSV;
using Domain.Models.TradeLogic;

namespace Application.Interfaces
{
    public interface IFileReader
    {
        /// <summary>
        /// Returns available trade codes for the specified data type.
        /// If filterCodes is provided, returns only codes from that list.
        /// </summary>
        IEnumerable<TradeCodeFile> GetTradeCodes(TradeDataType dataType, IEnumerable<string>? filterCodes = null);

        /// <summary>
        /// Reads data for multiple trade codes with optional date filtering.
        /// Files not found are skipped silently (logged if needed).
        /// </summary>
        IEnumerable<(string TradeCode, T Item)> ReadDataByCodes<T>(
           TradeDataType dataType,
           IEnumerable<string> tradeCodes,
           DateTime? startDate = null,
           DateTime? endDate = null) where T : class;

        IEnumerable<(string TradeCode, string StrategyName, T Item)> ReadDataByCodesWithStrategy<T>(
            TradeDataType dataType,
            IEnumerable<string> tradeCodes,
            DateTime? startDate = null,
            DateTime? endDate = null) where T : class;
    }
}
