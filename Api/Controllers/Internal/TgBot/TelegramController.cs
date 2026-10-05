using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Attribute;
using ViaTrade.Api.Contracts.Users;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Users.LinkTelegram;

namespace ViaTrade.Api.Controllers.Internal.TgBot;

[Route($"{ApiRoutes.V1.TgBot}/[controller]")]
[ApiExplorerSettings(GroupName = InternalServices.TgBot)]
[ApiController]
public class TelegramController : ControllerBase
{
	[ServicePassword]
	[HttpPost("links")]
	public async Task<Accepted> Link(
		[FromBody, Required] LinkTelegramRequest request,
		[FromServices] ICommandHandler<LinkTelegramCommand> handler,
		CancellationToken ct
	)
	{
		var command = new LinkTelegramCommand(request.TelegramToken, request.TelegramId);
		await handler.HandleAsync(command, ct);

		return TypedResults.Accepted(string.Empty);
	}
}
