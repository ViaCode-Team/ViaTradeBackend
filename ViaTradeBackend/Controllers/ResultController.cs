using System.ComponentModel.DataAnnotations;
using Application.Interfaces;
using Application.Interfaces.Auth;
using Domain.Models.Dto.Statistic;
using Domain.Models.TradeLogic;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ViaTradeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResultController(ITradeResultsService tradeResultsService,
        IJwtHelper jwtHelper) : ControllerBase
    {
        private readonly ITradeResultsService _tradeResultsService = tradeResultsService;
        private readonly IJwtHelper _jwtHelper = jwtHelper;

        [HttpGet("strategy/actual/statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<SignalStatistic>> GetSignalByUser(CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var signalStatistics = await _tradeResultsService.GetSignalStatisticAsync(userId, cancellationToken);
            return Ok(signalStatistics);
        }

        [HttpGet("strategy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<StrategyResultResponse>> GetResult(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endTime,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var response = await _tradeResultsService.GetStrategyResultAsync(userId, startDate, endTime, cancellationToken);
            return Ok(response);
        }

        [HttpGet("strategy/{strategyName}/{tradeCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<StrategyResultResponse>> GetResultByStrategyAndTradeCode(
            [FromRoute, Required] string strategyName,
            [FromRoute, Required] string tradeCode,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endTime,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var response = await _tradeResultsService.GetStrategyResultByCodeAsync(userId, strategyName, tradeCode, startDate, endTime, cancellationToken);
            return Ok(response);
        }
    }
}
