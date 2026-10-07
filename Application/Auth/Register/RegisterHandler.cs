using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Auth.Register;

public sealed class RegisterHandler(IRepository<User> userRepository, IPasswordHasher passwordHasher, IUnitOfWork uow)
	: ICommandHandler<RegisterCommand>
{
	public async Task HandleAsync(RegisterCommand command, CancellationToken ct)
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
	}
}
