using System.ComponentModel.DataAnnotations;
using Application.Interfaces;
using Application.Interfaces.Auth;
using Domain.Entities.DataBase;
using Domain.Models.Dto.Statistic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Models.Trade;

namespace ViaTradeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StatisticController(
        IStatisticService statisticService,
        IJwtHelper jwtHelper) : ControllerBase
    {
        private readonly IStatisticService _statisticService = statisticService;
        private readonly IJwtHelper _jwtHelper = jwtHelper;

        [HttpGet("global/byuser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<GlobalStatistic>> GetGlobalByUser(CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var tradeStatistics = await _statisticService.GetGlobalStatisticAsync(userId, cancellationToken);
            return Ok(tradeStatistics);
        }

        [HttpGet("byuser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Trade>>> GetByUser(CancellationToken cancellationToken,
            [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] TradeSignal? tradeSignal)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var trades = await _statisticService.GetByUserAsync(userId, startDate, endDate, tradeSignal, cancellationToken);
            return Ok(trades);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Trade>> GetTradeById(
            [Required] int id,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var trade = await _statisticService.GetTradeByIdAsync(id, userId, cancellationToken);
            return Ok(trade);
        }

        [HttpPost("byuser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Trade>> CreateTrade(
            [FromBody, Required] TradeRequest request,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var trade = await _statisticService.CreateTradeAsync(request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetTradeById), new { id = trade.Id }, trade);
        }

        [HttpPut("byuser/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateTrade(
            [Required, FromRoute] int id,
            [FromBody, Required] TradeRequest request,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            await _statisticService.UpdateTradeAsync(id, request, userId, cancellationToken);
            return NoContent();
        }

        [HttpDelete("byuser/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteTrade(
            [Required, FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            await _statisticService.DeleteTradeAsync(id, userId, cancellationToken);
            return NoContent();
        }
    }
}
