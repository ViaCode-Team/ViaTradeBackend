using System.ComponentModel.DataAnnotations;
using Application.Interfaces.Auth;
using Domain.Entities.DataBase;
using Domain.Models.Dto;
using Infrastructure.Repositories.DataBase;
using Infrastructure.Repositoryes.DataBase;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Attribute;

namespace ViaTradeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradeRemindController(
        TradeRemindRepository tradeRemindRepository,
        TradeCodeRepository tradeCodeRepository,
        IJwtHelper jwtHelper,
        UserService userService) : ControllerBase
    {
        private readonly TradeRemindRepository _tradeRemindRepository = tradeRemindRepository;
        private readonly TradeCodeRepository _tradeCodeRepository = tradeCodeRepository;
        private readonly IJwtHelper _jwtHelper = jwtHelper;
        private readonly UserService _userService = userService;

        [ServicePassword]
        [HttpGet("byuser/actual")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<TradeRemind>>> GetActualRemind(CancellationToken cancellationToken)
        {
            return Ok(await _tradeRemindRepository.GetActualTradeRemind(cancellationToken));
        }

        [Authorize]
        [HttpGet("byuser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TradeRemind>>> GetAllByUser(CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var reminders = await _tradeRemindRepository.GetByUserAsync(userId, cancellationToken);
            return Ok(reminders);
        }

        [Authorize]
        [HttpGet("byuser/instrument/{idInstrument}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TradeRemind>>> GetTradeRemindByUserInstrument(
            [Required, FromRoute] int idInstrument,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            await _userService.EnsureUserAsync(userId, cancellationToken);

            // Validate TradeCode exists
            var tradeCode = await _tradeCodeRepository.GetByIdAsync(idInstrument, cancellationToken);
            if (tradeCode == null) return NotFound();

            var reminders = await _tradeRemindRepository.GetByUserAndTradeCodeAsync(userId, idInstrument, cancellationToken);
            return Ok(reminders);
        }

        [Authorize]
        [HttpGet("byuser/{remindId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<TradeRemind>> GetTradeRemindById(
            [Required] int remindId,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var reminder = await _tradeRemindRepository.GetByIdAsync(remindId, cancellationToken);
            if (reminder == null || reminder.UserId != userId) return NotFound();

            return Ok(reminder);
        }

        [Authorize]
        [HttpPost("byuser/instrument/{idInstrument}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateInstrumentRemind(
            [Required, FromRoute] int idInstrument,
            [FromBody, Required] TradeRemindRequest request,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            await _userService.EnsureUserAsync(userId, cancellationToken);

            // Validate TradeCode exists
            var tradeCode = await _tradeCodeRepository.GetByIdAsync(idInstrument, cancellationToken);
            if (tradeCode == null) return NotFound();

            var remind = new TradeRemind
            {
                TextRemind = request.TextRemind,
                DateTime = request.DateTime,
                TradeCodeId = idInstrument,
                UserId = userId
            };

            await _tradeRemindRepository.AddAsync(remind, cancellationToken);
            await _tradeRemindRepository.SaveChangesAsync(cancellationToken);

            return Created();
        }

        [Authorize]
        [HttpPut("byuser/{redindId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateRemind(
            [Required, FromRoute] int redindId,
            [FromBody, Required] TradeRemindRequest request,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var remind = await _tradeRemindRepository.GetByIdAsync(redindId, cancellationToken);
            if (remind == null || remind.UserId != userId) return NotFound();

            // Validate linked TradeCode still exists
            var tradeCode = await _tradeCodeRepository.GetByIdAsync(remind.TradeCodeId, cancellationToken);
            if (tradeCode == null) return NotFound();

            remind.TextRemind = request.TextRemind;
            remind.DateTime = request.DateTime;

            _tradeRemindRepository.Update(remind);
            await _tradeRemindRepository.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        [Authorize]
        [HttpDelete("byuser/{redindId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteRemind(
            [Required, FromRoute] int redindId,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var remind = await _tradeRemindRepository.GetByIdAsync(redindId, cancellationToken);
            if (remind == null || remind.UserId != userId) return NotFound();

            _tradeRemindRepository.Remove(remind);
            await _tradeRemindRepository.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}