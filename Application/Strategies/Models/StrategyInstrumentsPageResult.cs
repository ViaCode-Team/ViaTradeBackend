using Application.Common.Models;
using Application.Instruments.Models;

namespace Application.Strategies.Models;

public record StrategyInstrumentsPageResult(bool StrategyExists, PageResult<RelatedInstrumentDto> Page);
