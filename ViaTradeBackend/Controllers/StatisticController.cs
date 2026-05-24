using System.ComponentModel.DataAnnotations;
using Application.Interfaces.Auth;
using Application.Interfaces.Database;
using Domain.Entities.DataBase;
using Infrastructure.Repositories.DataBase;
using Infrastructure.Repositoryes.DataBase;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Models.Trade;

namespace ViaTradeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StatisticController(
        ITradeRepository tradeRepository,
        TradeCodeRepository tradeCodeRepository,
        TradeTypeRepository tradeTypeRepository,
        IJwtHelper jwtHelper,
        UserService userService) : ControllerBase
    {
        private readonly ITradeRepository _tradeRepository = tradeRepository;
        private readonly TradeCodeRepository _tradeCodeRepository = tradeCodeRepository;
        private readonly TradeTypeRepository _tradeTypeRepository = tradeTypeRepository;
        private readonly IJwtHelper _jwtHelper = jwtHelper;
        private readonly UserService _userService = userService;

        [HttpGet("byuser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Trade>>> GetByUser(CancellationToken cancellationToken,
            DateTime? startDate, DateTime? endDate)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var trades = await _tradeRepository.GetByUserAndDateRangeAsync(userId, startDate, endDate, cancellationToken);

            return Ok(trades);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Trade>> GetTradeById(
            [Required] int id,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var trade = await _tradeRepository.GetByIdAsync(id, cancellationToken);
            if (trade == null || trade.UserId != userId) return NotFound();

            return Ok(trade);
        }

        [HttpPost("byuser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateTrade(
            [FromBody, Required] TradeRequest request,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var tradeCode = await _tradeCodeRepository.GetByIdAsync(request.TradeCodeId, cancellationToken);
            if (tradeCode == null) return NotFound();

            var tradeType = await _tradeTypeRepository.GetByIdAsync(request.TradeTypeId, cancellationToken);
            if (tradeType == null) return BadRequest(new { error = $"TradeType {request.TradeTypeId} not found" });

            double? netIncome = CalculateNetIncome(request.TradeOpen, request.TradeClose, request.Count, request.TradeTypeId);

            var trade = new Trade
            {
                DateOpen = request.DateOpen,
                DateClose = request.DateClose,
                TradeOpen = request.TradeOpen,
                TradeClose = request.TradeClose,
                NetIncome = netIncome,
                Count = request.Count,
                Price = (decimal)request.TradeOpen * request.Count,
                TradeTypeId = request.TradeTypeId,
                TradeCodeId = request.TradeCodeId,
                UserId = userId
            };

            await _tradeRepository.AddAsync(trade, cancellationToken);
            await _tradeRepository.SaveChangesAsync(cancellationToken);

            return Created();
        }

        [HttpPut("byuser/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateTrade(
            [Required, FromRoute] int id,
            [FromBody, Required] TradeRequest request,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var trade = await _tradeRepository.GetByIdAsync(id, cancellationToken);
            if (trade == null || trade.UserId != userId) return NotFound();

            var tradeCode = await _tradeCodeRepository.GetByIdAsync(request.TradeCodeId, cancellationToken);
            if (tradeCode == null) return NotFound();

            var tradeType = await _tradeTypeRepository.GetByIdAsync(request.TradeTypeId, cancellationToken);
            if (tradeType == null) return BadRequest(new { error = $"TradeType {request.TradeTypeId} not found" });

            double? netIncome = CalculateNetIncome(request.TradeOpen, request.TradeClose, request.Count, request.TradeTypeId);

            trade.DateOpen = request.DateOpen;
            trade.DateClose = request.DateClose;
            trade.TradeOpen = request.TradeOpen;
            trade.TradeClose = request.TradeClose;
            trade.NetIncome = netIncome;
            trade.Count = request.Count;
            trade.Price = (decimal)request.TradeOpen * request.Count;
            trade.TradeTypeId = request.TradeTypeId;
            trade.TradeCodeId = request.TradeCodeId;

            _tradeRepository.Update(trade);
            await _tradeRepository.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        [HttpDelete("byuser/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteTrade(
            [Required, FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var trade = await _tradeRepository.GetByIdAsync(id, cancellationToken);

            if (trade == null) return NotFound();

            if (trade.UserId != userId) return Forbid();

            _tradeRepository.Remove(trade);
            await _tradeRepository.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        private static double? CalculateNetIncome(double tradeOpen, double? tradeClose, int count, int tradeTypeId)
        {
            if (tradeClose == null) return null;

            if (tradeOpen == 0) return null;

            var percent = (tradeClose.Value - tradeOpen) / tradeOpen * 100;

            return Math.Round(percent, 2);
        }
    }
}