using Application.Common.Interfaces;
using Application.Users.Interfaces;
using MediatR;

namespace Application.Users.Commands;

public record UpdateLastLoginDateCommand(int UserId) : ICommand;

public class UpdateLastLoginDateCommandHandler(IUserRepository userRepository)
	: IRequestHandler<UpdateLastLoginDateCommand>
{
	public async Task Handle(UpdateLastLoginDateCommand request, CancellationToken ct)
	{
		await userRepository.UpdateLastLoginDateAsync(request.UserId, DateTime.UtcNow, ct);
	}
}
