using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Api.Contracts.Users;

public record UserSessionResponse(
	[StringLength(36)] string Id,
	[Range(1, int.MaxValue)] int UserId,
	[StringLength(1024)] string UserAgent,
	DateTime CreatedAt,
	DateTime LastSeen,
	bool IsCurrent
);
