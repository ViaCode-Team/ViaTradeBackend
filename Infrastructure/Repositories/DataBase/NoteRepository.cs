using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto.NoteRemind;
using Domain.Models.Pagination;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase
{
    public class NoteRepository(AppDbContext context) : GenericRepository<Note, NoteDto>(context), INoteRepository
    {
        public async Task<int> CountByUserAsync(int userId, CancellationToken cancellationToken)
        {
            return await _dbSet.CountAsync(n => n.UserId == userId, cancellationToken);
        }

        public async Task<int> CountByUserAndTypeAsync(int userId, NoteType noteType, CancellationToken cancellationToken) => noteType switch
        {
            NoteType.TradeCodeNote => await _dbSet.CountAsync(n => n.UserId == userId && n.TradeCodeId != null, cancellationToken),
            NoteType.TradeStrategyNote => await _dbSet.CountAsync(n => n.UserId == userId && n.TradeStrategyId != null, cancellationToken),
            _ => throw new KeyNotFoundException()
        };

        public async Task<PagedResult<Note>> GetUserNoteByPropPagedAsync(int userId, NoteType noteType, PaginationRequest paginationRequest, CancellationToken cancellationToken) => noteType switch
        {
            NoteType.TradeCodeNote => await _dbSet
                .Where(n => n.UserId == userId && n.TradeCodeId != null)
                .ToPagedResultAsync(paginationRequest, cancellationToken),

            NoteType.TradeStrategyNote => await _dbSet
                .Where(n => n.UserId == userId && n.TradeStrategyId != null)
                .ToPagedResultAsync(paginationRequest, cancellationToken),

            _ => throw new KeyNotFoundException()
        };

        public async Task<Note?> FindUserNoteByEntityAsync(int userId, int relatedId, NoteType noteType, CancellationToken cancellationToken) => noteType switch
        {
            NoteType.TradeCodeNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeCodeId == relatedId && n.UserId == userId, cancellationToken),
            NoteType.TradeStrategyNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeStrategyId == relatedId && n.UserId == userId, cancellationToken),
            _ => null
        };

        public async Task<Note> GetUserNoteByProp(int id, int userId, NoteType noteType, CancellationToken cancellationToken)
        {
            Note? found = noteType switch

            {
                NoteType.TradeCodeNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeCodeId == id && n.UserId == userId, cancellationToken),
                NoteType.TradeStrategyNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeStrategyId == id && n.UserId == userId, cancellationToken),
                _ => throw new KeyNotFoundException()
            };

            return found
                ?? throw new KeyNotFoundException();
        }

        public async Task AddUserNoteAsync(int relatedId, NoteType noteType, NoteDto dto, CancellationToken cancellationToken)
        {
            var note = new Note
            {
                UserId = dto.UserId,
                NoteText = dto.NoteText,
                TradeCodeId = noteType == NoteType.TradeCodeNote ? relatedId : null,
                TradeStrategyId = noteType == NoteType.TradeStrategyNote ? relatedId : null
            };

            _dbSet.Add(note);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateUserNoteAsync(int id, NoteType noteType, NoteDto dto, CancellationToken cancellationToken)
        {
            var note = await ResolveNoteAsync(id, dto.UserId, noteType, cancellationToken)
                ?? throw new KeyNotFoundException();

            note.NoteText = dto.NoteText;
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteUserNoteAsync(int id, int userId, NoteType noteType, CancellationToken cancellationToken)
        {
            var note = await ResolveNoteAsync(id, userId, noteType, cancellationToken)
                ?? throw new KeyNotFoundException();

            _dbSet.Remove(note);
            await context.SaveChangesAsync(cancellationToken);
        }

        private async Task<Note?> ResolveNoteAsync(int id, int userId, NoteType noteType, CancellationToken cancellationToken) => noteType switch
        {
            NoteType.TradeCodeNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeCodeId == id && n.UserId == userId, cancellationToken),
            NoteType.TradeStrategyNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeStrategyId == id && n.UserId == userId, cancellationToken),
            _ => throw new KeyNotFoundException()
        };
    }
}
