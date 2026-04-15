using Application.Interfaces;
using Domain.Entities.CSV;
using Domain.Entities.DataBase;
using Domain.Models.Dto;
using Infrastructure.Repositoryes.DataBase;

namespace Infrastructure.Services
{
    public class TradeCodeService(
        IFileReader tradefileReader,
        TradeCodeRepository tradeCodeRepository) : ITradeCodeService
    {
        private readonly IFileReader _tradefileReader = tradefileReader;
        private readonly TradeCodeRepository _tradeCodeRepository = tradeCodeRepository;

        public async Task<IEnumerable<TradeCode>> GetAllCodesAsync(CancellationToken ct = default)
        {
            return await _tradeCodeRepository.GetAllAsync(ct);
        }

        public async Task<IEnumerable<TradeCodeFileDto>> GetSysAllCodesAsync(
            TradeDataType dataType,
            CancellationToken ct = default)
        {
            var tradeFiles = _tradefileReader.GetTradeCodes(dataType);
            var tradeCodes = await _tradeCodeRepository.GetAllAsync(ct);

            var dbCodeMap = tradeCodes
                .ToDictionary(
                    c => c.ExchangeId,
                    c => c.Id,
                    StringComparer.OrdinalIgnoreCase
                );

            return tradeFiles
                .Where(fc => dbCodeMap.ContainsKey(fc.TradeCode))
                .Select(fc => new TradeCodeFileDto(
                    Id: dbCodeMap[fc.TradeCode],
                    ExchangeId: fc.TradeCode,
                    TimeFrame: fc.TimeFrame,
                    StartDate: fc.StartDate,
                    EndDate: fc.EndDate
                ));
        }

        public async Task<TradeCodeFileDto> GetSysCodeByIdAsync(
            TradeDataType dataType,
            string tradeIdString,
            CancellationToken ct = default)
        {
            string exchangeId;
            int? dbId = null;

            if (int.TryParse(tradeIdString, out var tradeCodeId))
            {
                var dbEntity = await _tradeCodeRepository.GetByIdAsync(tradeCodeId, ct) 
                    ?? throw new KeyNotFoundException($"TradeCode with Id {tradeCodeId} not found in database");
                
                exchangeId = dbEntity.ExchangeId;
                dbId = dbEntity.Id;
            }
            else
            {
                exchangeId = tradeIdString;

                var dbEntities = await _tradeCodeRepository.FindAsync(
                    c => c.ExchangeId == exchangeId,
                    ct);

                dbId = dbEntities.FirstOrDefault()?.Id;
            }

            var fileCodes = _tradefileReader.GetTradeCodes(dataType, [exchangeId]);
            var fileData = fileCodes.FirstOrDefault() 
                ?? throw new KeyNotFoundException($"No file data found for trade code '{exchangeId}'");

            if (dbId == null)
            {
                var dbEntities = await _tradeCodeRepository.FindAsync(
                    c => c.ExchangeId == fileData.TradeCode,
                    ct);
                dbId = dbEntities.FirstOrDefault()?.Id;
            }

            if (dbId == null)
                throw new KeyNotFoundException($"TradeCode '{exchangeId}' is not registered in database");

            return new TradeCodeFileDto(
                Id: dbId.Value,
                ExchangeId: fileData.TradeCode,
                TimeFrame: fileData.TimeFrame,
                StartDate: fileData.StartDate,
                EndDate: fileData.EndDate
            );
        }
    }
}
