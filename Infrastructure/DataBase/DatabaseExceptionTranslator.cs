using Application.Common.Exceptions;
using MySqlConnector;

namespace Infrastructure.DataBase;

internal static class DatabaseExceptionTranslator
{
	public static Exception? Translate(MySqlException exception)
	{
		return exception.ErrorCode switch
		{
			MySqlErrorCode.DuplicateKeyEntry => new ConflictException(
				"Resource already exists.",
				"resource_already_exists"
			),

			MySqlErrorCode.NoReferencedRow or MySqlErrorCode.NoReferencedRow2 => new NotFoundException(
				"Related resource not found.",
				"related_resource_not_found"
			),

			MySqlErrorCode.RowIsReferenced or MySqlErrorCode.RowIsReferenced2 => new ConflictException(
				"Resource is referenced by another entity.",
				"resource_is_referenced"
			),

			_ => null,
		};
	}
}
