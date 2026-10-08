using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Routing;
using ViaTrade.Api.Security.Authorization;
using ViaTrade.Application.Users.LinkTelegram;

namespace ViaTrade.Api.Controllers.Internal.TgBot;

[Route($"{ApiRoutes.V1.TgBot}/[controller]")]
[ApiExplorerSettings(GroupName = InternalServices.TgBot)]
[ApiController]
public class TelegramController(ISender sender) : ControllerBase
{
	[ServicePassword]
	[HttpPost("links")]
	public async Task<Accepted> Link([FromBody] LinkTelegramCommand command, CancellationToken ct)
	{
		await sender.Send(command, ct);

		return TypedResults.Accepted(string.Empty);
	}
}
