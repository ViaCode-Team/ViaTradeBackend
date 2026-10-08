using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ViaTrade.Api.ModelBinding.Attributes;
using ViaTrade.Api.Routing;
using ViaTrade.Api.Security.Authorization;
using ViaTrade.Application.Reminders.ListDue;
using ViaTrade.Application.Reminders.MarkDelivered;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Api.Controllers.Internal.TgBot;

[Route($"{ApiRoutes.V1.TgBot}/[controller]")]
[ApiExplorerSettings(GroupName = InternalServices.TgBot)]
[ApiController]
public class RemindersController(ISender sender, IOptions<NotificationStreamSettings> options) : ControllerBase
{
	[ServicePassword]
	[HttpGet("due")]
	public async Task<Ok<IReadOnlyList<DueReminderResult>>> GetDue(CancellationToken ct)
	{
		var reminders = await sender.Send(new ListDueRemindersQuery(options.Value.ReminderPublishBatchSize), ct);

		return TypedResults.Ok(reminders);
	}

	[ServicePassword]
	[HttpPut("{reminderId:int}/delivery")]
	public async Task<NoContent> ConfirmDelivery(
		[FromBody, FromRouteProperties(nameof(MarkReminderDeliveredCommand.ReminderId))]
			MarkReminderDeliveredCommand command,
		CancellationToken ct
	)
	{
		await sender.Send(command, ct);

		return TypedResults.NoContent();
	}
}
