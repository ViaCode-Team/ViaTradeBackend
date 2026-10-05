using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ViaTrade.Api.Attribute;
using ViaTrade.Api.Contracts.Reminders;
using ViaTrade.Api.Mappings;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Reminders.Common;
using ViaTrade.Application.Reminders.ListDue;
using ViaTrade.Application.Reminders.MarkDelivered;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Api.Controllers.Internal.TgBot;

[Route($"{ApiRoutes.V1.TgBot}/[controller]")]
[ApiExplorerSettings(GroupName = InternalServices.TgBot)]
[ApiController]
public class RemindersController(IOptions<NotificationStreamSettings> options) : ControllerBase
{
	[ServicePassword]
	[HttpGet("due")]
	public async Task<Ok<IEnumerable<DueReminderResponse>>> GetDue(
		[FromServices] IQueryHandler<ListDueRemindersQuery, IReadOnlyList<ReminderResult>> handler,
		CancellationToken ct
	)
	{
		var query = new ListDueRemindersQuery(options.Value.ReminderPublishBatchSize);
		var reminders = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(reminders.Select(ApiMapper.ToDueResponse));
	}

	[ServicePassword]
	[HttpPut("{reminderId:int}/delivery")]
	public async Task<NoContent> ConfirmDelivery(
		[FromRoute, Range(1, int.MaxValue)] int reminderId,
		[FromBody, Required] ConfirmReminderDeliveryRequest request,
		[FromServices] ICommandHandler<MarkReminderDeliveredCommand> handler,
		CancellationToken ct
	)
	{
		var command = new MarkReminderDeliveredCommand(request.UserId, reminderId);
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}
}
