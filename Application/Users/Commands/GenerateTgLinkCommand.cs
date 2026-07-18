using Application.Interfaces.Repositories.Redis;
using Domain.Entities.Redis;
using MediatR;
using System.Security.Cryptography;

namespace Application.Users.Commands;

public record GenerateTgLinkCommand(int UserId) : IRequest<string>;

public class GenerateTgLinkCommandHandler(IRedisRepository<TgTokenEntity> tgTokenRepository) 
	: IRequestHandler<GenerateTgLinkCommand, string>
{
	private readonly IRedisRepository<TgTokenEntity> _tgTokenRepository = tgTokenRepository;

	public async Task<string> Handle(GenerateTgLinkCommand request, CancellationToken cancellationToken)
	{
		var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
			.Replace("+", "-")
			.Replace("/", "_")
			.TrimEnd('=');

		var entity = new TgTokenEntity
		{
			Id = token,
			UserId = request.UserId
		};

		await _tgTokenRepository.SetAsync(entity, TimeSpan.FromMinutes(5));

		return $"https://t.me/ViaTradeBot?start={token}";
	}
}
