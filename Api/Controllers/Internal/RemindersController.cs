using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Attribute;
using ViaTrade.Api.Contracts.Reminders;
using ViaTrade.Api.Mappings;
using ViaTrade.Application.Reminders.Interfaces;

namespace ViaTrade.Api.Controllers.Internal;

[Route("api/v1/internal/[controller]")]
[ApiExplorerSettings(GroupName = "internal")]
[ApiController]
public class RemindersController(
	IReminderCommandService reminderCommandService,
	IReminderQueryService reminderQueryService
) : ControllerBase
{
	[ServicePassword]
	[HttpGet("due")]
	public async Task<Ok<IEnumerable<DueReminderResponse>>> GetDue(CancellationToken ct)
	{
		var reminders = await reminderQueryService.ListDueAsync(ct);

		return TypedResults.Ok(reminders.Select(ApiMapper.ToDueResponse));
	}

	[ServicePassword]
	[HttpPut("{reminderId:int}/delivery")]
	public async Task<NoContent> ConfirmDelivery(
		[FromRoute, Range(1, int.MaxValue)] int reminderId,
		[FromBody, Required] ConfirmReminderDeliveryRequest request,
		CancellationToken ct
	)
	{
		await reminderCommandService.MarkDeliveredAsync(request.UserId, reminderId, ct);

		return TypedResults.NoContent();
	}
}
