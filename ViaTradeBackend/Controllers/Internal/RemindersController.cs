using System.ComponentModel.DataAnnotations;
using Application.Reminders.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Attribute;
using ViaTradeBackend.Contracts.Reminders;
using ViaTradeBackend.Mappings;

namespace ViaTradeBackend.Controllers.Internal;

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
