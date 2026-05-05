using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Entities.DataBase
{
    public class NoteType: BaseEntity
    {
        [Required]
        [MaxLength(64)]
        public required string TypeName { get; set; }

        [JsonIgnore]
        public ICollection<Note>? Notes { get; set; }
    }
}
