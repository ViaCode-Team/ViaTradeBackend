using Application.Auth.Interfaces;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Users.Interfaces;
using Domain.Users.Entities;
using MediatR;

namespace Application.Auth.Commands;

public record RegisterCommand(string Login, string Password, string UserAgent) : ITransactionalCommand<AuthInternalResult>;

public class RegisterCommandHandler(
	IUserRepository userRepository, IPasswordHasher passwordHasher, ISender sender)
	: IRequestHandler<RegisterCommand, AuthInternalResult>
{
	public async Task<AuthInternalResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
	{
		if (await userRepository.ExistsAsync(u => u.Login == request.Login, cancellationToken))
			throw new InvalidOperationException("User already exists");

		var user = new User
		{
			Login = request.Login,
			HashPassword = passwordHasher.Hash(request.Password),
			RegisterDate = DateTime.UtcNow
		};

		await userRepository.AddAsync(user, cancellationToken);

		var loginCommand = new LoginCommand(request.Login, request.Password, request.UserAgent);
		return await sender.Send(loginCommand, cancellationToken);
	}
}
