using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Auth;

public record LoginRequest(
	[StringLength(64, MinimumLength = 1)] string Login,
	[StringLength(72, MinimumLength = 8)] string Password
);
