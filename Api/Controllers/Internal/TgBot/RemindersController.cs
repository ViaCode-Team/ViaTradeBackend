using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ViaTrade.Api.Attribute;
using ViaTrade.Api.Contracts.Reminders;
using ViaTrade.Api.Mappings;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Reminders.Interfaces;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Api.Controllers.Internal.TgBot;

[Route($"{ApiRoutes.V1.TgBot}/[controller]")]
[ApiExplorerSettings(GroupName = InternalServices.TgBot)]
[ApiController]
public class RemindersController(
	IReminderCommandService reminderCommandService,
	IReminderQueryService reminderQueryService,
	IOptions<NotificationStreamSettings> options
) : ControllerBase
{
	[ServicePassword]
	[HttpGet("due")]
	public async Task<Ok<IEnumerable<DueReminderResponse>>> GetDue(CancellationToken ct)
	{
		var reminders = await reminderQueryService.ListDueBatchAsync(options.Value.ReminderPublishBatchSize, ct);

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
