using Application.Users.Interfaces;
using MediatR;

namespace Application.Users.Commands;

public record UpdateLastLoginDateCommand(int UserId) : IRequest;

public class UpdateLastLoginDateCommandHandler(IUserRepository userRepository)
	: IRequestHandler<UpdateLastLoginDateCommand>
{
	private readonly IUserRepository _userRepository = userRepository;

	public async Task Handle(UpdateLastLoginDateCommand request, CancellationToken cancellationToken)
	{
		await _userRepository.UpdateLastLoginDateAsync(request.UserId, DateTime.UtcNow, cancellationToken);
	}
}
