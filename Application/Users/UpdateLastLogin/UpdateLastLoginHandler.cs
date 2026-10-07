using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Users.Common.Abstractions;

namespace ViaTrade.Application.Users.UpdateLastLogin;

public sealed class UpdateLastLoginHandler(IUserRepository userRepository) : ICommandHandler<UpdateLastLoginCommand>
{
	public async Task HandleAsync(UpdateLastLoginCommand command, CancellationToken ct)
	{
		await userRepository.ExecuteUpdateLastLoginAtAsync(command.UserId, DateTime.UtcNow, ct);
	}
}
