using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Auth.Login;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Auth.Register;

public sealed class RegisterHandler(
	IRepository<User> userRepository,
	IPasswordHasher passwordHasher,
	IUnitOfWork uow,
	ICommandHandler<LoginCommand, AuthTokensResult> loginHandler
) : ICommandHandler<RegisterCommand, AuthTokensResult>
{
	public async Task<AuthTokensResult> HandleAsync(RegisterCommand command, CancellationToken ct = default)
	{
		if (await userRepository.AnyAsync(u => u.Login == command.Login, ct))
			throw new ConflictException("User already exists.", "user_already_exists");

		var user = new User
		{
			Login = command.Login,
			PasswordHash = passwordHasher.Hash(command.Password),
			RegisteredAt = DateTime.UtcNow,
		};

		userRepository.Add(user);
		await uow.SaveChangesAsync(ct);

		var loginCommand = new LoginCommand(command.Login, command.Password, command.UserAgent);
		return await loginHandler.HandleAsync(loginCommand, ct);
	}
}
