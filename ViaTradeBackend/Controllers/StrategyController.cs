using System.ComponentModel.DataAnnotations;
using Application.Interfaces;
using Application.Interfaces.Auth;
using Domain.Entities.DataBase;
using Domain.Models.Dto;
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
    public class StrategyController(UserTradeStrategyRepository userTradeStrategyRepository, TradeStrategyRepository tradeStrategyRepository,
        IJwtHelper jwtHelper, UserService userService, UserStrategyTradeCodeRepository userStrategyTradeCodeRepository) : ControllerBase
    {
        private readonly TradeStrategyRepository _tradeStrategyRepository = tradeStrategyRepository;
        private readonly UserTradeStrategyRepository _userTradeStrategyRepository = userTradeStrategyRepository;
        private readonly UserStrategyTradeCodeRepository _userStrategyTradeCodeRepository = userStrategyTradeCodeRepository;
        private readonly UserService _userService = userService;
        private readonly IJwtHelper _jwtHelper = jwtHelper;

        // ToDo: Create StrategyService

        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TradeStrategy>>> GetAll(CancellationToken cancellationToken)
        {
            var response = await _tradeStrategyRepository.GetAllAsync(cancellationToken);

            return Ok(response);
        }

        [HttpGet("{strategyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<TradeStrategy>> GetById([Required] int strategyId, CancellationToken cancellationToken)
        {
            var response = await _tradeStrategyRepository.GetByIdAsync(strategyId, cancellationToken)
                ?? throw new KeyNotFoundException();

            return Ok(response);
        }

        [HttpGet("byuser/instrumentslink")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserStrategyTradeCodeDto>>> GetAllInstrumentsLink(CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var user = await _userService.EnsureUserAsunc(userId, cancellationToken);

            var response = await _userStrategyTradeCodeRepository.GetAllAsync(user.Id, cancellationToken);

            return Ok(response);
        }

        [HttpPost("byuser/instrumentslink")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateInstrumentsLink(
            [FromBody, Required] UserStrategyTradeCodeRequest userStrategyTradeCodeRequest,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var user = await _userService.EnsureUserAsunc(userId, cancellationToken);

            var newUserStrategyCode = new UserStrategyTradeCode
            {
                StrategyId = userStrategyTradeCodeRequest.StrategyId,
                TradeCodeId = userStrategyTradeCodeRequest.TradeCodeId,
                UserId = user.Id
            };

            await _userStrategyTradeCodeRepository.AddAsync(newUserStrategyCode, cancellationToken);
            await _userStrategyTradeCodeRepository.SaveChangesAsync(cancellationToken);

            return Created();
        }

        [HttpDelete("byuser/instrumentslink")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteInstrumentsLink(
            [FromQuery, Required] int strategyId,
            [FromQuery, Required] int tradeCodeId,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);

            var affectedRows = await _userStrategyTradeCodeRepository.ExecuteDeleteAsync(
                e => e.UserId == userId &&
                     e.StrategyId == strategyId &&
                     e.TradeCodeId == tradeCodeId,
                cancellationToken);

            if (affectedRows == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("byuser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserTradeStrategy>>> GetUsersStrategy(CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var user = await _userService.EnsureUserAsunc(userId, cancellationToken);

            var response = await _userTradeStrategyRepository.GetByUser(user.Id, cancellationToken);

            return Ok(response);
        }

        [HttpPost("byuser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateUsersStrategy([FromBody, Required] CreateUserStrategyRequest userStrategyRequest, CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var user = await _userService.EnsureUserAsunc(userId, cancellationToken);

            var strategyLink = new UserTradeStrategy
            {
                TradeStrategyId = userStrategyRequest.StrategyId,
                UserId = user.Id
            };

            await _userTradeStrategyRepository.AddAsync(strategyLink, cancellationToken);
            await _userTradeStrategyRepository.SaveChangesAsync(cancellationToken);

            return Created();
        }

        [HttpDelete("byuser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteUsersStrategy([FromQuery, Required] int strategyId, CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);

            var affectedRows = await _userTradeStrategyRepository.ExecuteDeleteAsync(
                e => e.UserId == userId && e.TradeStrategyId == strategyId,
                cancellationToken);

            if (affectedRows == 0)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
