using System.ComponentModel.DataAnnotations;
using Application.Interfaces;
using Application.Interfaces.Utils;
using Domain.Entities.DataBase;
using Domain.Models.Dto.NoteRemind;
using Domain.Models.Dto.Statistic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Models.Note;

namespace ViaTradeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NoteController(
        INoteService noteService,
        IJwtHelper jwtHelper) : ControllerBase
    {
        private readonly INoteService _noteService = noteService;
        private readonly IJwtHelper _jwtHelper = jwtHelper;

        [HttpGet("statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<NoteStatistic>> GetNoteStatistics(CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var statistics = await _noteService.GetNoteStatisticAsync(userId, cancellationToken);
            return Ok(statistics);
        }

        [HttpGet("byuser/instrument")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Note>>> GetByUserInstrumentAll(CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var notes = await _noteService.GetUserNoteByPropAllAsync(userId, NoteType.TradeCodeNote, cancellationToken);
            return Ok(notes);
        }

        [HttpGet("byuser/instrument/{idInstrument}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Note>> GetNoteByUserInstrument(
            [Required, FromRoute] int idInstrument,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var note = await _noteService.GetUserNoteByPropAsync(idInstrument, userId, NoteType.TradeCodeNote, cancellationToken);

            return Ok(note);
        }

        [HttpGet("byuser/strategy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Note>>> GetByUserStrategyAll(CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var notes = await _noteService.GetUserNoteByPropAllAsync(userId, NoteType.TradeStrategyNote, cancellationToken);
            return Ok(notes);
        }

        [HttpGet("byuser/strategy/{idStrategy}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Note>> GetByUserStrategy(
            [Required, FromRoute] int idStrategy,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var note = await _noteService.GetUserNoteByPropAsync(idStrategy, userId, NoteType.TradeStrategyNote, cancellationToken);
            return Ok(note);
        }

        [HttpPost("byuser/instrument/{idInstrument}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateInstrumentNote(
            [Required, FromRoute] int idInstrument,
            [FromBody, Required] NoteRequest request,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var dto = new NoteDto { UserId = userId, NoteText = request.NoteText };
            await _noteService.AddUserNoteWithValidationAsync(idInstrument, NoteType.TradeCodeNote, dto, cancellationToken);
            return Created();
        }

        [HttpPost("byuser/strategy/{idStrategy}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateStrategyNote(
            [Required, FromRoute] int idStrategy,
            [FromBody, Required] NoteRequest request,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var dto = new NoteDto { UserId = userId, NoteText = request.NoteText };
            await _noteService.AddUserNoteWithValidationAsync(idStrategy, NoteType.TradeStrategyNote, dto, cancellationToken);
            return Created();
        }

        [HttpPut("byuser/instrument/{idInstrument}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateInstrumentNote(
            [Required, FromRoute] int idInstrument,
            [FromBody, Required] NoteRequest request,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var dto = new NoteDto { UserId = userId, NoteText = request.NoteText };
            await _noteService.UpdateUserNoteWithValidationAsync(idInstrument, NoteType.TradeCodeNote, dto, cancellationToken);
            return NoContent();
        }

        [HttpPut("byuser/strategy/{idStrategy}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateStrategyNote(
            [Required, FromRoute] int idStrategy,
            [FromBody, Required] NoteRequest request,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var dto = new NoteDto { UserId = userId, NoteText = request.NoteText };
            await _noteService.UpdateUserNoteWithValidationAsync(idStrategy, NoteType.TradeStrategyNote, dto, cancellationToken);
            return NoContent();
        }

        [HttpDelete("byuser/instrument/{idInstrument}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteInstrumentNote(
            [Required, FromRoute] int idInstrument,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            await _noteService.DeleteUserNoteAsync(idInstrument, userId, NoteType.TradeCodeNote, cancellationToken);
            return NoContent();
        }

        [HttpDelete("byuser/strategy/{idStrategy}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteStrategyNote(
            [Required, FromRoute] int idStrategy,
            CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            await _noteService.DeleteUserNoteAsync(idStrategy, userId, NoteType.TradeStrategyNote, cancellationToken);
            return NoContent();
        }
    }
}
