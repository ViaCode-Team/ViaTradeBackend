using Domain.Entities;
using Domain.Users.Entities;

namespace Domain.Instruments.Entities;

public sealed class UserInstrument : BaseEntity<int>
{
	public required int UserId { get; set; }

	public required int InstrumentId { get; set; }

	public required User User { get; set; }

	public required Instrument Instrument { get; set; }
}
