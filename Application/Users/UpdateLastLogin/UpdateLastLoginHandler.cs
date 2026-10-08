using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Users.Common.Abstractions;

namespace ViaTrade.Application.Users.UpdateLastLogin;

public sealed class UpdateLastLoginHandler(IUserRepository userRepository) : IVoidCommandHandler<UpdateLastLoginCommand>
{
	public async ValueTask Handle(UpdateLastLoginCommand command, CancellationToken ct)
	{
		await userRepository.ExecuteUpdateLastLoginAtAsync(command.UserId, DateTime.UtcNow, ct);
	}
}
