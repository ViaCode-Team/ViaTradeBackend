using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using ViaTrade.Application.Email;
using ViaTrade.Domain.Entities;
using ViaTrade.Infrastructure.DataBase;
using ViaTrade.Infrastructure.DataBase.Interceptors;

namespace ViaTrade.Infrastructure.Email;

public sealed class EmailAccountStore(AppDbContext context) : IEmailAccountStore
{
	public Task<T> ExecuteAsync<T>(Func<IEmailAccountTransaction, Task<T>> operation, CancellationToken ct)
	{
		return context
			.Database.CreateExecutionStrategy()
			.ExecuteAsync(async () =>
			{
				context.ChangeTracker.Clear();
				using var suppression = SuppressExceptionTranslationScope.Create();
				await context.Database.OpenConnectionAsync(ct);
				var connection = context.Database.GetDbConnection();
				var lockName = "viatrade:email:" + EmailIdentity.Fingerprint(connection.Database)[..40];
				var locked = false;
				try
				{
					await using var command = connection.CreateCommand();
					command.CommandText = "SELECT GET_LOCK(@name, 10)";
					AddParameter(command, "@name", lockName);
					locked = Convert.ToInt32(await command.ExecuteScalarAsync(ct)) == 1;
					if (!locked)
						throw new TimeoutException("Could not acquire the email account lock.");

					await using var transaction = await context.Database.BeginTransactionAsync(
						IsolationLevel.ReadCommitted,
						ct
					);
					var result = await operation(new EmailAccountTransaction(context));
					await context.SaveChangesAsync(ct);
					await transaction.CommitAsync(ct);
					return result;
				}
				finally
				{
					try
					{
						if (connection.State == ConnectionState.Open)
						{
							await using var release = connection.CreateCommand();
							release.CommandText = "SELECT RELEASE_LOCK(@name)";
							AddParameter(release, "@name", lockName);
							await release.ExecuteScalarAsync(CancellationToken.None);
						}
					}
					catch
					{
						if (connection is MySqlConnection mysql)
							MySqlConnection.ClearPool(mysql);
						throw;
					}
					finally
					{
						await context.Database.CloseConnectionAsync();
					}
				}
			});
	}

	public async Task<IReadOnlyList<int>> ListMaintenanceUserIdsAsync(int afterUserId, int limit, CancellationToken ct)
	{
		return await context
			.Users.Where(user =>
				user.Id > afterUserId
				&& (
					user.EmailWorkStateJson != null
					|| user.PendingEmail != null
					|| user.PreviousEmail != null
					|| user.PasswordResetExpiresAt != null
					|| user.Email != null && user.EmailConfirmedAt == null
				)
			)
			.OrderBy(user => user.Id)
			.Select(user => user.Id)
			.Take(limit)
			.ToListAsync(ct);
	}

	private static void AddParameter(DbCommand command, string name, string value)
	{
		var parameter = command.CreateParameter();
		parameter.ParameterName = name;
		parameter.Value = value;
		command.Parameters.Add(parameter);
	}

	private sealed class EmailAccountTransaction(AppDbContext context) : IEmailAccountTransaction
	{
		public Task<User?> FindByUserIdAsync(int userId, CancellationToken ct) =>
			context.Users.FirstOrDefaultAsync(user => user.Id == userId, ct);

		public Task<User?> FindByIdentityAsync(string identity, CancellationToken ct)
		{
			if (identity.Contains('@'))
			{
				var email = EmailIdentity.NormalizeEmail(identity);
				return context.Users.FirstOrDefaultAsync(user => user.NormalizedEmail == email, ct);
			}

			var login = EmailIdentity.NormalizeLogin(identity);
			return context.Users.FirstOrDefaultAsync(user => user.NormalizedLogin == login, ct);
		}

		public Task<bool> IsEmailOccupiedAsync(string normalizedEmail, DateTime now, CancellationToken ct) =>
			context.Users.AnyAsync(
				user =>
					user.NormalizedEmail == normalizedEmail
					|| user.NormalizedPendingEmail == normalizedEmail && user.EmailChangeExpiresAt > now
					|| user.NormalizedPreviousEmail == normalizedEmail && user.EmailRecoveryExpiresAt > now,
				ct
			);

		public async Task AddAsync(User user, CancellationToken ct)
		{
			context.Users.Add(user);
			await context.SaveChangesAsync(ct);
		}

		public void Update(User user) => context.Users.Update(user);

		public void Delete(User user) => context.Users.Remove(user);
	}
}
