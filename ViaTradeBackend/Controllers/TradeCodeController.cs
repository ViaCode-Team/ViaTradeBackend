using Application.Interfaces;
using Application.Interfaces.Auth;
using Domain.Entities.CSV;
using Domain.Entities.DataBase;
using Domain.Models.Dto.Statistic;
using Domain.Models.Dto.Trade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TradeCodeController(
        IJwtHelper jwtHelper,
        ITradeCodeService tradeService) : ControllerBase
    {
        private readonly IJwtHelper _jwtHelper = jwtHelper;
        private readonly ITradeCodeService _tradeService = tradeService;

        [HttpGet("stocks/statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<StockStatistic>> GetStockStatistics(CancellationToken cancellationToken)
        {
            var result = await _tradeService.GetStockStatisticAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("stocks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TradeCode>>> GetAllStocksCodes()
        {
            var result = await _tradeService.GetAllCodesAsync();
            return Ok(result);
        }

        [HttpGet("sys/stocks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TradeCodeFileDto>>> GetSysAllStocksCodes()
        {
            var result = await _tradeService.GetSysAllCodesAsync(TradeDataType.Stocks);
            return Ok(result);
        }

        [HttpGet("sys/stocks/{tradeIdString}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<TradeCodeFileDto>> GetSysCodeById(
            [FromRoute, Required] string tradeIdString,
            CancellationToken cancellationToken)
        {
            var result = await _tradeService.GetSysCodeByIdAsync(TradeDataType.Stocks, tradeIdString, cancellationToken);
            return Ok(result);
        }
    }
}
