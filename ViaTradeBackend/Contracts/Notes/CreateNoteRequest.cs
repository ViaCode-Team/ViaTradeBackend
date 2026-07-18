using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Notes;

public record CreateNoteRequest(
	[StringLength(1024)] string NoteText
);



