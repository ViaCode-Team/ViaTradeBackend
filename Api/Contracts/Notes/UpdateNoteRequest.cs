using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Api.Contracts.Notes;

public record UpdateNoteRequest([StringLength(1024, MinimumLength = 1)] string Text);
