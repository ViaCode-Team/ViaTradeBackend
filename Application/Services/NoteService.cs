using Application.Contracts.Dto.NoteRemind;
using Application.Contracts.Dto.Statistic;
using Application.Interfaces;
using Application.Interfaces.Repositories.Database;
using Application.Specifications;
using Domain.Entities.DataBase;
using Domain.Models.Filters;
using Domain.Models.Pagination;

namespace Application.Services;

public class NoteService(
	INoteRepository noteRepository,
	ITradeCodeRepository tradeCodeRepository,
	ITradeStrategyRepository tradeStrategyRepository) : INoteService
{
	private readonly INoteRepository _noteRepository = noteRepository;
	private readonly ITradeCodeRepository _tradeCodeRepository = tradeCodeRepository;
	private readonly ITradeStrategyRepository _tradeStrategyRepository = tradeStrategyRepository;

	public async Task<NoteStatisticDto> GetNoteStatisticAsync(int userId, CancellationToken cancellationToken)
	{

		return await _noteRepository.GetNoteStatisticAsync(userId, cancellationToken);
	}

	public async Task<PagedResult<NoteDto>> GetUserNotePagedAsync(int userId, NoteFilterRequest? filterRequest, PaginationRequest? paginationRequest, CancellationToken cancellationToken)
	{

		var spec = new NoteQuerySpecification(userId, filterRequest);
		return await _noteRepository.GetPagedFilteredAsync(spec, paginationRequest, cancellationToken);
	}

	public async Task<Note> GetUserNoteByPropAsync(int id, int userId, NoteType noteType, CancellationToken cancellationToken)
	{

		return await _noteRepository.GetUserNoteByProp(id, userId, noteType, cancellationToken);
	}

	public async Task AddUserNoteWithValidationAsync(int relatedId, NoteType noteType, NoteDto dto, CancellationToken cancellationToken)
	{
		await ValidateRelatedEntityExistsAsync(relatedId, noteType, cancellationToken);
		await EnsureUserHasNoExistingNoteAsync(dto.UserId, relatedId, noteType, cancellationToken);
		await _noteRepository.AddUserNoteAsync(relatedId, noteType, dto, cancellationToken);
	}

	public async Task UpdateUserNoteWithValidationAsync(int relatedId, NoteType noteType, NoteDto dto, CancellationToken cancellationToken)
	{
		await ValidateRelatedEntityExistsAsync(relatedId, noteType, cancellationToken);
		await _noteRepository.UpdateUserNoteAsync(relatedId, noteType, dto, cancellationToken);
	}

	public async Task DeleteUserNoteAsync(int id, int userId, NoteType noteType, CancellationToken cancellationToken)
	{

		await _noteRepository.DeleteUserNoteAsync(id, userId, noteType, cancellationToken);
	}

	private async Task ValidateRelatedEntityExistsAsync(int relatedId, NoteType noteType, CancellationToken cancellationToken)
	{
		var exists = noteType switch
		{
			NoteType.TradeCodeNote => await _tradeCodeRepository.GetByIdAsync(relatedId, cancellationToken) != null,
			NoteType.TradeStrategyNote => await _tradeStrategyRepository.GetByIdAsync(relatedId, cancellationToken) != null,
			_ => false
		};

		if (!exists)
		{
			throw new KeyNotFoundException($"Related entity of type {noteType} with id {relatedId} not found");
		}
	}

	private async Task EnsureUserHasNoExistingNoteAsync(int userId, int relatedId, NoteType noteType, CancellationToken cancellationToken)
	{
		var existing = await _noteRepository.FindUserNoteByEntityAsync(userId, relatedId, noteType, cancellationToken);
		if (existing != null)
			throw new InvalidOperationException($"User already has a note assigned to this {noteType}");
	}
}
