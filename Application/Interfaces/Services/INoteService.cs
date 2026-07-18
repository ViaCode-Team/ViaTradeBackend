using Application.Contracts.Dto.NoteRemind;
using Application.Contracts.Dto.Statistic;
using Domain.Entities.DataBase;
using Domain.Models.Filters;
using Domain.Models.Pagination;

namespace Application.Interfaces;

public interface INoteService
{
	Task<NoteStatisticDto> GetNoteStatisticAsync(int userId, CancellationToken cancellationToken);
	Task<PagedResult<NoteDto>> GetUserNotePagedAsync(int userId, NoteFilterRequest? filterRequest, PaginationRequest? paginationRequest, CancellationToken cancellationToken);
	Task<Note> GetUserNoteByPropAsync(int id, int userId, NoteType noteType, CancellationToken cancellationToken);
	Task AddUserNoteWithValidationAsync(int relatedId, NoteType noteType, NoteDto dto, CancellationToken cancellationToken);
	Task UpdateUserNoteWithValidationAsync(int relatedId, NoteType noteType, NoteDto dto, CancellationToken cancellationToken);
	Task DeleteUserNoteAsync(int id, int userId, NoteType noteType, CancellationToken cancellationToken);
}
