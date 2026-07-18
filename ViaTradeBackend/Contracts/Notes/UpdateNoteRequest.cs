using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Notes;

public record UpdateNoteRequest(
	[StringLength(1024)] string NoteText
);



