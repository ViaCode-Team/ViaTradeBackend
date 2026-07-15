using Domain.Entities.DataBase;
using Domain.Models.Dto.NoteRemind;
using Domain.Models.Pagination;
using Domain.Models.Dto.Statistic;
using Domain.Interfaces;

namespace Application.Interfaces.Repositories.Database;

public interface INoteRepository : IRepository<Note, NoteDto>
{
	Task<NoteStatistic> GetNoteStatisticAsync(int userId, CancellationToken cancellationToken = default);
	Task<PagedResult<NoteDto>> GetPagedFilteredAsync(ISpecification<Note> spec, PaginationRequest? paginationRequest, CancellationToken cancellationToken);
	Task<Note> GetUserNoteByProp(int id, int userId, NoteType noteType, CancellationToken cancellationToken);
	Task AddUserNoteAsync(int relatedId, NoteType noteType, NoteDto dto, CancellationToken cancellationToken);
	Task UpdateUserNoteAsync(int id, NoteType noteType, NoteDto dto, CancellationToken cancellationToken);
	Task DeleteUserNoteAsync(int id, int userId, NoteType noteType, CancellationToken cancellationToken);
	Task<Note?> FindUserNoteByEntityAsync(int userId, int relatedId, NoteType noteType, CancellationToken cancellationToken);
}
