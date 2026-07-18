using Application.Users.Interfaces;
using MediatR;
using System.Security.Cryptography;

namespace Application.Users.Commands;

public record GenerateTgLinkCommand(int UserId) : IRequest<string>;

public class GenerateTgLinkCommandHandler(ITgTokenRepository tgTokenRepository)
	: IRequestHandler<GenerateTgLinkCommand, string>
{
	private readonly ITgTokenRepository _tgTokenRepository = tgTokenRepository;

	public async Task<string> Handle(GenerateTgLinkCommand request, CancellationToken cancellationToken)
	{
		var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
			.Replace("+", "-")
			.Replace("/", "_")
			.TrimEnd('=');

		await _tgTokenRepository.SetAsync(token, request.UserId, TimeSpan.FromMinutes(5));

		return $"https://t.me/ViaTradeBot?start={token}";
	}
}
