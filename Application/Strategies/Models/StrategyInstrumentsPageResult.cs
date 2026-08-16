using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Models;

namespace ViaTrade.Application.Strategies.Models;

public record StrategyInstrumentsPageResult(bool StrategyExists, PageResult<RelatedInstrumentDto> Page);
