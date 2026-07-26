using System.ComponentModel.DataAnnotations;
using Application.Users.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Attribute;
using ViaTradeBackend.Contracts.Auth;
using ViaTradeBackend.Contracts.Users;
using ViaTradeBackend.Mappings;

namespace ViaTradeBackend.Controllers.Internal;

[Route("api/v1/internal/[controller]")]
[ApiExplorerSettings(GroupName = "internal")]
[ApiController]
public class TelegramController(IUserCommandService userCommandService, IUserQueryService userQueryService)
	: ControllerBase
{
	[ServicePassword]
	[HttpPost("links")]
	public async Task<Accepted> Link([FromBody, Required] LinkTelegramRequest request, CancellationToken ct)
	{
		await userCommandService.LinkTelegramAsync(request.TelegramToken, request.TelegramId, ct);

		return TypedResults.Accepted(string.Empty);
	}

	[ServicePassword]
	[HttpGet("recipients")]
	public async Task<Ok<List<UserTelegramResponse>>> GetRecipients(CancellationToken ct)
	{
		var users = await userQueryService.ListTelegramRecipientsAsync(ct);

		return TypedResults.Ok(users.Select(ApiMapper.ToResponse).ToList());
	}
}
