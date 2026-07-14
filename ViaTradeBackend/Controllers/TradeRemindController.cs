using Application.Interfaces;
using Application.Interfaces.Utils;
using Domain.Entities.DataBase;
using Domain.Models.Dto;
using Domain.Models.Dto.Statistic;
using Domain.Models.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Attribute;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TradeRemindController(
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
		var statistics = await _tradeRemindService.GetTradeRemindStatisticAsync(userId, cancellationToken);
		return Ok(statistics);
	}

	[ServicePassword]
	[HttpGet("byuser/actual")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<List<TradeRemind>>> GetActualRemind(CancellationToken cancellationToken)
	{
		var reminders = await _tradeRemindService.GetActualRemindAsync(cancellationToken);
		return Ok(reminders);
	}

	[ServicePassword]
	[HttpDelete("byuser/actual/{remindId}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<ActionResult> DeleteActualRemind([FromRoute, Required] int remindId, CancellationToken cancellationToken)
	{
		await _tradeRemindService.DeleteActualRemindAsync(remindId, cancellationToken);
		return NoContent();
	}

	[Authorize]
	[HttpGet("byuser")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<PagedResult<TradeRemind>>> GetAllByUser([FromQuery] PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var reminders = await _tradeRemindService.GetByUserPagedAsync(userId, paginationRequest, cancellationToken);
		return Ok(reminders);
	}

	[Authorize]
	[HttpGet("byuser/instrument/{idInstrument}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<PagedResult<TradeRemind>>> GetByUserInstrument(
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
	public async Task<ActionResult<TradeRemind>> GetRemindById(
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
	public async Task<ActionResult> Create(
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
	public async Task<ActionResult> Update(
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
	public async Task<ActionResult> Delete(
		[Required, FromRoute] int remindId,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _tradeRemindService.DeleteAsync(remindId, userId, cancellationToken);
		return NoContent();
	}
}
