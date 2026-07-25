using Application.Common.Exceptions;
using MySqlConnector;

namespace Infrastructure.DataBase;

internal static class DatabaseMySqlExceptionTranslator
{
	public static Exception? Translate(MySqlException exception)
	{
		return exception.ErrorCode switch
		{
			MySqlErrorCode.DuplicateKeyEntry => new ConflictException(
				"Resource already exists.",
				"resource_already_exists",
				exception
			),

			MySqlErrorCode.NoReferencedRow or MySqlErrorCode.NoReferencedRow2 => new NotFoundException(
				"Related resource not found.",
				"related_resource_not_found",
				exception
			),

			MySqlErrorCode.RowIsReferenced or MySqlErrorCode.RowIsReferenced2 => new ConflictException(
				"Resource is referenced by another entity.",
				"resource_is_referenced",
				exception
			),

			MySqlErrorCode.DataTooLong
			or MySqlErrorCode.DataOutOfRange
			or MySqlErrorCode.ColumnCannotBeNull
			or MySqlErrorCode.NoDefaultForField
			or MySqlErrorCode.InvalidJsonData
			or MySqlErrorCode.InvalidJsonBinaryData
			or MySqlErrorCode.InvalidUseOfNull
			or MySqlErrorCode.NullColumnInIndex => new BadRequestException(
				"The supplied data violates a database constraint.",
				"database_constraint_violation",
				exception
			),

			MySqlErrorCode.LockDeadlock
			or MySqlErrorCode.LockWaitTimeout
			or MySqlErrorCode.UserLockDeadlock
			or MySqlErrorCode.XARBDeadlock => new ServiceUnavailableException(
				"The database is busy. Retry the request.",
				"database_busy",
				exception
			),

			MySqlErrorCode.CannotExecuteInReadOnlyTransaction
			or MySqlErrorCode.ReadOnlyTransaction
			or MySqlErrorCode.OpenAsReadOnly
			or MySqlErrorCode.InnodbReadOnly => new ServiceUnavailableException(
				"The database is temporarily unavailable for writes.",
				"database_read_only",
				exception
			),

			MySqlErrorCode.ConnectionCountError or MySqlErrorCode.TooManyUserConnections =>
				new ServiceUnavailableException(
					"The database is overloaded. Retry the request later.",
					"database_overloaded",
					exception
				),

			MySqlErrorCode.LockTableFull => new ServiceUnavailableException(
				"The database has insufficient capacity.",
				"database_capacity_exceeded",
				exception
			),

			_ => null,
		};
	}
}
