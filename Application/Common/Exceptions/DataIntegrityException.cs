namespace ViaTrade.Application.Common.Exceptions;

public sealed class DataIntegrityException(
	string message = "Inconsistent server data was detected.",
	string code = "data_integrity_error"
) : AppException(message, code) { }
