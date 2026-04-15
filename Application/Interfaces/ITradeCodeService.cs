using Domain.Entities.CSV;
using Domain.Entities.DataBase;
using Domain.Models.Dto;

namespace Application.Interfaces
{
    public interface ITradeCodeService
    {
        Task<IEnumerable<TradeCode>> GetAllCodesAsync(CancellationToken ct = default);

        Task<IEnumerable<TradeCodeFileDto>> GetSysAllCodesAsync(TradeDataType dataType, CancellationToken ct = default);
        Task<TradeCodeFileDto> GetSysCodeByIdAsync(TradeDataType dataType, string tradeIdString, CancellationToken ct = default);
    }
}
