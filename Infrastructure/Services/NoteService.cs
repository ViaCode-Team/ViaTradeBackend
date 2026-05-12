using Domain.Entities.DataBase;
using Domain.Models.Dto;
using Infrastructure.Repositories.DataBase;
using Infrastructure.Repositoryes.DataBase;

namespace Infrastructure.Services
{
    public class NoteService(
        NoteRepository noteRepository,
        TradeCodeRepository tradeCodeRepository,
        TradeStrategyRepository tradeStrategyRepository)
    {
        private readonly NoteRepository _noteRepository = noteRepository;
        private readonly TradeCodeRepository _tradeCodeRepository = tradeCodeRepository;
        private readonly TradeStrategyRepository _tradeStrategyRepository = tradeStrategyRepository;

        public async Task ValidateRelatedEntityExistsAsync(int relatedId, NoteType noteType, CancellationToken cancellationToken)
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

        public async Task EnsureUserHasNoExistingNoteAsync(int userId, int relatedId, NoteType noteType, CancellationToken cancellationToken)
        {
            var existing = await _noteRepository.FindUserNoteByEntityAsync(userId, relatedId, noteType, cancellationToken);
            if (existing != null)
            { 
                throw new InvalidOperationException($"User already has a note assigned to this {noteType}");
            }
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
    }
}