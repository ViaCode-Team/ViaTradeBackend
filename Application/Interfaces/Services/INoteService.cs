using Domain.Entities.DataBase;
using Domain.Models.Dto.NoteRemind;
using Domain.Models.Dto.Statistic;

namespace Application.Interfaces
{
    public interface INoteService
    {
        Task<NoteStatistic> GetNoteStatisticAsync(int userId, CancellationToken cancellationToken);
        Task<IEnumerable<Note>> GetUserNoteByPropAllAsync(int userId, NoteType noteType, CancellationToken cancellationToken);
        Task<Note> GetUserNoteByPropAsync(int id, int userId, NoteType noteType, CancellationToken cancellationToken);
        Task AddUserNoteWithValidationAsync(int relatedId, NoteType noteType, NoteDto dto, CancellationToken cancellationToken);
        Task UpdateUserNoteWithValidationAsync(int relatedId, NoteType noteType, NoteDto dto, CancellationToken cancellationToken);
        Task DeleteUserNoteAsync(int id, int userId, NoteType noteType, CancellationToken cancellationToken);
    }
}
