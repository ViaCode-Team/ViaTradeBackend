using Application.Interfaces;
using Application.Interfaces.Utils;
using Domain.Entities.DataBase;
using Domain.Models.Dto;
using Domain.Models.Dto.NoteRemind;
using Domain.Models.Dto.Statistic;
using Domain.Models.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Attribute;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RemindController(
	ITradeRemindService tradeRemindService,
	IJwtHelper jwtHelper) : ControllerBase
{
	private readonly ITradeRemindService _tradeRemindService = tradeRemindService;
	private readonly IJwtHelper _jwtHelper = jwtHelper;

	[Authorize]
	[HttpGet("statistics")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<TradeRemindStatistic>> GetRemindStatistics(CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var statistics = await _tradeRemindService.GetRemindStatisticAsync(userId, cancellationToken);
		return Ok(statistics);
	}

	[ServicePassword]
	[HttpGet("byuser/actual")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<TradeRemind>>> GetActualReminders(CancellationToken cancellationToken)
	{
		var reminders = await _tradeRemindService.GetActualRemindAsync(cancellationToken);
		return Ok(reminders);
	}

	[ServicePassword]
	[HttpDelete("byuser/actual/{remindId}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<ActionResult> DeleteActualReminder([FromRoute, Required] int remindId, CancellationToken cancellationToken)
	{
		await _tradeRemindService.DeleteActualRemindAsync(remindId, cancellationToken);
		return NoContent();
	}

	[Authorize]
	[HttpGet("byuser")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<PagedResult<TradeRemindDto>>> GetUserReminders([FromQuery] PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var userReminders = await _tradeRemindService.GetByUserPagedAsync(userId, paginationRequest, cancellationToken);
		return Ok(userReminders);
	}

	[Authorize]
	[HttpGet("byuser/instrument/{idInstrument}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<PagedResult<TradeRemindDto>>> GetUserRemindersByInstrument(
		[FromQuery] PaginationRequest paginationRequest,
		[Required, FromRoute] int idInstrument,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var reminders = await _tradeRemindService.GetByUserAndTradeCodePagedAsync(userId, idInstrument, paginationRequest, cancellationToken);
		return Ok(reminders);
	}

	[Authorize]
	[HttpGet("byuser/{remindId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<TradeRemind>> GetUserReminderById(
		[Required] int remindId,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var reminder = await _tradeRemindService.GetByIdAsync(remindId, userId, cancellationToken);
		return Ok(reminder);
	}

	[Authorize]
	[HttpPost("byuser/instrument/{idInstrument}")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<ActionResult> CreateUserReminder(
		[Required, FromRoute] int idInstrument,
		[FromBody, Required] TradeRemindRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _tradeRemindService.CreateAsync(userId, idInstrument, request, cancellationToken);
		return Created();
	}

	[Authorize]
	[HttpPut("byuser/{remindId}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<ActionResult> UpdateUserReminder(
		[Required, FromRoute] int remindId,
		[FromBody, Required] TradeRemindRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _tradeRemindService.UpdateAsync(remindId, userId, request, cancellationToken);
		return NoContent();
	}

	[Authorize]
	[HttpDelete("byuser/{remindId}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<ActionResult> DeleteUserReminder(
		[Required, FromRoute] int remindId,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _tradeRemindService.DeleteAsync(remindId, userId, cancellationToken);
		return NoContent();
	}
}
