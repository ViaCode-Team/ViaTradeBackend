using Application.Auth.Interfaces;
using Application.Common.Models;
using Application.Users.Interfaces;
using Domain.Users.Entities;
using MediatR;

namespace Application.Auth.Commands;

public record RegisterCommand(string Login, string Password, string UserAgent) : IRequest<AuthInternalResult>;

public class RegisterCommandHandler(
	IUserRepository userRepository,
	IPasswordHasher passwordHasher,
	ISender sender)
	: IRequestHandler<RegisterCommand, AuthInternalResult>
{
	private readonly IUserRepository _userRepository = userRepository;
	private readonly IPasswordHasher _passwordHasher = passwordHasher;
	private readonly ISender _sender = sender;

	public async Task<AuthInternalResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
	{
		if (await _userRepository.ExistsAsync(u => u.Login == request.Login, cancellationToken))
			throw new InvalidOperationException("User already exists");

		var user = new User(request.Login, _passwordHasher.Hash(request.Password), DateTime.UtcNow);

		await _userRepository.AddAsync(user, cancellationToken);
		await _userRepository.SaveChangesAsync(cancellationToken);

		var loginCommand = new LoginCommand(request.Login, request.Password, request.UserAgent);
		return await _sender.Send(loginCommand, cancellationToken);
	}
}
