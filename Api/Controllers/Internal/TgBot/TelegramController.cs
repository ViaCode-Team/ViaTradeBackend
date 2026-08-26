using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Attribute;
using ViaTrade.Api.Contracts.Users;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Users.Interfaces;

namespace ViaTrade.Api.Controllers.Internal.TgBot;

[Route($"{ApiRoutes.V1.TgBot}/[controller]")]
[ApiExplorerSettings(GroupName = InternalServices.TgBot)]
[ApiController]
public class TelegramController(IUserCommandService userCommandService) : ControllerBase
{
	[ServicePassword]
	[HttpPost("links")]
	public async Task<Accepted> Link([FromBody, Required] LinkTelegramRequest request, CancellationToken ct)
	{
		await userCommandService.LinkTelegramAsync(request.TelegramToken, request.TelegramId, ct);

		return TypedResults.Accepted(string.Empty);
	}
}
