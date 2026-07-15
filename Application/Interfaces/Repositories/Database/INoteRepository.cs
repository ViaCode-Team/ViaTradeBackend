using Domain.Entities.DataBase;
using Domain.Models.Dto.NoteRemind;
using Domain.Models.Pagination;

namespace Application.Interfaces.Repositories.Database;

public interface INoteRepository : IRepository<Note, NoteDto>
{
	Task<int> CountByUserAsync(int userId, CancellationToken cancellationToken = default);
	Task<int> CountByUserAndTypeAsync(int userId, NoteType noteType, CancellationToken cancellationToken);
	Task<PagedResult<NoteDto>> GetUserNoteByPropPagedAsync(int userId, NoteType noteType, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<Note> GetUserNoteByProp(int id, int userId, NoteType noteType, CancellationToken cancellationToken);
	Task AddUserNoteAsync(int relatedId, NoteType noteType, NoteDto dto, CancellationToken cancellationToken);
	Task UpdateUserNoteAsync(int id, NoteType noteType, NoteDto dto, CancellationToken cancellationToken);
	Task DeleteUserNoteAsync(int id, int userId, NoteType noteType, CancellationToken cancellationToken);
	Task<Note?> FindUserNoteByEntityAsync(int userId, int relatedId, NoteType noteType, CancellationToken cancellationToken);
}
