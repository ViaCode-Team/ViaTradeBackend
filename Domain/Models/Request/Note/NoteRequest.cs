using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Models.Note
{
    public class NoteRequest
    {
        [StringLength(1024)]
        public required string NoteText { get; set; }
    }
}
