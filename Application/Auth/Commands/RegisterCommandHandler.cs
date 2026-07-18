using Application.Interfaces.Repositories.Database;
using Application.Interfaces.Utils;
using Application.Models;
using Domain.Users.Entities;
using MediatR;

namespace Application.Auth.Commands;

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

		var loginCommand = new LoginCommand(request.Login, request.Password, "initial");
		return await _sender.Send(loginCommand, cancellationToken);
	}
}
