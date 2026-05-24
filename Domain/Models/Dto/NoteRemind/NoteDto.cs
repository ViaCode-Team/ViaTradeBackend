using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Dto.NoteRemind
{
    public class NoteDto
    {
        [Required]
        public required int UserId { get; set; }
        [Required]
        [StringLength(1024)]
        public required string NoteText { get; set; }

        public int? TradeCodeId { get; set; }
        public int? TradeStrategyId { get; set; }   
    }
}