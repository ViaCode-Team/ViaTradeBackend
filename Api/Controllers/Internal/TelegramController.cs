using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Attribute;
using ViaTrade.Api.Contracts.Users;
using ViaTrade.Api.Mappings;
using ViaTrade.Application.Users.Interfaces;

namespace ViaTrade.Api.Controllers.Internal;

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
